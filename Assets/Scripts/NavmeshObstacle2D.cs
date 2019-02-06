using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NavmeshObstacle2D : MonoBehaviour {

    Vector3 previousPos;
    Quaternion previousRotation;
    Vector3 previousScale;

    public NavmeshArea2D area;
    [HideInInspector]
    public new Collider2D collider;

	// Use this for initialization
	void Start () {
        collider = GetComponent<Collider2D>();
        area = GameObject.FindObjectOfType<NavmeshArea2D>();
	}
	
	// Update is called once per frame
	void Update () {
        if (previousPos != transform.position || 
            previousRotation != transform.rotation ||
            previousScale != transform.localScale)
        {
            area.UpdateGrid(collider);
        }

        UnityEngine.Profiling.Profiler.BeginSample("Other Obstacle Stuff");
        previousPos = transform.position;
        previousRotation = transform.rotation;
        previousScale = transform.localScale;
        UnityEngine.Profiling.Profiler.EndSample();
	}

    void OnDrawGizmosSelected() {
        collider = GetComponent<Collider2D>();
        area = GameObject.FindObjectOfType<NavmeshArea2D>();

        Vector2 min = new Vector2 (collider.bounds.min.x -1, collider.bounds.min.y-1 );
        Vector2 max = new Vector2(collider.bounds.max.x + 1, collider.bounds.max.y + 1);



        Debug.DrawLine(min, new Vector2(min.x, max.y), Color.green);
        Debug.DrawLine(new Vector2(min.x, max.y), new Vector2(max.x, max.y), Color.green);
        Debug.DrawLine(min, new Vector2(max.x, min.y), Color.green);
        Debug.DrawLine(new Vector2(max.x, min.y), new Vector2(max.x, max.y), Color.green);
    }
}
