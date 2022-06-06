using System;
using UnityEngine;

public class ManageDetections : MonoBehaviour {

    [SerializeField] private FieldOfView visionCone;
    
    private void Update() {
        for(var i = 0; i < transform.childCount; i++) {
            var child = transform.GetChild(i);
            if(visionCone.visibleTargets.Contains(child)) {
                child.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            } else {
                child.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            }
        }
        
    }
}
