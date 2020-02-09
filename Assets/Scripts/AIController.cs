using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Circuit circuit;
    public float brakingSensitivity = 1.1f;
    Drive ds;
    public float steeringSensitivity = 0.01f;
    public float accellerationSensitivity = 0.3f;
    Vector3 target;
    Vector3 nextTarget;
    int currentWP = 0;
    float totalDistanceToTarget;

    GameObject tracker;
    int currentTrackerWP = 0;
    public float lookAhead = 10;

    float lastTimeMoving = 0f;

    // Start is called before the first frame update
    void Start()
    {
        ds = this.GetComponent<Drive>();
        target = circuit.waypoints[currentWP].transform.position;
        nextTarget = circuit.waypoints[currentWP + 1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);

        tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = ds.rb.gameObject.transform.position;
        tracker.transform.rotation = ds.rb.gameObject.transform.rotation;
    }

    void ProgressTracker()
    {
        Debug.DrawLine(ds.rb.gameObject.transform.position, tracker.transform.position);

        if (Vector3.Distance(ds.rb.gameObject.transform.position, tracker.transform.position) > lookAhead)
            return;

        tracker.transform.LookAt(circuit.waypoints[currentTrackerWP].transform.position);
        tracker.transform.Translate(0, 0, 1.0f);

        if (Vector3.Distance(tracker.transform.position, circuit.waypoints[currentTrackerWP].transform.position) < 1)
        {
            currentTrackerWP++;
            if (currentTrackerWP >= circuit.waypoints.Length)
                currentTrackerWP = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ProgressTracker();
        Vector3 localTarget;
        float targetAngle;

        if (ds.rb.velocity.magnitude > 1)
            lastTimeMoving = Time.time;

        if (Time.time > lastTimeMoving + 4)
        {
            ds.rb.gameObject.transform.position = circuit.waypoints[currentTrackerWP].transform.position + Vector3.up * 2;
            transform.transform.position = ds.rb.gameObject.transform.position;
        }

        if (Time.time < ds.rb.GetComponent<AvoidDetector>().avoidTime)
        {
            localTarget = tracker.transform.right * ds.rb.GetComponent<AvoidDetector>().avoidPath;
        } else
        {
            localTarget = ds.rb.gameObject.transform.InverseTransformPoint(tracker.transform.position);
        }
        targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(ds.CurrentSpeed);

        float speedFactor = ds.CurrentSpeed / ds.maxSpeed;

        float corner = Mathf.Clamp(Mathf.Abs(targetAngle), 0, 90);
        float cornerFactor = corner / 90.0f;

        float brake = 0;
        if (corner > 10 && speedFactor > 0.1f)
            brake = Mathf.Lerp(0, 1 + speedFactor * brakingSensitivity, cornerFactor);

        float accel = 1f;
        if (corner > 20 && speedFactor > 0.2f)
            accel = Mathf.Lerp(0, 1 * accellerationSensitivity, 1 - cornerFactor);

        ds.Go(accel, steer, brake);

        ds.CheckForSkid();
        ds.CalculateEngineSound();
    }
}
