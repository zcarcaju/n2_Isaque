using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Ball : Agent
{
    Rigidbody rb;
    Coroutine stopCoroutine;
    bool portalAToB;
    bool portalBToA;
    bool hasRewardTeleport;
    
    [SerializeField]
    Transform target;
    [SerializeField]
    Transform portalA;
    [SerializeField]
    Transform portalB;
    [SerializeField]
    float ballY;
    [SerializeField]
    float portalsY;
    [SerializeField]
    float zero;
    [SerializeField]
    float portalAZ;
    [SerializeField]
    float portalBZ;
    [SerializeField]
    float bigReward;
    [SerializeField]
    float smallReward;
    [SerializeField]
    float negativeReward;
    [SerializeField]
    float speed;
    [SerializeField]
    float portalsXLimit1;
    [SerializeField]
    float portalsXLimit2;
    [SerializeField]
    float targetXLimit1;
    [SerializeField]
    float targetXLimit2;
    [SerializeField]
    float targetZLimit1;
    [SerializeField]
    float targetZLimit2;
    [SerializeField]
    float distTarget;
    [SerializeField]
    float dropHeight;
    [SerializeField]
    float stopCoroutineTime;
    [SerializeField]
    Vector3 offsetPortalB;
    [SerializeField]
    Vector3 offsetPortalA;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        if (transform.localPosition.y < dropHeight)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
        }

        stopCoroutine = StartCoroutine(Restart());
        transform.localPosition = new Vector3(zero, ballY, zero);
        target.localPosition = new Vector3(Random.Range(targetXLimit1, targetXLimit2), ballY, Random.Range(targetZLimit1, targetZLimit2));
        portalA.localPosition = new Vector3(Random.Range(portalsXLimit1, portalsXLimit2), portalsY, portalAZ);
        portalB.localPosition = new Vector3(Random.Range(portalsXLimit1, portalsXLimit2), portalsY, portalBZ);
        portalAToB = false;
        portalBToA = false;
        hasRewardTeleport = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(portalA.localPosition);
        sensor.AddObservation(portalB.localPosition);

        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rb.AddForce(controlSignal * speed);

        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);

        if (distanceToTarget < distTarget)
        {
            SetReward(bigReward);
            OnEnd();
            Debug.Log("Big Reward");
        }

        if (transform.localPosition.y < dropHeight || portalBToA)
        {
            SetReward(negativeReward);
            OnEnd();
            Debug.Log("Negative Reward");
        }

        if (portalAToB && !hasRewardTeleport)
        {
            SetReward(smallReward);
            hasRewardTeleport = true;
            Debug.Log("Small Reward");
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PortalA"))
        {
            Teleport("b");
        }
        if (other.gameObject.CompareTag("PortalB"))
        {
            Teleport("a");
        }
    }

    void Teleport(string portalTo)
    {
        switch (portalTo)
        {
            case "a":
                transform.localPosition = portalA.localPosition + offsetPortalA;
                portalBToA = true;
                break;
            case "b":
                transform.localPosition = portalB.localPosition + offsetPortalB;
                portalAToB = true;
                break;
        }
    }

    void OnEnd()
    {
        StopCoroutine(stopCoroutine);
        EndEpisode();
    }

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(stopCoroutineTime);
        SetReward(negativeReward);
        OnEnd();
    }
}
