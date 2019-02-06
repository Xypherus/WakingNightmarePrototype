﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour {

    public float length;
    public int segmentCount;
    public GameObject ropeSegment;

    List<Ladder> segments;

    int _lastSegmentCount;
    GameObject _lastRopeSegment;
    float _lastLength;


	// Use this for initialization
	void Start () {
        segments = new List<Ladder>();
        UpdateSegments();
	}
	
	// Update is called once per frame
	void Update () {
        _lastSegmentCount = segmentCount;
        _lastRopeSegment = ropeSegment;
        _lastLength = length;
	}
    
    void UpdateSegments() {
        foreach (Ladder segment in segments) { Destroy(segment.gameObject); }
        segments.Clear();

        float height = length / segmentCount;

        for (int i = segmentCount-1; i >= 0; i--) {
            Vector3 position = new Vector3(transform.position.x, (transform.position.y - (i * height)) - (height/2));

            GameObject ladder = Instantiate(ropeSegment, position, transform.rotation, transform);
            Ladder ladderObject = ladder.GetComponent<Ladder>();
            ladder.transform.localScale = new Vector3(ladder.transform.localScale.x, height*4, ladder.transform.localScale.z);

            segments.Add(ladderObject);
        }

        if (segmentCount > 1)
        {
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
                else {
                    ladder.next = segments[i + 1];
                    ladder.previous = segments[i - 1];
                }
            }
        }  
    }

    private void OnDrawGizmosSelected()
    {
        if (length > 0 && segmentCount > 0) {
            float height = length / segmentCount;
            Vector3 start = transform.position;
            Color lineColor = Color.green;
            Vector3 lastPos = start;

            for (int i = 1; i <= segmentCount; i++) {
                if (i % 2 == 0) { lineColor = Color.green; }
                else { lineColor = Color.blue; }

                Vector3 position = start - new Vector3(0, i * height, 0);
                Debug.DrawLine(lastPos, position, lineColor);
                lastPos = position;
            }
        }
    }
}
