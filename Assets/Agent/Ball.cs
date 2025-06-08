using UnityEngine;
using UnityEngine.Events;

public class Ball : MonoBehaviour {
    public UnityEvent TouchedGroundEvent;
    public UnityEvent LeftGroundEvent;

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Ground")) TouchedGroundEvent.Invoke();
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject.CompareTag("Ground")) LeftGroundEvent.Invoke();
    }
}
