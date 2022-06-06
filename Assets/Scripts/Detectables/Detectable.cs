using System.Linq;
using UnityEngine;

public class Detectable : MonoBehaviour {
   
    [SerializeField] private DetectionPoint[] detectionPoints;
    
    [Range(0.0f, 100.0f)]
    [SerializeField] private float minRequiredVisibilityPercentage;

    private void Start() {
        detectionPoints = transform.GetComponentsInChildren<DetectionPoint>();
    }

    public bool IsVisible(Transform from, float fovAngle, int obstacleMask) {
        var fromPos = from.position;
        var toPos = transform.position;
        if (detectionPoints.Length == 0) {
            return Vector3.Angle(from.forward, (toPos - fromPos).normalized) < fovAngle / 2 && !Physics.Raycast(fromPos, (toPos - fromPos).normalized, Vector3.Distance(fromPos, toPos), obstacleMask);
        }

        var numObstructed = (
            from detectionPoint in detectionPoints 
            select detectionPoint.transform.position into dpPos 
            let dirToTarget = (dpPos - fromPos).normalized 
            where Vector3.Angle(@from.forward, dirToTarget) > fovAngle / 2 || Physics.Raycast(fromPos, dirToTarget, Vector3.Distance(fromPos, dpPos), obstacleMask) 
            select dpPos
        ).Count();

        return 100f - (float)numObstructed / detectionPoints.Length * 100f >= minRequiredVisibilityPercentage;
    }
}
