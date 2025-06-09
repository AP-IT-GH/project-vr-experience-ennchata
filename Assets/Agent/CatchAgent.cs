using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CatchAgent : Agent {
    [Header("References")]
    public Ball Ball;
    public Transform BallSpawnpoint;

    [Header("Movement Parameters")]
    public float MoveSpeed = 10f; // per second
    public float RotateSpeed = 180f; // per second

    [Header("Detection")]
    public LayerMask NoGoZoneMask;
    public float NoGoZoneDetectionRadius = 10f;

    [Header("Rewards")]
    public float CatchReward = 10f;
    public float BallGroundHitPenalty = 1f;
    public float NoGoZoneEnterPenalty = 3f;
    public float BallOffPlanePenalty = 5f;
    public float AgentOffPlanePenalty = 10f;
    public float MovementPenalty = 0.01f;
    public bool NewMovementPenaltyScheme = false;

    [Header("Ball Physics")]
    public float BallThrowForce = 8f;
    public Vector2 HorizonalThrowAngleRange = new Vector2(-30f, 30f);
    public Vector2 VerticalThrowAngleRange = new Vector2(-5f, 30f);

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody agentRigidbody;
    private Rigidbody ballRigidbody;
    private bool touchingBall, touchingNoGoZone, ballTouchingGround, ballTouchingGroundDebounce = false;

    // Lifecycle + phsyics methods
    private void Start() {
        transform.GetPositionAndRotation(out initialPosition, out initialRotation);
        agentRigidbody = GetComponent<Rigidbody>();
        ballRigidbody = Ball.GetComponent<Rigidbody>();

        Ball.TouchedGroundEvent.AddListener(() => {
            ballTouchingGround = true;
        });
        Ball.LeftGroundEvent.AddListener(() => {
            ballTouchingGround = false;
            ballTouchingGroundDebounce = false;
        });
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Ball")) touchingBall = true;
        else if (collision.gameObject.CompareTag("NoGoZone")) touchingNoGoZone = true;
    }

    // ML Agent methods
    public override void OnEpisodeBegin() {
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        agentRigidbody.velocity = Vector3.zero;
        agentRigidbody.angularVelocity = Vector3.zero;
        touchingBall = false;
        touchingNoGoZone = false;
        ballTouchingGround = false;
        ballTouchingGroundDebounce = false;

        Ball.transform.position = BallSpawnpoint.position;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        Vector3 direction = (transform.position - Ball.transform.position).normalized;
        float horizontalDeviation = UnityEngine.Random.Range(HorizonalThrowAngleRange.x, HorizonalThrowAngleRange.y);
        float verticalDeviation = UnityEngine.Random.Range(VerticalThrowAngleRange.x, VerticalThrowAngleRange.y);
        Vector3 deviatedDirection = Quaternion.Euler(verticalDeviation, horizontalDeviation, 0) * direction;
        ballRigidbody.AddForce(deviatedDirection * BallThrowForce, ForceMode.VelocityChange);
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation.y);
        sensor.AddObservation(transform.position - Ball.transform.position);

        sensor.AddObservation(agentRigidbody.velocity);
        sensor.AddObservation(ballRigidbody.velocity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, NoGoZoneDetectionRadius, NoGoZoneMask);
        Vector3 closestDirection = Vector3.zero;
        float closestDistance = NoGoZoneDetectionRadius;

        if (colliders.Length > 0) foreach (Collider collider in colliders) {
            Vector3 closestPoint = collider.ClosestPoint(transform.position);
            float distance = Vector3.Distance(transform.position, closestPoint);

            if (distance < closestDistance) {
                closestDistance = distance;
                closestDirection = transform.position - closestPoint;
            }
        }
        sensor.AddObservation(closestDistance / NoGoZoneDetectionRadius);
        sensor.AddObservation(closestDirection);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if (touchingBall) {
            AddReward(CatchReward);
            EndEpisode();
        } else if (touchingNoGoZone) {
            AddReward(-NoGoZoneEnterPenalty);
            EndEpisode();
        } else if (transform.localPosition.y < -1f) {
            AddReward(-AgentOffPlanePenalty);
            EndEpisode();
        } else if (Ball.transform.localPosition.y < -1f) {
            AddReward(-BallOffPlanePenalty);
            EndEpisode();
        }

        if (ballTouchingGround && !ballTouchingGroundDebounce) {
            ballTouchingGround = false;
            ballTouchingGroundDebounce = true;
            AddReward(-BallGroundHitPenalty);
        }

        float turnInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        Quaternion turnQuaternion = Quaternion.Euler(0, turnInput * RotateSpeed * Time.deltaTime, 0);
        agentRigidbody.MoveRotation(agentRigidbody.rotation * turnQuaternion);

        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        Vector3 moveVector = moveInput * MoveSpeed * Time.deltaTime * transform.forward;
        agentRigidbody.MovePosition(agentRigidbody.position + moveVector);

        if (NewMovementPenaltyScheme) {
            float turnPunishment = -MovementPenalty * 2 * Mathf.Abs(turnInput);
            float movePunishment = moveInput > 0 ? -MovementPenalty * Mathf.Abs(moveInput) : -MovementPenalty * 4 * Mathf.Abs(moveInput);
            AddReward(turnPunishment + movePunishment);
        } else AddReward(-MovementPenalty * (Mathf.Abs(moveInput) + Mathf.Abs(turnInput)));
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}
