using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MoveTo : MonoBehaviour {
    public Transform goal;
    
    [SerializeField] private float distThresh = 1f;
    public CrowdAgentSpawner spawner;
    
    private bool toDestroy;
    private NavMeshAgent agent;

    public bool Pathfinding { private set; get; }


    private void Start () {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = Random.Range(30, 60);
        agent.destination = goal.position;
        agent.speed = Random.Range(0.75f, 1.05f);
        Pathfinding = false;
    }

    public void SetTarget(Transform goal) {
        this.goal = goal;
        agent.destination = goal.position;
        agent.isStopped = false;
        Pathfinding = true;
    }

    private void Update() {
        //if (!(Vector3.Distance(transform.position, goal.position) < distThresh)) return;
        //if (toDestroy) {
        //    Destroy(gameObject);
        //    return;
        //}
        
        //goal = spawner.RndFromList();
        //if (goal.CompareTag("Exit")) toDestroy = true;
        //agent.destination = goal.position;
        var g = goal.position;
        var t = transform.position;
        if (Pathfinding && Vector3.Distance(new Vector3(g.x, 0, g.z), new Vector3(t.x, 0, t.z)) < 0.1f) {
            agent.isStopped = true;
            Pathfinding = false;
        } 
    }

    private void OnDestroy() {
        spawner.AgentDestroyed();
    }
}
