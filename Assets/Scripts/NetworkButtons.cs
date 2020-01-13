using TMPro;
using UnityEngine;

// Concerning the ROOM (in Photon) : place the player has to join to play with others (either a random or a certain room by name or from the Room List)
public class NetworkButtons : MonoBehaviour
{
    public static NetworkButtons Instance;

    // Need to be public so as to link input field in Unity canvas in these attributes
    public TMP_InputField createRoomInput;
    public TMP_InputField joinRoomInput;
    public TMP_InputField usernameInput;
    // For the dropdown menu
    public TMP_Dropdown joinRoomInputDropdown;
    public GameObject roomNameInput;

    public GameObject connexionMenuView;

    private GameObject usernameGO;

    private void Awake()
    {
        usernameGO = GameObject.FindGameObjectWithTag("Username");

        if (Instance == null)
            Instance = this;
    }

    public void _OnClickCreateRoom()
    {
        Debug.Log("NetworkButtons - _OnClickCreateRoom()");

        bool bug = true;
        if (createRoomInput.text.Length > 0 && usernameInput.text.Length > 0)
        {
            Debug.Log("Name of the room : " + createRoomInput.text);

            usernameGO.name = usernameInput.text;
            PhotonNetwork.CreateRoom(createRoomInput.text, new RoomOptions() { MaxPlayers = 3 }, null);

            bug = false;
        }
        if (usernameInput.text.Length == 0)
        {
            Debug.Log("Please enter username of length > 0");

            usernameInput.placeholder.color = Color.red;
            bug = false;
        }
        if (createRoomInput.text.Length == 0)
        {
            Debug.Log("Please enter room name of length > 0");

            createRoomInput.placeholder.color = Color.red;
            bug = false;
        }
        if (bug)
            Debug.Log("Room cannot be joined");
    }

    public void _OnClickJoinRoom()
    {
        Debug.Log("NetworkButtons - _OnClickJoinRoom()");

        bool bug = true;
        if (joinRoomInput.text.Length > 0 && 
            usernameInput.text.Length > 0)
        {
            Debug.Log(joinRoomInput.text);
            //Debug.Log(roomNameInput.text);

            usernameGO.name = usernameInput.text;

            // Make a list of the available rooms to join and just clic on one to join the room
            PhotonNetwork.JoinRoom(joinRoomInput.text);
            //PhotonNetwork.JoinRoom(roomNameInput.GetComponent<TextMeshProUGUI>().text);

            bug = false;
        }
        if (usernameInput.text.Length == 0)
        {
            Debug.Log("Please enter username of length > 0");

            usernameInput.placeholder.color = Color.red;
            bug = false;
        }
        if (joinRoomInput.text.Length == 0)
        {
            Debug.Log("Please enter room name of length > 0");

            joinRoomInput.placeholder.color = Color.red;
            //roomNameInput.placeholder.color = Color.red;
            bug = false;
        }
        if (bug)
            Debug.Log("Room cannot be joined");


    }






    // Called when this client created a room and entered it. OnJoinedRoom() will be called as well
    private void OnCreatedRoom()
    {
        Debug.Log("NetworkButtons - OnCreatedRoom");

        connexionMenuView.SetActive(false);
    }

    // Called when the LoadBalancingClient entered a room, no matter if this client created it or simply joined
    private void OnJoinedRoom()
    {
        Debug.Log("NetworkButtons - OnJoinedRoom ");

        PhotonNetwork.LoadLevel("GameScene");
    }





    private void Update()
    {
        ///////////////////////////////////// Try to implement dropdown menu ///////////////////////////////////// 
        //if (joinRoomInputDropdown != null)
        //{
        //    var options = joinRoomInputDropdown.options;
        //    RoomInfo[] rooms = PhotonNetwork.GetRoomList();
        //    int i = 0;
        //    foreach (var input in options)
        //    {
        //        input.text = rooms[i].Name;
        //        ++i;
        //    }
        //}
    }





    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
