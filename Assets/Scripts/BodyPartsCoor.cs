//using UnityEngine;

public class BodyPartsCoor : Photon.MonoBehaviour
{
    public int index = 1;



    // Update is called once per frame
    private void Update()
    {
        if (photonView.isMine)
        {
            switch (index)
            {
                case 1: // Set the head position to the headset one 
                    transform.position = NetworkPlayer.Instance.headset.transform.position;
                    transform.rotation = NetworkPlayer.Instance.headset.transform.rotation;
                    break;
                case 2: // Set the left hand position to the left motion controller one 
                    transform.position = NetworkPlayer.Instance.leftController.transform.position;
                    transform.rotation = NetworkPlayer.Instance.leftController.transform.rotation;
                    break;
                case 3: // Set the right hand position to the right motion controller one 
                    transform.position = NetworkPlayer.Instance.rightController.transform.position;
                    transform.rotation = NetworkPlayer.Instance.rightController.transform.rotation;
                    break;
            }
        }
    }
}
