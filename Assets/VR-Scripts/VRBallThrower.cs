using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class VRBallThrower : MonoBehaviour {
    [Header("VR References")]
    public Transform VRCamera;
    public XRRayInteractor RightController;
    public ActionBasedController RightControllerAction;

    [Header("Ball Settings")]
    public GameObject BallPrefab;
    public Transform BallSpawnPoint;
    public float BallThrowForce = 8f;
    public float BallLifetime = 15f;

    [Header("Movement Constraints")]
    public float MinX = -6f;
    public float MaxX = 6f;
    public float FixedY = 0.5f;
    public float FixedZ = -9f;

    [Header("Agent Integration")]
    public CatchAgent CatchAgent;

    [Header("Visual Feedback")]
    public LineRenderer TrajectoryLine;
    public int TrajectoryPoints = 30;
    public float TrajectoryTimeStep = 0.1f;

    [Header("Throw Cooldown")]
    public float ThrowCooldown = 1f;

    private float lastThrowTime = 0f;

    void Start() {
        Vector3 startPos = new Vector3(0, FixedY, FixedZ);
        transform.position = startPos;

        if (RightControllerAction != null) {
            RightControllerAction.activateAction.action.performed += OnTriggerPressed;
        }

        if (TrajectoryLine != null) {
            TrajectoryLine.positionCount = TrajectoryPoints;
            TrajectoryLine.enabled = false;
        }
    }

    void Update() {
        ConstrainPlayerMovement();
        UpdateTrajectoryVisualization();
    }

    void ConstrainPlayerMovement() {
        Vector3 currentPos = transform.position;
        float clampedX = Mathf.Clamp(currentPos.x, MinX, MaxX);

        Vector3 constrainedPos = new Vector3(clampedX, FixedY, FixedZ);

        if (Vector3.Distance(currentPos, constrainedPos) > 0.01f) {
            transform.position = constrainedPos;
        }
    }

    void UpdateTrajectoryVisualization() {
        if (TrajectoryLine == null) return;

        if (RightController != null) {
            TrajectoryLine.enabled = true;
            Vector3 throwDirection = RightController.transform.forward;
            Vector3 startPosition = BallSpawnPoint.position;

            for (int i = 0; i < TrajectoryPoints; i++) {
                float time = i * TrajectoryTimeStep;
                Vector3 point = CalculateTrajectoryPoint(startPosition, throwDirection * BallThrowForce, time);
                TrajectoryLine.SetPosition(i, point);

                if (point.y <= 0) {
                    for (int j = i; j < TrajectoryPoints; j++) {
                        TrajectoryLine.SetPosition(j, point);
                    }
                    break;
                }
            }
        } else {
            TrajectoryLine.enabled = false;
        }
    }

    Vector3 CalculateTrajectoryPoint(Vector3 startPos, Vector3 initialVelocity, float time) {
        Vector3 gravity = Physics.gravity;
        return startPos + initialVelocity * time + 0.5f * gravity * time * time;
    }

    void OnTriggerPressed(UnityEngine.InputSystem.InputAction.CallbackContext context) {
        if (Time.time - lastThrowTime < ThrowCooldown) {
            return;
        }

        if (RightController != null) {
            ThrowBall();
        }
    }

    void ThrowBall() {
        if (BallPrefab == null || BallSpawnPoint == null) {
            Debug.LogWarning("Ball prefab or spawn point not assigned!");
            return;
        }

        GameObject newBall = Instantiate(BallPrefab, BallSpawnPoint.position, Quaternion.identity);

        Rigidbody ballRb = newBall.GetComponent<Rigidbody>();
        Ball ballBall = newBall.GetComponent<Ball>();
        if (ballRb == null || ballBall == null) {
            Destroy(newBall);
            return;
        }

        Vector3 throwDirection = RightController.transform.forward;
        ballRb.velocity = Vector3.zero;
        ballRb.AddForce(throwDirection * BallThrowForce, ForceMode.VelocityChange);

        if (CatchAgent != null) {
            CatchAgent.Ball = ballBall;
        }

        Destroy(newBall, BallLifetime);
        lastThrowTime = Time.time;
    }

    void OnDestroy() {
        if (RightControllerAction != null) {
            RightControllerAction.activateAction.action.performed -= OnTriggerPressed;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Vector3 minPos = new Vector3(MinX, FixedY, FixedZ);
        Vector3 maxPos = new Vector3(MaxX, FixedY, FixedZ);
        Gizmos.DrawLine(minPos, maxPos);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (BallSpawnPoint != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(BallSpawnPoint.position, 0.15f);

            if (RightController != null) {
                Gizmos.color = Color.yellow;
                Vector3 throwDir = RightController.transform.forward * 2f;
                Gizmos.DrawRay(BallSpawnPoint.position, throwDir);
            }
        }
    }
}