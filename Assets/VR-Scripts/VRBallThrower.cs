using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VRBallThrower : MonoBehaviour {
    [Header("References")]
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;
    public CatchAgent mlAgent;
    public LineRenderer aimLine;

    [Header("Throwing Parameters")]
    public float throwForce = 8f;
    public float ballLifetime = 10f;
    public int aimLinePoints = 50;
    public float trajectoryTimeStep = 0.1f;

    private GameObject currentBall;
    private bool isAiming = false;
    private bool lastTriggerState = false;

    // We'll manually access this action by name
    private InputAction triggerAction;

    void Start() {
        // Setup line
        if (aimLine != null) {
            aimLine.positionCount = aimLinePoints;
            aimLine.enabled = false;
        }

        // Manually find the RightHand trigger action
        var inputActionAsset = Resources.Load<InputActionAsset>("XRI Default Input Actions");
        if (inputActionAsset != null) {
            triggerAction = inputActionAsset.FindAction("XRI RightHand Interaction/Select", true);
            triggerAction.Enable();
        } else {
            Debug.LogError("Could not find 'XRI Default Input Actions' in Resources folder.");
        }
    }

    void Update() {
        if (triggerAction == null) return;

        bool triggerPressed = triggerAction.ReadValue<float>() > 0.5f;

        if (triggerPressed && !lastTriggerState) {
            StartAiming();
        } else if (!triggerPressed && lastTriggerState) {
            if (isAiming) {
                ThrowBall();
                StopAiming();
            }
        }

        lastTriggerState = triggerPressed;

        if (isAiming) {
            UpdateAimLine();
        }
    }

    void StartAiming() {
        isAiming = true;
        if (aimLine != null) aimLine.enabled = true;
    }

    void StopAiming() {
        isAiming = false;
        if (aimLine != null) aimLine.enabled = false;
    }

    void UpdateAimLine() {
        if (!isAiming || aimLine == null) return;

        aimLine.positionCount = aimLinePoints;

        Vector3 throwDirection = transform.forward;
        Vector3 startPosition = ballSpawnPoint.position;

        for (int i = 0; i < aimLinePoints; i++) {
            float time = i * trajectoryTimeStep;
            Vector3 point = CalculateTrajectoryPoint(startPosition, throwDirection * throwForce, time);
            aimLine.SetPosition(i, point);

            if (point.y <= 0f) {
                for (int j = i + 1; j < aimLinePoints; j++) {
                    aimLine.SetPosition(j, point);
                }
                break;
            }
        }
    }

    Vector3 CalculateTrajectoryPoint(Vector3 startPos, Vector3 velocity, float time) {
        return startPos + velocity * time + 0.5f * Physics.gravity * time * time;
    }

    void ThrowBall() {
        if (ballPrefab == null || ballSpawnPoint == null) return;

        if (currentBall != null) Destroy(currentBall);

        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);

        Ball ballComponent = currentBall.GetComponent<Ball>();
        if (ballComponent == null) ballComponent = currentBall.AddComponent<Ball>();

        if (mlAgent != null) {
            mlAgent.Ball = ballComponent;
            mlAgent.EndEpisode();
        }

        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        if (ballRb != null) {
            ballRb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
        }

        Destroy(currentBall, ballLifetime);
    }
}
