using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTestAgent : NavmeshAgent2D {

    public Transform target;
	
	// Update is called once per frame
	void Update () {
        target = GameObject.FindGameObjectWithTag("Target").transform;
        if (target)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, (1 << LayerMask.NameToLayer("Environment")));
            if (hit)
            {
                path = GetPath(hit.point, target.position);
            }
        }
	}
}
