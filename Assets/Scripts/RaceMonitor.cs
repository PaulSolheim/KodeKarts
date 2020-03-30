using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Realtime;
using Photon.Pun;

public class RaceMonitor : MonoBehaviourPunCallbacks
{
    public GameObject[] countDownItems;
    CheckpointManager[] carsCPM;

    public GameObject[] carPrefabs;
    public Transform[] spawnPos;

    public static bool racing = false;
    public static int totalLaps = 1;
    public GameObject gameOverPanel;
    public GameObject HUD;

    public GameObject startRace;
    public GameObject waitingText;

    int playerCar;

    // Start is called before the first frame update
    void Start()
    {
        racing = false;

        foreach (GameObject g in countDownItems)
            g.SetActive(false);

        gameOverPanel.SetActive(false);

        startRace.SetActive(false);
        waitingText.SetActive(false);

        playerCar = PlayerPrefs.GetInt("PlayerCar");
        int randomStartPos = Random.Range(0, spawnPos.Length);
        Vector3 startPos = spawnPos[randomStartPos].position;
        Quaternion startRot = spawnPos[randomStartPos].rotation;
        GameObject pcar = null;

        if (PhotonNetwork.IsConnected)
        {
            startPos = spawnPos[PhotonNetwork.LocalPlayer.ActorNumber - 1].position;
            startRot = spawnPos[PhotonNetwork.LocalPlayer.ActorNumber - 1].rotation;

            if (NetworkedPlayer.LocalPlayerInstance == null)
            {
                pcar = PhotonNetwork.Instantiate(carPrefabs[playerCar].name, startPos, startRot, 0);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                startRace.SetActive(true);
            }
            else
            {
                waitingText.SetActive(true);
            }
        }
        else
        {
            pcar = Instantiate(carPrefabs[playerCar]);
            pcar.transform.position = startPos;
            pcar.transform.rotation = startRot;

            foreach (Transform t in spawnPos)
            {
                if (t == spawnPos[randomStartPos]) continue;
                GameObject car = Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)]);
                car.transform.position = t.position;
                car.transform.rotation = t.rotation;
            }

            StartGame();
        }

        SmoothFollow.playerCar = pcar.gameObject.GetComponent<Drive>().rb.transform;
        pcar.GetComponent<AIController>().enabled = false;
        pcar.GetComponent<Drive>().enabled = true;
        pcar.GetComponent<PlayerController>().enabled = true;

    }

    public void BeginGame()
    {
        string[] aiNames = { "Trevor", "Nick", "Penny", "Bob", "Dolly", "Alice", "Donald", "Phil", "Betty", "Sally", "John", "Carl", "Lisa", "Nelly", "Ginger", "Ted", "Emma", "Henry" };
        int numAIPlayers = PhotonNetwork.CurrentRoom.MaxPlayers - PhotonNetwork.CurrentRoom.PlayerCount;
        for (int i = PhotonNetwork.CurrentRoom.PlayerCount; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            Vector3 startPos = spawnPos[i].position;
            Quaternion startRot = spawnPos[i].rotation;
            int r = Random.Range(0, carPrefabs.Length);

            object[] instanceData = new object[1];
            instanceData[0] = (string)aiNames[Random.Range(0, aiNames.Length)];

            GameObject AICar = PhotonNetwork.Instantiate(carPrefabs[r].name, startPos, startRot, 0, instanceData);
            AICar.GetComponent<AIController>().enabled = true;
            AICar.GetComponent<Drive>().enabled = true;
            AICar.GetComponent<Drive>().networkName = (string)instanceData[0];
            AICar.GetComponent<PlayerController>().enabled = false;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGame", RpcTarget.All, null);
        }
    }

    [PunRPC]
    public void StartGame()
    {
        StartCoroutine(PlayCountDown());
        startRace.SetActive(false);
        waitingText.SetActive(false);

        GameObject[] cars = GameObject.FindGameObjectsWithTag("car");
        carsCPM = new CheckpointManager[cars.Length];
        for (int i = 0; i < cars.Length; i++)
            carsCPM[i] = cars[i].GetComponent<CheckpointManager>();
    }

    IEnumerator PlayCountDown()
    {
        yield return new WaitForSeconds(2);
        foreach (GameObject g in countDownItems)
        {
            g.SetActive(true);
            yield return new WaitForSeconds(1);
            g.SetActive(false);
        }
        racing = true;
    }

    [PunRPC]
    public void RestartGame()
    {
        PhotonNetwork.LoadLevel("Track1");
    }

    public void RestartLevel()
    {
        racing = false;
        if (PhotonNetwork.IsConnected)
            photonView.RPC("RestartGame", RpcTarget.All, null);
        else
            SceneManager.LoadScene("Track1");
    }

    public void MainMenu()
    {
        racing = false;
        SceneManager.LoadScene("MainMenu");
    }

    /*
    bool raceOver = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            raceOver = true;
    }
    */

    void LateUpdate()
    {
        if (!racing) return;
        int finishedCount = 0;
        foreach (CheckpointManager cpm in carsCPM)
        {
            if (cpm.lap == totalLaps + 1)
                finishedCount++;
        }
        if (finishedCount == carsCPM.Length)
        {
            HUD.SetActive(false);
            gameOverPanel.SetActive(true);
        }
    }
}
