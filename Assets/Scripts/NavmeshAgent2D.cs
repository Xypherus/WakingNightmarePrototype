﻿using System.Collections;
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

    public List<NavmeshNode2D> path = new List<NavmeshNode2D>();
    new public Rigidbody2D rigidbody;
    public Ladder ladder;

    public bool isStopped;
    public bool pathing;

    new CapsuleCollider2D collider;

    NavmeshArea2D area;

    public void MoveTo(Vector2 position, UnityEngine.Events.UnityAction callback) {
        StartCoroutine(MoveToEnumerator(position, callback));
    }

    public void DismountLadder() {
        Debug.Log("Dismounting Ladder");
        ladder = null;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    public void LadderMountDismount(float radius) {
        Debug.Log("Toggleing Ladder");
        if (!ladder) { MountNearestLadder(radius); }
        else { DismountLadder(); }
    }

    public void MountNearestLadder(float radius) {
        Debug.Log("Attempting Mounting Ladder");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, area.layerMask);

        foreach (Collider2D collider in colliders) {
            if (collider.GetComponent<Ladder>()) {
                collider.GetComponent<Ladder>().MountLadder(this);
                break;
            }
        }
    }

    protected virtual void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        area = FindObjectOfType<NavmeshArea2D>();
        path = new List<NavmeshNode2D>();
        collider = GetComponent<CapsuleCollider2D>();
        crouchHeight = height / 2;
        collider.size = new Vector2(width, height);
    }

    protected virtual void FixedUpdate() {
        Orient();
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

    private void Orient() {
        if (ladder)
        {
            transform.up = ladder.GetUp();
        }
        else {
            //Later: Get normal vector of the ground undernieth and set the tran's up to that.
            transform.up = Vector3.up;
        }
    }

    private void OnDrawGizmos() {
        if (!collider) { collider = GetComponent<CapsuleCollider2D>(); }
        collider.size = new Vector2(width, height);
    }

    private IEnumerator MoveToEnumerator(Vector2 position, UnityEngine.Events.UnityAction callback) {
        rigidbody.velocity = Vector2.zero;
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        if (pathing) { isStopped = true; }
        for (float i = 0; i < 1; i += Time.deltaTime) {
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

