using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drive : MonoBehaviour
{
    public WheelCollider[] WCs;
    public GameObject[] Wheels;
    public float torque = 200;
    public float maxSteerAngle = 30.0f;
    public float maxBrakeTorque = 500.0f;

    public AudioSource skidSound;
    public AudioSource highAccel;

    public Transform SkidTrailPrefab;
    Transform[] skidTrails = new Transform[4];

    public ParticleSystem smokePrefab;
    ParticleSystem[] skidSmoke = new ParticleSystem[4];

    public GameObject brakeLight;

    public Rigidbody rb;
    public float gearLength = 3;
    public float CurrentSpeed { get { return rb.velocity.magnitude * gearLength; } }
    public float lowPitch = 1f;
    public float highPitch = 6f;
    public int numGears = 5;
    float rpm;
    int currentGear = 1;
    float currentGearPerc;
    public float maxSpeed = 200;

    public GameObject playerNamePrefab;
    public Renderer jeepMesh;

    public string networkName = "";
    string[] aiNames = { "Trevor", "Nick", "Penny", "Bob", "Dolly", "Alice", "Donald", "Phil", "Betty", "Sally", "John", "Carl", "Lisa", "Nelly", "Ginger", "Ted", "Emma", "Henry" };

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            skidSmoke[i] = Instantiate(smokePrefab);
            skidSmoke[i].Stop();
        }
        brakeLight.SetActive(false);

        GameObject playerName = Instantiate(playerNamePrefab);
        playerName.GetComponent<NameUIController>().target = rb.gameObject.transform;
        if (this.GetComponent<AIController>().enabled)
            if (networkName != "")
                playerName.GetComponent<Text>().text = networkName;
            else
                playerName.GetComponent<Text>().text = aiNames[Random.Range(0, aiNames.Length - 1)];
        else
            playerName.GetComponent<Text>().text = PlayerPrefs.GetString("PlayerName");

        playerName.GetComponent<NameUIController>().carRend = jeepMesh;
    }

    public void CalculateEngineSound()
    {
        float gearPercentage = (1 / (float)numGears);
        float targetGearFactor = Mathf.InverseLerp(gearPercentage * currentGear, gearPercentage * (currentGear + 1),
                                                    Mathf.Abs(CurrentSpeed / maxSpeed));
        currentGearPerc = Mathf.Lerp(currentGearPerc, targetGearFactor, Time.deltaTime * 5f);

        var gearNumFactor = currentGear/ (float) numGears;
        rpm = Mathf.Lerp(gearNumFactor, 1, currentGearPerc);

        float speedPercentage = Mathf.Abs(CurrentSpeed / maxSpeed);
        float upperGearMax = (1 / (float)numGears) * (currentGear + 1);
        float downGearMax = (1 / (float)numGears) * currentGear;

        if (currentGear > 0 && speedPercentage < downGearMax)
            currentGear--;

        if (speedPercentage > upperGearMax && (currentGear < (numGears - 1)))
            currentGear++;

        float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);
        highAccel.pitch = Mathf.Min(highPitch, pitch) * 0.25f;

    }

    public void Go(float accel, float steer, float brake)
    {
        accel = Mathf.Clamp(accel, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1) * maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;

        if (brake != 0)
            brakeLight.SetActive(true);
        else
            brakeLight.SetActive(false);

        float thrustTorque = 0; 
        if(CurrentSpeed < maxSpeed)
            thrustTorque = accel * torque;

        for (int i = 0; i < 4; i++)
        {
            WCs[i].motorTorque = thrustTorque;

            if (i < 2)
                WCs[i].steerAngle = steer;
            else
                WCs[i].brakeTorque = brake;

            WCs[i].GetWorldPose(out Vector3 position, out Quaternion quat);
            Wheels[i].transform.position = position;
            Wheels[i].transform.rotation = quat;
        }
    }


    public void CheckForSkid()
    {
        int numSkidding = 0;
        for (int i = 0; i < 4; i++)
        {
            WCs[i].GetGroundHit(out WheelHit wheelHit);

            if (Mathf.Abs(wheelHit.forwardSlip) >= 0.4f || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.4f)
            {
                numSkidding++;
                if (!skidSound.isPlaying)
                {
                    skidSound.Play();
                }
                skidSmoke[i].transform.position = WCs[i].transform.position - WCs[i].transform.up * WCs[i].radius;
                skidSmoke[i].Emit(1);
            }
        }

        if (numSkidding == 0 && skidSound.isPlaying)
        {
            skidSound.Stop();
        }
    }

}

