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

    public void MountLadder(NavmeshAgent2D actor) {
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
                actor.rigidbody.isKinematic = true;
                actor.ladder = this;
            });
        }
        else {
            actor.rigidbody.isKinematic = true;
            actor.ladder = this;
        }
    }

    void OrientLadder() {
        if (ladderType == LadderType.Background) { return; }

        Vector2 ladderTop;
        Vector2 ladderBottom;
        ladderTop = new Vector2(transform.position.x, transform.position.y + (transform.localScale.y/2));
        ladderBottom = new Vector2(transform.position.x, transform.position.y - (transform.localScale.y / 2));

        if (ladderTop.y == ladderBottom.y)
        {
            if (ladderBottom.x > ladderTop.x)
            {
                top = ladderBottom;
                bottom = ladderTop;
            }
            else
            {
                bottom = ladderBottom;
                top = ladderTop;
            }
        }
        else if (ladderTop.y > ladderBottom.y)
        {
            top = ladderTop;
            bottom = ladderBottom;
        }
        else {
            top = ladderBottom;
            bottom = ladderTop;
        }
    }

    void RenderSprite() {
        if (ladderType == LadderType.Side) { renderer.sprite = sideSprite; }
        else if (ladderType == LadderType.Background) { renderer.sprite = BackgroundSprite; }
    }
}
