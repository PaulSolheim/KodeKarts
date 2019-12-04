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
    bool isJump = false;

    // Start is called before the first frame update
    void Start()
    {
        ds = this.GetComponent<Drive>();
        target = circuit.waypoints[currentWP].transform.position;
        nextTarget = circuit.waypoints[currentWP + 1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localTarget = ds.rb.gameObject.transform.InverseTransformPoint(target);
        Vector3 nextLocalTarget = ds.rb.gameObject.transform.InverseTransformPoint(nextTarget);
        float distanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);

        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float nextTargetAngle = Mathf.Atan2(nextLocalTarget.x, nextLocalTarget.z) * Mathf.Rad2Deg;

        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(ds.CurrentSpeed);

        float distanceFactor = distanceToTarget / totalDistanceToTarget;
        float speedFactor = ds.CurrentSpeed / ds.maxSpeed;
        float nextWaypointFactor = Mathf.Abs(nextTargetAngle) / 180;

        float accel = Mathf.Lerp(accellerationSensitivity, 1, distanceFactor);
        float brake = Mathf.Lerp(-1 + (nextWaypointFactor * brakingSensitivity), 1 + speedFactor, 1 - distanceFactor);

        // Next some hardcoded rules (trial and error based):

        if (speedFactor < 0.1f) { brake = 0f; }     // If pointing wrong way after flipping

        if (nextTargetAngle > 20)
        {
            if (speedFactor > 0.2f)
            {
                brake += 0.2f;
                accel -= 0.2f;
            }
        }

        if (isJump)
        {
            accel = 1;
            brake = 0;
            Debug.Log("isJump");
        }

        // Debug.Log("Brake:" + brake + " Accel: " + accel + " Speed: " + ds.rb.velocity.magnitude);

        // if (distanceToTarget < 5) { brake = 1.0f; accel = 0.1f; }

        ds.Go(accel, steer, brake);

        if (distanceToTarget < 4) //threshold, make larger if car starts to circle waypoint
        {
            currentWP++;
            if (currentWP >= circuit.waypoints.Length)
                currentWP = 0;
            target = circuit.waypoints[currentWP].transform.position;

            if (currentWP == circuit.waypoints.Length - 1)
                nextTarget = circuit.waypoints[0].transform.position;
            else
                nextTarget = circuit.waypoints[currentWP + 1].transform.position;

            totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);

            if (ds.rb.gameObject.transform.InverseTransformPoint(target).y > 4)
            {
                isJump = true;
            }
            else isJump = false;

        }

        ds.CheckForSkid();
        ds.CalculateEngineSound();
    }
}
