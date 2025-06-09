using UnityEngine;
using UnityEngine.Events;

public class Ball : MonoBehaviour {
    [Header("Events")]
    public UnityEvent TouchedGroundEvent;
    public UnityEvent LeftGroundEvent;

    [Header("Ground Detection")]
    public LayerMask groundLayerMask = 1; // Default layer

    private bool isGrounded = false;
    private Rigidbody ballRigidbody;

    void Start() {
        ballRigidbody = GetComponent<Rigidbody>();

        // Ensure we have a rigidbody
        if (ballRigidbody == null) {
            ballRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        // Initialize events if null
        if (TouchedGroundEvent == null)
            TouchedGroundEvent = new UnityEvent();
        if (LeftGroundEvent == null)
            LeftGroundEvent = new UnityEvent();
    }

    void OnCollisionEnter(Collision collision) {
        // Check if touching ground
        if (IsGroundLayer(collision.gameObject.layer) && !isGrounded) {
            isGrounded = true;
            TouchedGroundEvent?.Invoke();
        }
    }

    void OnCollisionExit(Collision collision) {
        // Check if leaving ground
        if (IsGroundLayer(collision.gameObject.layer) && isGrounded) {
            isGrounded = false;
            LeftGroundEvent?.Invoke();
        }
    }

    bool IsGroundLayer(int layer) {
        return (groundLayerMask.value & (1 << layer)) != 0;
    }

    public void ResetBall(Vector3 position) {
        transform.position = position;
        if (ballRigidbody != null) {
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }
        isGrounded = false;
    }
}
