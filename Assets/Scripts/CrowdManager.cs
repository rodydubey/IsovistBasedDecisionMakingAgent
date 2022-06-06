using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrowdManager: MonoBehaviour {
    public List<Transform> crowd = new List<Transform>();
    private Transform[] cache = Array.Empty<Transform>();
    [Range(0f, 1f)]
    [SerializeField] private float directionThreshold = 0.75f;
    public int numTowards;
    public int numAway;
    public int total;
    public Vector3 avgMvmt;

    public int GetTotalNum() {
        return crowd.Count;
    }

    public int NumMovingTowards() {
        return crowd.Count(agent => Vector3.Dot(agent.forward, transform.forward) < -directionThreshold);
    }
    
    public int NumMovingAway() {
        return crowd.Count(agent => Vector3.Dot(agent.forward, transform.forward) > directionThreshold);
    }

    public Vector3 GetAvgMovement() {
        return crowd.Select(agent => agent.forward).Aggregate(Vector3.zero, (acc, vec) => acc + vec);
    }

    private void LateUpdate() {
        numTowards = NumMovingTowards();
        numAway = NumMovingAway();
        total = GetTotalNum();
        avgMvmt = GetAvgMovement();
        //Debug.DrawRay(transform.position, GetAvgMovement(), Color.red);
        
        if(!gameObject.GetComponent<PlayerController>()) return;
        
        foreach (var agent in cache) {
            agent.GetComponent<LineRenderer>().enabled = false;
        }
        
        foreach (var agent in crowd) {
            //Debug.DrawRay(agent.position, agent.forward, Color.cyan);
            agent.GetComponent<LineRenderer>().enabled = true;
            var line = agent.GetComponent<LineRenderer>();
            line.SetPosition(0, agent.position);
            line.SetPosition(1, agent.forward + agent.position);
        }

        
        cache = crowd.ToArray();
    }
}