using UnityEngine;

public class NetworkPlayer : Photon.MonoBehaviour
{
    public static NetworkPlayer Instance;

    public GameObject headset;
    public GameObject leftController;
    public GameObject rightController;

    public GameObject headPrefab;
    public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;

    //public Vector3 realPosition;



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update  
    private void Start()
    {
        Debug.Log("NetworkPlayer - Start");

        // PLAYER NAME
        PhotonNetwork.player.NickName = GameObject.FindGameObjectWithTag("Username").name;

        // PLAYER MODELLING
        GameObject headPrefabInstance = PhotonNetwork.Instantiate(headPrefab.name, headPrefab.transform.position, headPrefab.transform.rotation, 0);
        //headPrefabInstance.transform.parent = headset.transform;

        GameObject leftHandPrefabInstance = PhotonNetwork.Instantiate(leftHandPrefab.name, leftHandPrefab.transform.position, leftHandPrefab.transform.rotation, 0);
        //leftHandPrefabInstance.transform.parent = leftController.transform;

        GameObject rightHandPrefabInstance = PhotonNetwork.Instantiate(rightHandPrefab.name, rightHandPrefab.transform.position, rightHandPrefab.transform.rotation, 0);
        //rightHandPrefabInstance.transform.parent = rightController.transform;

        // All cube interactible objects will belong to the master client
        if (PhotonNetwork.isMasterClient)
        {
            GameObject[] listCubes = GameObject.FindGameObjectsWithTag("CubeInteractible");
            foreach (GameObject cube in listCubes)
            {
                // IF SIMPLE CUBE WRAPPER IS USED
                cube.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.masterClient);

                // IF CUBE WRAPPED IS USED
                //cube.transform.parent.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player.ID);

                // IF CUBE WRAPPER IS USED
                //cube.GetComponentInChildren<PhotonView>().TransferOwnership(PhotonNetwork.player.ID);
                //Debug.Log("Owner of " + cube.GetComponentInChildren<PhotonView>() + " : " + PhotonNetwork.player);
            }

        }
    }

    private void Update()
    {
        ////if (!photonView.isMine)
        ////{
        ////    //Each player keep track of each other's positions IF the script is attached to every player
        ////    transform.position = Vector3.Lerp(transform.position, realPosition, .15f);
        ////}
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}




//// Use this function because movements of cube have to be followed in a continuum
//#region IPunObservable implementation
//void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//{
//    //Debug.Log("PlayerInstance - OnPhotonSerializeView");

//        //if (stream.isWriting && !PhotonNetwork.isMasterClient)
//        //else if (stream.isReading && PhotonNetwork.isMasterClient)

//        // Client needs to send manipulation of the cube infos for the master client
//        if (stream.isWriting)
//        {
//            stream.SendNext(listOfInteractibles[index].transform.position);
//            stream.SendNext(listOfInteractibles[index].transform.rotation);
//        }
//        else if (stream.isReading)
//        {
//            // Get cube movements from the client that is currently grabbing a cube
//            Vector3 cubePos = (Vector3)stream.ReceiveNext();
//            Quaternion cubeRot = (Quaternion)stream.ReceiveNext();

//            // If the client corresponding to that computer doesn't control this cube 
//            if (!photonView.isMine)
//            {
//                listOfInteractibles[index].transform.position = cubePos;
//                listOfInteractibles[index].transform.rotation = cubeRot;
//            }
//        }
//}
//#endregion