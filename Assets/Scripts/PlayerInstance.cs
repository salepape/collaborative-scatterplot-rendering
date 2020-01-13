using UnityEngine;
using TMPro;

// Forced to write another script because when headPrefabInstance exists, must have a detached script attached from it
public class PlayerInstance : Photon.MonoBehaviour
{
    public static PlayerInstance Instance;

    [System.NonSerialized]
    public Vector4 playerColor;

    private GameObject usernameContainer;

    [System.NonSerialized]
    public TextMeshPro username;

    // PV = Photon View
    private PhotonView PVLocalPlayer;



    // Awake is called before the game starts, ideal for variable initializations
    private void Awake()
    {
        // The if-statement below will only be applied to the player instance corresponding to the computer where the script IS running
        playerColor = new Vector4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);

        usernameContainer = GameObject.FindGameObjectWithTag("Username");
        usernameContainer.transform.parent = transform;

        username = usernameContainer.AddComponent<TextMeshPro>();
        username.fontSize = 1;

        PVLocalPlayer = photonView;
    }

    // Start is called before the first frame update, is run only is the game object where the script is attached on is enabled
    private void Start()
    {
        // The if-statement below will only be applied to the player instance corresponding to the computer where the script IS running
        if (photonView.isMine)
        {
            GetComponent<MeshRenderer>().material.color = playerColor;

            username.text = usernameContainer.name;
            username.autoSizeTextContainer = true;
            username.color = playerColor;

            // Send the previous information to all other players (clients) in the room
            photonView.RPC("DisplayNameAndColor", PhotonTargets.OthersBuffered, (Vector3)playerColor, username.text);
        }
    }

    // All clients execute the RPC method only on the networked GameObject with the PhotonView on which the RPC method is applied on.    
    [PunRPC]
    public void DisplayNameAndColor(Vector3 RPCplayerColor, string RPCtext, PhotonMessageInfo info)
    {
        Debug.Log("PlayerInstance - Pass in PunRPC function ! The sender is : " + info.sender);

        GetComponent<MeshRenderer>().material.color = new Vector4(RPCplayerColor.x, RPCplayerColor.y, RPCplayerColor.z, 1.0f);

        username.text = RPCtext;
        username.gameObject.name = RPCtext;
        username.autoSizeTextContainer = true;
        username.color = new Vector4(RPCplayerColor.x, RPCplayerColor.y, RPCplayerColor.z, 1.0f);
    }

    // Update is called once per frame
    private void Update()
    {
        // Position of username following the head
        usernameContainer.transform.position = transform.position + Vector3.up * 0.5f;

        // Make the names be oriented according to the camera position (looking in the opposite direction of the lookAt one)
        if(!photonView.isMine)
            usernameContainer.transform.rotation = Quaternion.LookRotation(usernameContainer.transform.position - GameObject.FindGameObjectWithTag("MainCamera").transform.position);
    }
}








