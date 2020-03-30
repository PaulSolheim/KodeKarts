using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameUIController : MonoBehaviour
{
    public Text playerName;
    public Text lapDisplay;
    public Transform target;
    CanvasGroup canvasGroup;
    public Renderer carRend;
    CheckpointManager cpManager;

    int carRego;
    bool regoSet = false;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        playerName = this.GetComponent<Text>();
        canvasGroup = this.GetComponent<CanvasGroup>();
    }

    void LateUpdate()
    {
        if (!RaceMonitor.racing) { canvasGroup.alpha = 0; return; }
        if (!regoSet)
        {
            carRego = LeaderBoard.RegisterCar(playerName.text);
            regoSet = true;
            return;
        }
        if (carRend == null) return;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        bool carInView = GeometryUtility.TestPlanesAABB(planes, carRend.bounds);
        canvasGroup.alpha = carInView ? 1 : 0;
        this.transform.position = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1.5f);

        if (cpManager == null)
            cpManager = target.GetComponent<CheckpointManager>();

        LeaderBoard.SetPosition(carRego, cpManager.lap, cpManager.checkPoint, cpManager.timeEntered);
        string position = LeaderBoard.GetPosition(carRego);

        lapDisplay.text = position;  // + " " + cpManager.lap + " (CP: " + cpManager.checkPoint + ")";
       
    }
}
