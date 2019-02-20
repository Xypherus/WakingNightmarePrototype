using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Ladder : MonoBehaviour {

    public enum LadderType { Side, Background }
    public LadderType ladderType;

    public Ladder next = null;
    public Ladder previous = null;
    public float height;

    Vector2 top;
    Vector2 bottom;
    Vector2 left;
    Vector2 right;

    new BoxCollider2D collider;
    new SpriteRenderer renderer;

    Vector2 lastPosition;
    Vector2 positionDelta;

    [HideInInspector]
    public float percent;

    private void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();

        if (transform.localScale != Vector3.one)
        {
            renderer.size = transform.localScale;
            transform.localScale = Vector3.one;
        }

        lastPosition = transform.position;
        positionDelta = Vector2.zero;
    }

    // Update is called once per frame
    void Update () {
        height = Vector2.Distance(bottom, top);
        positionDelta = (Vector2) transform.position - lastPosition;
        OrientLadder();

        if (transform.localScale != Vector3.one) {
            renderer.size = transform.localScale;
            transform.localScale = Vector3.one;
        }

        collider.size = renderer.size;
        
        lastPosition = transform.position;

        Debug.Log(percent + " percent up the ladder", this);
    }

    public Vector3 GetUp() {
        return ((Vector3)top - transform.position).normalized;
    }

    public void MoveOnLadder(NavmeshAgent2D actor, Vector2 movement) {
        //TODO: Reprogram to move to a point that is a percentage the distance to top;
        actor.transform.position = GetPositionOnLadder();
        Vector3 previousPos = actor.transform.position;

        bool avoidCollCheck = false;
        if (CheckActorCollisions(actor) > 0) { avoidCollCheck = true; }
        if (ladderType == LadderType.Side)
        {
            previousPos = actor.transform.position;
            if (actor.isProne) { return; }

            float direction = movement.y;

            percent += (actor.speed / 4 / height) * Time.deltaTime * direction;

            if (percent <= 0 ) {
                if (previous)
                {
                    actor.transform.position = previous.top;
                    actor.ladder = previous;
                }
                else {
                    actor.transform.position = bottom;
                    percent = 0;
                }
            }
            else if (percent >= 1) {
                if (next)
                {
                    actor.transform.position = next.bottom;
                    actor.ladder = next;
                }
                else {
                    actor.transform.position = top;
                    percent = 1;
                }
            }


            actor.transform.position = GetPositionOnLadder();


            if (!avoidCollCheck) { HandleActorCollisions(actor, previousPos); }

        }
        else {
            Vector3 target = Vector3.zero;
            if (movement.x > 0 ) { target.x = right.x; }
            else if (movement.x < 0) { target.x = left.x; }
            else { target.x = actor.transform.position.x; }

            if (movement.y > 0) { target.y = top.y; }
            else if (movement.y < 0) { target.y = bottom.y; }
            else { target.y = actor.transform.position.y; }

            actor.transform.position = Vector3.MoveTowards(actor.transform.position, target, actor.speed / 4 * Time.deltaTime);

            if (!avoidCollCheck) { HandleActorCollisions(actor, previousPos); }
        } 
            
    }

    public void MountLadder(NavmeshAgent2D actor) {
        actor.ladder = this;
        if (ladderType == LadderType.Side)
        {
            percent = Vector2.Distance(bottom, actor.transform.position) / Vector2.Distance(bottom, top);

            actor.MoveTo(GetPositionOnLadder(), () =>
            {
                if (actor.ladder)
                {
                    actor.rigidbody.bodyType = RigidbodyType2D.Kinematic;
                }
                else {
                    actor.rigidbody.bodyType = RigidbodyType2D.Dynamic;
                }
            });
        }
        else {
            actor.rigidbody.bodyType = RigidbodyType2D.Kinematic;
            actor.rigidbody.velocity = Vector3.zero;
        }
        
    }

    void HandleActorCollisions(NavmeshAgent2D agent, Vector3 returnPosition) {
        if (CheckActorCollisions(agent) > 0) { agent.transform.position = returnPosition; }
    }

    public int CheckActorCollisions(NavmeshAgent2D agent) {
        if (ladderType == LadderType.Side)
        {
            Collider2D[] contacts = Physics2D.OverlapBoxAll(agent.transform.position, new Vector2(0.01f, agent.GetSize().y), transform.eulerAngles.z, 1 << LayerMask.NameToLayer("Environment"));
            return contacts.Length;
        }
        else
        {
            Collider2D[] contacts = new Collider2D[100];
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(1 << LayerMask.NameToLayer("Environment"));
            int discrepencies = agent.GetComponent<Collider2D>().OverlapCollider(filter, contacts);
            return discrepencies;
        }
    }

    Vector2 GetPositionOnLadder() {
        Vector2 newPos = Vector2.Lerp(bottom, top, percent);
        return newPos;
    }

    void OrientLadder() {
        Vector2 ladderTop;
        Vector2 ladderBottom;
        Vector2 ladderRight;
        Vector2 ladderLeft;
        ladderTop = transform.TransformPoint(new Vector2(0, collider.offset.y + (collider.size.y/2)));
        ladderBottom = transform.TransformPoint(new Vector2(0, collider.offset.y - (collider.size.y / 2)));
        ladderRight = transform.TransformPoint(new Vector3(collider.offset.x + (collider.size.x / 2), 0));
        ladderLeft = transform.TransformPoint(new Vector3(collider.offset.x - (collider.size.x / 2), 0));

        top = ladderTop;
        bottom = ladderBottom;
        right = ladderRight;
        left = ladderLeft;

        /*
        if (ladderTop.y == ladderBottom.y)
        {
            if (ladderBottom.x > ladderTop.x)
            {
                left = ladderRight;
                right = ladderLeft;
                top = ladderBottom;
                bottom = ladderTop;
            }
            else
            {
                left = ladderLeft;
                right = ladderRight;
                bottom = ladderBottom;
                top = ladderTop;
            }
        }
        else if (ladderTop.y > ladderBottom.y)
        {
            left = ladderLeft;
            right = ladderRight;
            top = ladderTop;
            bottom = ladderBottom;
        }
        else {
            left = ladderRight;
            right = ladderLeft;
            top = ladderBottom;
            bottom = ladderTop;
        }
        */
    }

    public Vector2 GetPointOnLadder(Vector2 position) {
        return (Vector2) Vector3.Project((position-bottom), top-bottom)+bottom;
    }

    private void OnDrawGizmosSelected()
    {
        Start();
        Update();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(top, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottom, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(right, 0.5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(left, 0.5f);
    }
}
