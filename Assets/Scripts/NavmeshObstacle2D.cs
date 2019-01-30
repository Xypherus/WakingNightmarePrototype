using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NavmeshObstacle2D : MonoBehaviour {

    Vector3 previousPos;
    Quaternion previousRotation;
    Vector3 previousScale;

    public NavmeshArea2D area;
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
            UnityEngine.Profiling.Profiler.BeginSample("Navmesh Updating");
            area.UpdateGrid(collider);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        previousPos = transform.position;
        previousRotation = transform.rotation;
        previousScale = transform.localScale;
	}

    void OnDrawGizmosSelected() {
        /*
        collider = GetComponent<Collider2D>();
        area = GameObject.FindObjectOfType<NavmeshArea2D>();

        Vector2 min = collider.bounds.min;
        Vector2 max = collider.bounds.max;

        float width = max.x - min.x;
        float height = max.y - min.y;
        
        min = new Vector2(transform.position.x - width, transform.position.y - height);
        max = new Vector2(transform.position.x + width, transform.position.y + width);
        

        Debug.DrawLine(min, new Vector2(min.x, max.y), Color.green);
        Debug.DrawLine(new Vector2(min.x, max.y), new Vector2(max.x, max.y), Color.green);
        Debug.DrawLine(min, new Vector2(max.x, min.y), Color.green);
        Debug.DrawLine(new Vector2(max.x, min.y), new Vector2(max.x, max.y), Color.green);
        */
    }
}
