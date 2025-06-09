using UnityEngine;

public class TrainingAreaManager : MonoBehaviour {
    [Header("References")]
    public CatchAgent catchAgent;
    public VRBallThrower ballThrower;
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;

    [Header("Initialization")]
    public bool createInitialBall = true;

    void Start() {
        InitializeSystem();
    }

    void InitializeSystem() {
        // Ensure all references are set
        if (catchAgent == null)
            catchAgent = FindObjectOfType<CatchAgent>();

        if (ballThrower == null)
            ballThrower = FindObjectOfType<VRBallThrower>();

        // Set up ball thrower references
        if (ballThrower != null) {
            ballThrower.ballPrefab = ballPrefab;
            ballThrower.ballSpawnPoint = ballSpawnPoint;
            ballThrower.mlAgent = catchAgent;
        }

        // Create initial ball to prevent null reference errors
        if (createInitialBall && catchAgent != null && ballPrefab != null) {
            CreateInitialBall();
        }

        // Set up catch agent references
        if (catchAgent != null) {
            catchAgent.BallSpawnpoint = ballSpawnPoint;
        }
    }

    void CreateInitialBall() {
        GameObject initialBall = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
        Ball ballComponent = initialBall.GetComponent<Ball>();

        if (ballComponent == null) {
            ballComponent = initialBall.AddComponent<Ball>();
        }

        catchAgent.Ball = ballComponent;

        // Make sure the ball is stationary initially
        Rigidbody ballRb = initialBall.GetComponent<Rigidbody>();
        if (ballRb != null) {
            ballRb.velocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.useGravity = true;
        }
    }

    public void ResetTrainingArea() {
        if (catchAgent != null) {
            catchAgent.EndEpisode();
        }
    }
}
