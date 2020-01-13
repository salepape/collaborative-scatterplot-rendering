using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInteractions : MonoBehaviour
{
    //public static NetworkInteractions Instance;
    //public bool TransferOwnershipOnRequest = true;

    //public void OnOwnershipRequest(object[] viewAndPlayer)
    //{
    //    PhotonView view = viewAndPlayer[0] as PhotonView;
    //    PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;

    //    Debug.Log("OnOwnershipRequest(): Player " + requestingPlayer + " requests ownership of: " + view + ".");
    //    if (TransferOwnershipOnRequest)
    //    {
    //        view.TransferOwnership(requestingPlayer.ID);
    //    }
    //}

    //public void OnOwnershipTransfered(object[] viewAndPlayers)
    //{
    //    PhotonView view = viewAndPlayers[0] as PhotonView;

    //    PhotonPlayer newOwner = viewAndPlayers[1] as PhotonPlayer;

    //    PhotonPlayer oldOwner = viewAndPlayers[2] as PhotonPlayer;

    //    Debug.Log("OnOwnershipTransfered for PhotonView" + view.ToString() + " from " + oldOwner + " to " + newOwner);
    //}
}
