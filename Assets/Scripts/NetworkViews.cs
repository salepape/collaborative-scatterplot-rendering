using UnityEngine;
using System.Collections;

// Concerning the LOBBY (in Photon) : place where all available rooms are listed
public class NetworkViews : MonoBehaviour
{
    public static NetworkViews Instance;

    public GameObject loadingView;
    public GameObject connectionView;
    public GameObject connectionFailedView;
    public GameObject connectionMenuView;



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Joins the lobby connecting to the photon server
        PhotonNetwork.ConnectUsingSettings("1.0");
    }

    // Called when the client is connected to the Master Server and ready for matchmaking and other tasks
    private void OnConnectedToMaster()
    {
        Debug.Log("NetworkButtons - OnConnectedToMaster");

        // Synchronizes every client to the scene in which the master client is currently on
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    // Called on entering a lobby on the Master Server
    private IEnumerator OnJoinedLobby()
    {
        Debug.Log("NetworkViews - OnJoinedLobby");

        loadingView.SetActive(false);
        connectionView.SetActive(true);

        yield return new WaitForSeconds(2);

        connectionView.SetActive(false);
        connectionMenuView.SetActive(true);
    }

    // Called after disconnecting from the Photon server. It could be a failure or intentional
    private void OnDisconenected()
    {
        Debug.Log("NetworkViews - OnDisconenected");

        if (loadingView == true)
            loadingView.SetActive(false);

        if (connectionView == true)
            connectionView.SetActive(false);

        connectionFailedView.SetActive(true);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

