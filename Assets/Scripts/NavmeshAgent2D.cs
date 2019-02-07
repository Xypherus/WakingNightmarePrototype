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

    public List<NavmeshNode2D> path = new List<NavmeshNode2D>();
    new public Rigidbody2D rigidbody;
    public Ladder ladder;

    public bool isStopped;
    public bool pathing;

    protected CapsuleCollider2D capsuleCollider;

    NavmeshArea2D area;

    #region Testing Variables
    protected Transform _sprite;
    protected float _initSpriteHeight;
    #endregion

    public void MoveTo(Vector2 position, UnityEngine.Events.UnityAction callback) {
        StartCoroutine(MoveToEnumerator(position, callback));
    }

    public void DismountLadder() {
        if (!ladder) { Debug.LogWarning("Could not dismount ladder because it does not exist."); return; }
        if (ladder.CheckActorCollisions(this) > 0) { Debug.LogWarning("Could not dismount ladder because player is inside terrain!"); return; }

        Debug.Log("Dismounting Ladder...");
        ladder = null;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
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
                _sprite.localScale = new Vector3(direction, _sprite.localScale.y);
            }
            transform.up = ladder.GetUp();
        }
        else {
            //Later: Get normal vector of the ground undernieth and set the tran's up to that.
            //transform.up = Vector3.up;

            RaycastHit2D surfacePoint = Physics2D.Raycast(transform.position, Vector2.down, height, 1 << LayerMask.NameToLayer("Environment"));

            if (surfacePoint)
            {
                transform.up = Vector2.Lerp(transform.up, surfacePoint.normal, 10 * Time.deltaTime);
            }
            else {
                transform.up = Vector2.Lerp(transform.up, Vector2.up, 10 * Time.deltaTime);
            }
        }
    }

    private void OnDrawGizmos() {
        if (!capsuleCollider) { capsuleCollider = GetComponent<CapsuleCollider2D>(); }
        capsuleCollider.size = new Vector2(width, height);
    }

    protected void GroundCheck() {
        RaycastHit2D ground = Physics2D.Raycast(transform.position, -transform.up, (capsuleCollider.size.y/2)+0.02f, 1 << LayerMask.NameToLayer("Environment"));

        if (ground || ladder) { isGrounded = true; }
        else { isGrounded = false; }
    }

    private IEnumerator MoveToEnumerator(Vector2 position, UnityEngine.Events.UnityAction callback) {
        rigidbody.velocity = Vector2.zero;
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        if (pathing) { isStopped = true; }
        for (float i = 0; i < 0.5; i += Time.deltaTime) {
            transform.position = Vector3.Lerp(transform.position, position, i);
            yield return new WaitForEndOfFrame();
        }
        transform.position = position;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        if (pathing) { isStopped = false; }
        callback();
    }

    private void OnDrawGizmosSelected() {
        if (!area) { area = FindObjectOfType<NavmeshArea2D>(); }

        Gizmos.DrawCube(area.NodeAtPoint(transform.position, this).worldPosition, Vector3.one/4);
        for (int i = 1; i < path.Count; i++) {
            Debug.DrawLine(path[i-1].worldPosition, path[i].worldPosition, Color.green);
        }
    }
}

