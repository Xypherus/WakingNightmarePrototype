using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Ladder : MonoBehaviour {

    public enum LadderType { Side, Background }
    public LadderType ladderType;

    [Tooltip("The sprite of the ladder when the type is Side.")]
    public Sprite sideSprite;
    [Tooltip("The sprite of the ladder when it is the background type.")]
    public Sprite BackgroundSprite;

    Vector2 top;
    Vector2 bottom;
    Vector2 left;
    Vector2 right;

    new BoxCollider2D collider;
    new SpriteRenderer renderer;

    private void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update () {
        RenderSprite();
        OrientLadder();
	}

    public Vector3 GetUp() {
        return ((Vector3)top - transform.position).normalized;
    }

    public void MoveOnLadder(NavmeshAgent2D actor, Vector2 movement) {
        if (ladderType == LadderType.Side)
        {
            float direction = movement.x;
            if (direction == 0) { direction = movement.y; }
            Debug.Log(direction);

            if (direction > 0)
            {
                actor.transform.position = Vector3.MoveTowards(actor.transform.position, top, actor.speed / 4 * Time.deltaTime);
            }
            else if (direction < 0)
            {
                actor.transform.position = Vector3.MoveTowards(actor.transform.position, bottom, actor.speed / 4 * Time.deltaTime);
            }
        }
        else {
            Vector3 target = Vector3.zero;
            if (movement.x > 0) { target.x = right.x; }
            else if (movement.x < 0) { target.x = left.x; }
            else { target.x = actor.transform.position.x; }

            if (movement.y > 0) { target.y = top.y; }
            else if (movement.y < 0) { target.y = bottom.y; }
            else { target.y = actor.transform.position.y; }

            actor.transform.position = Vector3.MoveTowards(actor.transform.position, target, actor.speed / 4 * Time.deltaTime);
        }
    }

    public void MountLadder(NavmeshAgent2D actor) {
        actor.ladder = this;
        if (ladderType == LadderType.Side)
        {
            Vector2 position;
            if (Vector2.Distance(actor.transform.position, top) < Vector2.Distance(actor.transform.position, bottom))
            {
                position = top;
            }
            else { position = bottom; }

            actor.MoveTo(position, () =>
            {
                actor.rigidbody.bodyType = RigidbodyType2D.Kinematic;
            });
        }
        else {
            actor.rigidbody.bodyType = RigidbodyType2D.Kinematic;
            actor.ladder = this;
        }
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
    }

    void RenderSprite() {
        if (ladderType == LadderType.Side) { renderer.sprite = sideSprite; }
        else if (ladderType == LadderType.Background) { renderer.sprite = BackgroundSprite; }
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
