using UnityEngine;

public class PlayerController : SmartAgentController {
    
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private float maxVelocityChange = 10.0f;

    private Camera mainCamera;
    
    private bool plyrCtrl = true;

    private void Start() {
        mainCamera = Camera.main;
        id = 0;
    }

    public override void MakeRandomDecision(Vector3[] options) {
        if (plyrCtrl) return;
        base.MakeRandomDecision(options);
    }

    protected override void Update() {
        if (Input.GetKeyDown(KeyCode.H)) {
            var g = GameObject.Find("GridZero");
            var curRenderStatus = g.GetComponentInChildren<Renderer>().enabled;
            foreach (var renderer in g.GetComponentsInChildren<Renderer>()) {
                renderer.enabled = !curRenderStatus;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.K)) {
            plyrCtrl = !plyrCtrl;
        }
        if(plyrCtrl) return;
        base.Update();
    }

    private void FixedUpdate() {
        if (!plyrCtrl) return;
        var targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        // Apply a force that attempts to reach our target velocity
        var velocity = playerRb.velocity;
        var velocityChange = targetVelocity - velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        playerRb.AddForce(velocityChange, ForceMode.VelocityChange);
            
        transform.eulerAngles = Vector3.zero;

        var mousePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            mainCamera.transform.position.y));
        transform.LookAt(mousePos + Vector3.up * transform.position.y);
    }
}
