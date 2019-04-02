using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class NavmeshAgent2D : MonoBehaviour {
    #region Editor Variables
    public float width;
    public float height;
    public float crouchHeight;
    public float jumpDistance;
    public float speed;
    public float maxSpeed;
    public float maxReach;
    public float stoppingDistance;
    public float pathAccuracy = 0.2f;
    #endregion

    #region Movement Bools
    public bool isGrounded;
    public bool canGrab = true;
    public bool isProne;
    public bool sprinting = false;
    public bool isDead;
    #endregion

    public List<NavmeshNode2D> path = new List<NavmeshNode2D>();
    new public Rigidbody2D rigidbody;
    public Ladder ladder;
    public NavmeshNode2D ledge;

    public bool isStopped;
    public bool pathing;

    protected CapsuleCollider2D capsuleCollider;
    protected NavmeshArea2D area;

    protected bool wasCrouched = false;
    protected bool canWalkGrab = false;
    public bool canMove = true;

    private float lastPath;

    #region Testing Variables
    protected Transform _sprite;
    protected float _initSpriteHeight;
    #endregion

    protected virtual void Update() {
        
    }

    protected virtual void FixedUpdate() {
        _sprite.localScale = capsuleCollider.size;

        Orient();
        GroundCheck();
    }

    protected virtual void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        area = FindObjectOfType<NavmeshArea2D>();
        path = new List<NavmeshNode2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        crouchHeight = height / 2;
        SetSize(new Vector2(width, height));

        _sprite = transform.Find("Sprite");
        _initSpriteHeight = _sprite.localScale.y;
    }

    public void SetSize(Vector2 newSize) {
        if (newSize.y < newSize.x) { capsuleCollider.direction = CapsuleDirection2D.Horizontal; }
        else { capsuleCollider.direction = CapsuleDirection2D.Vertical; }

        capsuleCollider.size = newSize;
        capsuleCollider.offset = new Vector2(0f, capsuleCollider.size.y / 2);
    }

    public void MoveTo(Vector2 position, UnityEngine.Events.UnityAction callback) {
        StartCoroutine(MoveToEnumerator(position, callback));
    }

    public NavmeshNode2D GetClosestLedge() {
        if (area == null) { return null; }
        List<NavmeshNode2D> ledges = area.NodesOfTypeInRange(this, transform.position, new List<NavmeshNode2D.NodeType> { NavmeshNode2D.NodeType.Ledge }, maxReach * transform.localScale.x);
        if (ledges == null) { return null; }
        isProne = false;

        NavmeshNode2D closest = ledges[0];
        float closestDistance = Mathf.Infinity;
        foreach (NavmeshNode2D ledge in ledges)
        {
            float distance = Vector2.Distance(transform.position, ledge.worldPosition);
            if (distance <= closestDistance) { closest = ledge; closestDistance = distance; }
        }

        return closest;
    }

    public bool LedgeNearby() {
        if (GetClosestLedge() != null) { return true; }
        else { return false; }
    }

    public virtual void GrabLedge() {
        if (area == null) { return; }
        if (!canGrab || ladder) { return; }
        NavmeshNode2D closest = GetClosestLedge();

        canMove = false;
        canGrab = false;
        ledge = closest;
        //Todo: change to hanging animation
        MoveTo(closest.worldPosition, () =>
        {
            canMove = true;
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            canGrab = true;
        });
    }

    public virtual void ClimbLedge() {
        if (ledge == null) { return; }

        List<NavmeshNode2D.NavmeshNodeConnection2D> surfaces = new List<NavmeshNode2D.NavmeshNodeConnection2D>();
        ledge.ConnectedToTypes(new List<NavmeshNode2D.NodeType> { NavmeshNode2D.NodeType.Walkable, NavmeshNode2D.NodeType.Crawlable}, out surfaces);
        RaycastHit2D surface = Physics2D.Raycast(surfaces[0].b.worldPosition, Vector2.down, area.resolution, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawLine(surfaces[0].b.worldPosition, surface.point, Color.red, 3f);

        if (surface)
        {
            Debug.Log("Climbing Ledge...");
            
            MoveTo(new Vector2(surface.point.x, surface.point.y + (height/2)), () => {
                ledge = null;
                rigidbody.AddForce((new Vector2(Input.GetAxisRaw("Horizontal"), .1f)));
            });
        }
        else { Debug.LogWarning("Can not climb this ledge."); }
    }

    public virtual void ReleaseLedge() {
        Debug.Log("Releasing ledge");
        if (ledge == null) { return; }

        ledge = null;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    public Vector2 GetSize() {
        return new Vector2(width * transform.localScale.x, height * transform.localScale.y);
    }

    public void DismountLadder() {
        if (!ladder) { Debug.LogWarning("Could not dismount ladder because it does not exist."); return; }
        if (ladder.CheckActorCollisions(this) > 0) { Debug.LogWarning("Could not dismount ladder because player is inside terrain!", this); return; }

        if (ladder.GetComponent<Rigidbody2D>()) {
            rigidbody.velocity = ladder.GetComponent<Rigidbody2D>().velocity;
        }

        ladder = null;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    public void LadderMountDismount(float radius) {
        if (!ladder) { MountNearestLadder(radius); }
        else { DismountLadder(); }
    }

    public Ladder GetNearestLadder(float radius) {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, 1 << LayerMask.NameToLayer("Ladder"));
        Collider2D closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in colliders)
        {
            if (closest == null)
            {
                closest = collider;
                closestDistance = Vector2.Distance(collider.transform.position, transform.position);
                continue;
            }

            float distance = Vector2.Distance(collider.transform.position, transform.position);
            if (distance <= closestDistance)
            {
                closest = collider;
                closestDistance = distance;
            }
        }

        if (closest) { return closest.GetComponent<Ladder>(); }
        else { return null; }
    }

    public bool LadderNearby() {
        if (GetNearestLadder(maxReach)) { return true; }
        else { return false; }
    }

    public void MountNearestLadder(float radius) {
        Ladder newLadder = GetNearestLadder(radius);

        if (newLadder) { newLadder.MountLadder(this); }
    }

    protected virtual void Crouch() {
        if (isProne)
        {
            SetSize(new Vector2(width, height / 2));
        }
        else {
            SetSize(new Vector2(width, height));
        }
    }

    public virtual float GetWalkDirection() {
        if (!pathing || path.Count == 0) { return 0; }
        return GetWalkVector().x;
    }

    public virtual Vector2 GetWalkVector() {
        if (!pathing || path.Count == 0) { return Vector2.zero; }

        NavmeshNode2D targetNode = GetTargetNodeInPath();
        Vector2 heading = targetNode.worldPosition - transform.position;
        float distance = heading.magnitude;
        Vector2 direction = heading / distance;
        direction = new Vector2(Mathf.Clamp(Mathf.RoundToInt(direction.x), -1, 1), Mathf.Clamp(Mathf.RoundToInt(direction.y), -1, 1));

        return direction;
    }

    public void FindPathTo(Vector3 position, int iterations) {
        path = GetPath(transform.position, position, iterations);
    }

    public int PathIndexOf(NavmeshNode2D node) {
        for (int i = 0; i < path.Count; i++) {
            if (path[i]== node) { return i; }
        }

        return -1;
    }

    public virtual NavmeshNode2D GetTargetNodeInPath() {
        NavmeshNode2D closestNode = GetClosestNodeInPath();
        NavmeshNode2D targetNode = closestNode;
        if (path.Count == 0 || closestNode == null) {
            return null;
        }

        for (int i = path.IndexOf(closestNode); i < path.Count; i++) {
            if (Vector2.Distance(path[i].worldPosition, transform.position) <= stoppingDistance) { targetNode = path[i]; }
        }

        return targetNode;
    }

    public virtual NavmeshNode2D GetClosestNodeInPath() {
        if (path.Count == 0) { return null; }

        NavmeshNode2D currentNode = area.NodeAtPoint(transform.position, this);
        NavmeshNode2D nearest = path[0];
        float nearestDistance = Vector2.Distance(nearest.worldPosition, transform.position);
        foreach (NavmeshNode2D pnode in path) {
            float distance = Vector2.Distance(pnode.worldPosition, transform.position);
            if (distance < nearestDistance) { nearest = pnode; nearestDistance = distance; }
        }

        return nearest;
    }

    protected virtual Transform GetGround() {
        if (!isGrounded) { return null; }

        RaycastHit2D ground = Physics2D.Raycast(transform.position, Vector2.down, 0.02f, 1 << LayerMask.NameToLayer("Environment"));

        if (ground)
        {
            return ground.transform;
        }
        else { return null; }
    }

    protected virtual List<NavmeshNode2D> GetPath(Vector2 start, Vector2 end, int maxIterations = 1000) {
        if (!pathing || isStopped) { return path; }

        if (Time.realtimeSinceStartup <= lastPath + pathAccuracy) { return path; }
        else { Debug.Log("Updating AI", this); }

        List<NavmeshNode2D> closedList = new List<NavmeshNode2D>();
        List<NavmeshNode2D> openList = new List<NavmeshNode2D>();
        NavmeshNode2D startNode = area.NodeAtPoint(start, this);
        NavmeshNode2D endNode = area.NodeAtPoint(end, this);

        openList.Add(startNode);
        NavmeshNode2D currentNode = GetBestNode(openList);

        int iterations = 0;

        UnityEngine.Profiling.Profiler.BeginSample("Finding Path", this);
        while (iterations < maxIterations) {
            currentNode = GetBestNode(openList);
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == endNode) { break; }

            UnityEngine.Profiling.Profiler.BeginSample("viewing neighbors" + currentNode.connections.Count, this);
            foreach (NavmeshNode2D.NavmeshNodeConnection2D connection in currentNode.connections) {

                UnityEngine.Profiling.Profiler.BeginSample("traversibility", this);
                if (!NodeIsTraversible(connection.b) || closedList.Contains(connection.b)) { UnityEngine.Profiling.Profiler.EndSample(); continue; }
                UnityEngine.Profiling.Profiler.EndSample();

                UnityEngine.Profiling.Profiler.BeginSample("Cost Calculation", this);
                float newCostToNeighbor = currentNode.gcost + GCost(currentNode, connection.b);
                if (newCostToNeighbor < connection.b.gcost || !openList.Contains(connection.b)) {
                    TotalCost(connection, endNode);
                    connection.b.parent = currentNode;

                    if (!openList.Contains(connection.b)) { openList.Add(connection.b); }
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }
            UnityEngine.Profiling.Profiler.EndSample();

            iterations++;
        }
        UnityEngine.Profiling.Profiler.EndSample();

        return RetracePath(startNode, currentNode);
    }

    private NavmeshNode2D FindTraversibleNeighbor(NavmeshNode2D node) {
        if (NodeIsTraversible(node)) { return node; }

        foreach (NavmeshNode2D.NavmeshNodeConnection2D connection in node.connections) {
            if (NodeIsTraversible(connection.b)) { return connection.b; }
        }

        return null;
    }

    private List<NavmeshNode2D> RetracePath(NavmeshNode2D start, NavmeshNode2D end) {
        List<NavmeshNode2D> path = new List<NavmeshNode2D>();
        NavmeshNode2D currentNode = end;

        int count = 0;
        while (currentNode!= start && count < 1000) {
            if (currentNode.parent == null) {
                break;
            }

            path.Add(currentNode);
            currentNode = currentNode.parent;
            count++;
        }

        if (count >= 1000) { Debug.LogWarning("Limit Reached for path"); }

        path.Reverse();
        lastPath = Time.realtimeSinceStartup;
        return path;
    }

    protected virtual bool NodeIsTraversible(NavmeshNode2D node) {
        UnityEngine.Profiling.Profiler.BeginSample("Traversibility calculations", this);

        if (node.type == NavmeshNode2D.NodeType.None || 
            node.type == NavmeshNode2D.NodeType.Air)
        { UnityEngine.Profiling.Profiler.EndSample(); return false; }
        else { UnityEngine.Profiling.Profiler.EndSample(); return true; }
    }

    protected virtual NavmeshNode2D GetBestNeighbor(NavmeshNode2D node, NavmeshNode2D end) {
        NavmeshNode2D best = node.connections[0].b;
        foreach (NavmeshNode2D.NavmeshNodeConnection2D edge in node.connections) {
            TotalCost(edge, end);
            if (edge.b.fcost < best.fcost) { best = edge.b; }
        }

        return best;
    }

    protected virtual NavmeshNode2D GetBestNode(List<NavmeshNode2D> nodes) {
        NavmeshNode2D best = nodes[0];

        foreach (NavmeshNode2D node in nodes) {
            if (node.fcost < best.fcost) { best = node; }
        }

        return best;
    }

    protected virtual float GCost(NavmeshNode2D a, NavmeshNode2D b) {
        float x = Mathf.Abs(b.gridPosition.x - a.gridPosition.x);
        float y = Mathf.Abs(b.gridPosition.y - a.gridPosition.y);

        if (y < x)
        {
            return 14 * y + (10 * (x - y));
        }
        else
        { return 14 * x + (10 * (y - x)); }
    }

    protected virtual float TotalCost(NavmeshNode2D.NavmeshNodeConnection2D edge, NavmeshNode2D end)
    {
        edge.b.gcost = GCost(edge.a, edge.b) * (edge.b.GetWeight() + edge.a.GetWeight());
        edge.b.hcost = GCost(edge.b, end);
        edge.b.fcost = edge.b.gcost + edge.b.hcost;
        return edge.b.fcost;
    }

    [System.Obsolete("Use 'GCost' instead")]
    protected virtual float Heuristic(NavmeshNode2D n, NavmeshNode2D end) {
        return Mathf.Abs(end.worldPosition.x - n.worldPosition.x) + Mathf.Abs(end.worldPosition.y - n.worldPosition.y);
    }

    protected virtual void Orient() {
        if (ladder)
        {
            if (rigidbody.velocity.x != 0) {
                float direction = Mathf.Abs(rigidbody.velocity.x) / rigidbody.velocity.x;
                _sprite.localScale = new Vector3(direction * Mathf.Abs(_sprite.localScale.x), _sprite.localScale.y);
            }
            transform.up = Vector2.Lerp(transform.up, ladder.GetAveragedUp(), Time.deltaTime * 10).normalized;
        }
        else {
            //Later: Get normal vector of the ground undernieth and set the tran's up to that.
            //transform.up = Vector3.up;

            transform.up = Vector2.up;
        }
    }

    protected void GroundCheck() {
        Collider2D ground = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y ), new Vector2(GetSize().x/2, 0.04f), 0f, 1 << LayerMask.NameToLayer("Environment"));

        if (ground) {
            isGrounded = true;
            canWalkGrab = true;
        }
        else { isGrounded = false; }
    }

    private IEnumerator MoveToEnumerator(Vector2 position, UnityEngine.Events.UnityAction callback) {
        rigidbody.velocity = Vector2.zero;
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        if (pathing) { isStopped = true; }

        for (float i = 0; i < 0.2; i += Time.deltaTime) {
            transform.position = Vector3.Lerp(transform.position, position, i);
            yield return new WaitForEndOfFrame();
        }

        transform.position = position;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        if (pathing) { isStopped = false; }
        callback();
    }

    void DrawGroundedBox() {
        Vector2 bottomLeft = new Vector2(transform.position.x - transform.localScale.x / 4 , transform.position.y - 0.2f);
        Vector2 bottomRight = new Vector2(transform.position.x + transform.localScale.x / 4, transform.position.y - 0.2f);
        Vector2 topRight = new Vector2(transform.position.x + transform.localScale.x / 4, transform.position.y + 0.2f);
        Vector2 topLeft = new Vector2(transform.position.x - transform.localScale.x / 4, transform.position.y + 0.2f);

        Debug.DrawLine(bottomLeft, bottomRight, Color.yellow);
        Debug.DrawLine(bottomRight, topRight, Color.yellow);
        Debug.DrawLine(bottomLeft, topLeft, Color.yellow);
        Debug.DrawLine(topLeft, topRight, Color.yellow);
    }

    private void OnDrawGizmos() {
        if (!capsuleCollider) { capsuleCollider = GetComponent<CapsuleCollider2D>(); }
        if (!Application.isPlaying)
        {
            SetSize(new Vector2(width, height));
        }
    }

    protected virtual void OnDrawGizmosSelected() {
        if (!area) { area = FindObjectOfType<NavmeshArea2D>(); }

        if (Application.isPlaying && path.Count >0)
        {
            Gizmos.DrawCube(area.NodeAtPoint(transform.position, this).worldPosition, Vector3.one / 4);
            for (int i = 1; i < path.Count; i++)
            {
                Debug.DrawLine(path[i - 1].worldPosition, path[i].worldPosition, Color.green);
            }
        }

        DrawGroundedBox();
    }
}

