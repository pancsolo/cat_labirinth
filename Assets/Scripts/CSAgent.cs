using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CatAgent : Agent
{
    public Transform target;

    public float moveSpeed = 5f; 
    public float rotateSpeed = 200f;

    private Rigidbody rb;
    private float previousDistance;

    private Vector3 startPosition;
    private Quaternion startRotation;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        if (target == null)
        {
            GameObject t = GameObject.FindWithTag("Target");
            if (t != null) target = t.transform;
            else Debug.LogError("Nincs Target a jelenetben!");
        }
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.localPosition = startPosition;
        transform.localRotation = startRotation;


        previousDistance = Vector3.Distance(transform.localPosition, target.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.InverseTransformPoint(target.position));

        sensor.AddObservation(transform.InverseTransformVector(rb.velocity));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveForward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rotate = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 move = transform.forward * moveForward * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + move);

        transform.Rotate(Vector3.up, rotate * rotateSpeed * Time.deltaTime);

        float currentDistance = Vector3.Distance(transform.localPosition, target.localPosition);

        if (currentDistance < previousDistance)
        {
            AddReward(0.01f); 
        }

        previousDistance = currentDistance;

        
        AddReward(-0.005f);

        
        if (transform.localPosition.y < -1f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            SetReward(1.0f);
            EndEpisode();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            
            SetReward(-0.5f);
            EndEpisode();
        }
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            SetReward(-0.5f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");   
        continuousActionsOut[1] = Input.GetAxis("Horizontal"); 
    }
}