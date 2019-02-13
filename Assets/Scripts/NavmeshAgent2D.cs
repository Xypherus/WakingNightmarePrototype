using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class NavmeshAgent2D : MonoBehaviour {
    public float width;
    public float height;
    public float crouchHeight;
    public float jumpDistance;
    public float speed;
    public float maxSpeed;
    public float maxReach;

    public bool isGrounded;
    public bool canGrab = true;
    public bool isProne = false;
    public bool sprinting = false;

    public List<NavmeshNode2D> path = new List<NavmeshNode2D>();
    new public Rigidbody2D rigidbody;
    public Ladder ladder;
    public NavmeshNode2D ledge;

    public bool isStopped;
    public bool pathing;

    protected CapsuleCollider2D capsuleCollider;

    protected NavmeshArea2D area;

    bool wasCrouched = false;

    #region Testing Variables
    protected Transform _sprite;
    protected float _initSpriteHeight;
    #endregion

    protected virtual void Update() {
        
    }

    public void MoveTo(Vector2 position, UnityEngine.Events.UnityAction callback) {
        StartCoroutine(MoveToEnumerator(position, callback));
    }

    public virtual void GrabLedge() {
        if (area == null) { return; }
        if (!canGrab || ladder) { return; }
        List<NavmeshNode2D> ledges = area.NodesOfTypeInRange(this, transform.position, new List<NavmeshNode2D.NodeType> { NavmeshNode2D.NodeType.Ledge}, maxReach*transform.localScale.x);
        if (ledges == null) { return; }
        isProne = false;

        canGrab = false;
        NavmeshNode2D closest = ledges[0];
        float closestDistance = Mathf.Infinity;
        foreach (NavmeshNode2D ledge in ledges) {
            float distance = Vector2.Distance(transform.position, ledge.worldPosition);
            if ( distance <= closestDistance) { closest = ledge; closestDistance = distance; }
        }

        //Todo: change to hanging animation
        MoveTo(closest.worldPosition, () =>
        {
            ledge = closest;
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
                rigidbody.AddForce(300 * (new Vector2(Input.GetAxisRaw("Horizontal"), .1f)));
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
        if (ladder.CheckActorCollisions(this) > 0) { Debug.LogWarning("Could not dismount ladder because player is inside terrain!"); return; }

        if (ladder.GetComponent<Rigidbody2D>()) {
            rigidbody.velocity = ladder.GetComponent<Rigidbody2D>().velocity;
        }

        ladder = null;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        transform.parent = null;

    }

    public void LadderMountDismount(float radius) {
        if (!ladder) { MountNearestLadder(radius); }
        else { DismountLadder(); }
    }

    public void MountNearestLadder(float radius) {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, 1 << LayerMask.NameToLayer("Ladder"));
        Collider2D closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in colliders) {
            if (closest == null) {
                closest = collider;
                closestDistance = Vector2.Distance(collider.transform.position, transform.position);
                continue;
            }

            float distance = Vector2.Distance(collider.transform.position, transform.position);
            if (distance <= closestDistance) {
                closest = collider;
                closestDistance = distance;
            }
        }

        if (closest != null) { closest.GetComponent<Ladder>().MountLadder(this); }
    }

    protected virtual void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        area = FindObjectOfType<NavmeshArea2D>();
        path = new List<NavmeshNode2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        crouchHeight = height / 2;
        capsuleCollider.size = new Vector2(width, height);

        _sprite = transform.Find("Sprite");
        _initSpriteHeight = _sprite.localScale.y;
    }

    protected virtual void FixedUpdate() {

        if (capsuleCollider.size.y < capsuleCollider.size.x) { capsuleCollider.direction = CapsuleDirection2D.Horizontal; }
        else { capsuleCollider.direction = CapsuleDirection2D.Vertical; }

        _sprite.localScale = capsuleCollider.size;

        Orient();
        GroundCheck();
    }

    protected virtual List<NavmeshNode2D> GetPath(Vector2 start, Vector2 end) {
        List<NavmeshNode2D> closedList = new List<NavmeshNode2D>();
        List<NavmeshNode2D> openList = new List<NavmeshNode2D>();
        NavmeshNode2D startNode = area.NodeAtPoint(start, this);
        NavmeshNode2D endNode = area.NodeAtPoint(end, this);
        NavmeshNode2D currentNode = startNode;
        

        if (startNode.type == NavmeshNode2D.NodeType.None || startNode == null) { return closedList; }

        closedList.Add(startNode);
        TotalCost(startNode, startNode, endNode);

        int count = 0;
        while (currentNode != endNode && count < 1000) {
            NavmeshNode2D best = currentNode.connections[0].b;
            TotalCost(currentNode, best, endNode);
            foreach (NavmeshNode2D.NavmeshNodeConnection2D connection in currentNode.connections) {
                TotalCost(connection.a, connection.b, endNode);
                if (connection.b.fcost <= best.fcost) {
                    best = connection.b;
                }
            }
            closedList.Add(best);
            currentNode = best;
            count++;
        }
        if (currentNode != endNode && jumpDistance > 0) {
            //could do something here if path is not found

        }

        return closedList;
    }

    protected virtual float TotalCost(NavmeshNode2D parent, NavmeshNode2D n, NavmeshNode2D end) {
        n.fcost = Gcost(parent, n) + Heuristic(n, end);
        return n.fcost;
    }

    protected virtual float Heuristic(NavmeshNode2D n, NavmeshNode2D end) {
        return Mathf.Abs(end.worldPosition.x - n.worldPosition.x) + Mathf.Abs(end.worldPosition.y - n.worldPosition.y);
    }

    protected virtual float Gcost(NavmeshNode2D parent, NavmeshNode2D n) {
        if (n.type == NavmeshNode2D.NodeType.None) { n.gcost = Mathf.Infinity; }
        else if (parent == n) { n.gcost = 0; }
        else
        {
            n.gcost = parent.gcost + Vector2.Distance(parent.worldPosition, n.worldPosition);
        }

        return n.gcost;
    }

    protected virtual void Orient() {
        if (ladder)
        {
            if (rigidbody.velocity.x != 0) {
                float direction = Mathf.Abs(rigidbody.velocity.x) / rigidbody.velocity.x;
                _sprite.localScale = new Vector3(direction * Mathf.Abs(_sprite.localScale.x), _sprite.localScale.y);
            }
            transform.up = ladder.GetUp();
        }
        else {
            //Later: Get normal vector of the ground undernieth and set the tran's up to that.
            //transform.up = Vector3.up;

            transform.up = Vector2.up;
        }
    }

    private void OnDrawGizmos() {
        if (!capsuleCollider) { capsuleCollider = GetComponent<CapsuleCollider2D>(); }
        if (!Application.isPlaying)
        {
            capsuleCollider.size = new Vector2(width, height);
        }
    }

    protected void GroundCheck() {
        Collider2D ground = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - (transform.localScale.y/2)), new Vector2(GetSize().x/2, 0.02f), 0f, 1 << LayerMask.NameToLayer("Environment"));

        if (ground) { isGrounded = true; }
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
        Vector2 bottomLeft = new Vector2(transform.position.x - transform.localScale.x / 4 , transform.position.y - transform.localScale.y / 2 - 0.1f);
        Vector2 bottomRight = new Vector2(transform.position.x + transform.localScale.x / 4, transform.position.y - transform.localScale.y / 2 - 0.1f);
        Vector2 topRight = new Vector2(transform.position.x + transform.localScale.x / 4, transform.position.y - transform.localScale.y / 2 + 0.1f);
        Vector2 topLeft = new Vector2(transform.position.x - transform.localScale.x / 4, transform.position.y - transform.localScale.y / 2 + 0.1f);

        Debug.DrawLine(bottomLeft, bottomRight, Color.yellow);
        Debug.DrawLine(bottomRight, topRight, Color.yellow);
        Debug.DrawLine(bottomLeft, topLeft, Color.yellow);
        Debug.DrawLine(topLeft, topRight, Color.yellow);
    }

    protected virtual void OnDrawGizmosSelected() {
        if (!area) { area = FindObjectOfType<NavmeshArea2D>(); }

        Gizmos.DrawCube(area.NodeAtPoint(transform.position, this).worldPosition, Vector3.one/4);
        for (int i = 1; i < path.Count; i++) {
            Debug.DrawLine(path[i-1].worldPosition, path[i].worldPosition, Color.green);
        }

        DrawGroundedBox();
    }
}

