using System.Collections.Generic;
using UnityEngine;

public class WorldMapping : MonoBehaviour
{
    public GameObject roomCreator;
    public GameObject roomClient;

    public GameObject caliPos1;
    public GameObject creatorBase1;
    public GameObject creatorBase2;
    public GameObject clientBase1;
    public GameObject clientBase2;



    private void Start()
    {
        // If we are the master client, no need to be preoccupied by the client base stations because master doesn't have to do world mapping
        if (PhotonNetwork.isMasterClient)
        {
            Destroy(roomClient);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && !PhotonNetwork.isMasterClient)
        {
            Debug.Log("WorldMapping - Space key pressed !");
            _OnEnterWorldMapping();
        }
    }

    // Only has an impact for the clients, not for the creator
    public void _OnEnterWorldMapping()
    {
        Debug.Log("WorldMapping - _OnEnterWorldMapping");

        // Only one base station is needed to compute calibration (index 0 is chosen by default)
        GameObject NetworkPlayer = GameObject.FindGameObjectWithTag("Player");

        // Verifies if index of base stations have not been inversed between server and client base stations
        List<string> indexBaseCreator = roomCreator.GetComponent<BaseStationsCoor>().stringIndexBaseStations;
        List<string> indexBaseClient = roomClient.GetComponent<BaseStationsCoor>().stringIndexBaseStations;

        // CaliPos1 is a temporary variable (dump location) that saves transform values of ClientBase1
        caliPos1.transform.SetParent(clientBase1.transform, true);

        // indexBaseCreator[0] always different to indexBaseClient[0]
        if (indexBaseCreator[1] != indexBaseClient[1])
        {
            clientBase1.transform.position = creatorBase2.transform.position;
            clientBase1.transform.rotation = creatorBase2.transform.rotation;

            clientBase2.transform.position = creatorBase1.transform.position;
            clientBase2.transform.rotation = creatorBase1.transform.rotation;
        }
        else
        {
            clientBase1.transform.position = creatorBase1.transform.position;
            clientBase1.transform.rotation = creatorBase1.transform.rotation;

            clientBase2.transform.position = creatorBase2.transform.position;
            clientBase2.transform.rotation = creatorBase2.transform.rotation;
        }

        // Removes the parent relationship 
        caliPos1.transform.parent = null;

        NetworkPlayer.transform.position = caliPos1.transform.position;
        NetworkPlayer.transform.rotation = caliPos1.transform.rotation;
    }
}
