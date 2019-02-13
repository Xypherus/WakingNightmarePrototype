using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour {

    //Creates Public variables that are used in personalizing ladder
    public float length;
    public int segmentCount;
    public GameObject ropeSegment;

    //Initializes List of segments in the Ladder
    List<Ladder> segments;


	// Use this for initialization
	void Start () {
        //Sets parent object to be uniform size
        transform.localScale = new Vector3(1, 1, 1);

        //Creates list of segments
        segments = new List<Ladder>();

        //Calls function that Creates the ladder segments
        UpdateSegments();
	}

    void UpdateSegments()
    {
        //Destroys any gameobjects already in the parent
        foreach (Ladder segment in segments) { Destroy(segment.gameObject); }
        segments.Clear();

        //States the math for finding height of each segment
        float height = length / segmentCount;

        //Builds rope from bottom to top
        for (int i = segmentCount - 1; i >= 0; i--)
        {
            //Sets position of current rope segment
            Vector3 position = new Vector3(transform.position.x, (transform.position.y - (i * height)) - (height / 2));

            //Instantiates current rope segment at correct position
            GameObject ladder = Instantiate(ropeSegment, position, transform.rotation, transform);

            //Gets Ladder component and Changes the scale to fit the correct height
            Ladder ladderObject = ladder.GetComponent<Ladder>();
            ladder.transform.localScale = new Vector3(ladder.transform.localScale.x, height, ladder.transform.localScale.z);

            //Adds the current segment to the segments list
            segments.Add(ladderObject);
        }

        //Next portion cannot be done if there are no segments
        if (segmentCount > 1)
        {
            //Defines what is considered the "next" ladder and "previous" ladder based on the list that was formed
            //This is used for climbing and grabbing purposes
            for (int i = 0; i < segmentCount; i++)
            {

                Ladder ladder = segments[i];

                if (i == 0)
                {
                    ladder.next = segments[i + 1];
                }
                else if (i == segmentCount - 1)
                {
                    ladder.previous = segments[i - 1];
                }
                else
                {
                    ladder.next = segments[i + 1];
                    ladder.previous = segments[i - 1];
                }
            }

            for(int i = segmentCount - 1; i >= 0; i--)
            {
                Ladder sections = segments[i];
                //Finds each segments hinge joint
                HingeJoint2D Hinge = sections.GetComponent<HingeJoint2D>();
                if (i >= 0 && i < segmentCount - 1)
                {
                    //Sets each hinge joint to be connected to the segment before it
                    Hinge.connectedBody = segments[i + 1].GetComponent<Rigidbody2D>();
                }

            }
        }
    }     

    private void OnDrawGizmos()
    {   
        if (length > 0 && segmentCount > 0) {

            //Initializes variables for Drawing
            float height = length / segmentCount;
            Vector3 start = transform.position;
            Color lineColor = Color.green;
            Vector3 lastPos = start;

            //Tests to see if even number, if it is the green, if not then blue
            for (int i = 1; i <= segmentCount; i++) {
                if (i % 2 == 0) { lineColor = Color.green; }
                else { lineColor = Color.blue; }

                //Draws the projected rope using colors and variables specified above
                Vector3 position = start - (transform.rotation * new Vector3(0, i * height, 0));
                Debug.DrawLine(lastPos, position, lineColor);
                lastPos = position;
            }
        }
    }
}
