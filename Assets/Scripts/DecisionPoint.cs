using System;
using System.Linq;
using UnityEngine;

public class DecisionPoint : MonoBehaviour {
    [SerializeField] private Vector3[] outgoingVectors;
    [SerializeField] private float radius;
    [SerializeField] private float cdRadius;
    private bool onCooldown;


    private void Update() {
        var cdhits = Physics.OverlapSphere(transform.position, cdRadius, LayerMask.GetMask("Default"));
        var cdplayer = cdhits.Any(x => x.gameObject.CompareTag("Player"));
        
        if (!cdplayer) {
            onCooldown = false;
        }
        
        var hits = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Default"));
        var player = hits.FirstOrDefault(x => x.gameObject.CompareTag("Player"));
        
        if (player && !onCooldown) {
            player.GetComponent<SmartAgentController>().MakeRandomDecision(outgoingVectors);
            onCooldown = true;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        foreach (var outgoingVector in outgoingVectors) {
            Gizmos.DrawRay(transform.position, outgoingVector);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, cdRadius);
    }
}
