using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MotionControllersEvents : MonoBehaviour
{
    public SteamVR_Action_Boolean action = null;

    private SteamVR_Behaviour_Pose pose = null;
    public FixedJoint joint = null;

    public CustomizedInteractible currentInteractible = null;
    public List<CustomizedInteractible> contactInteractibles = new List<CustomizedInteractible>();



    private void Awake()
    {
        pose = GetComponent<SteamVR_Behaviour_Pose>();
        joint = GetComponent<FixedJoint>();
    }

    // IF SIMPLE CUBE IS USED
    private void OnTriggerEnter(Collider objCol)
    {
        //Debug.Log("MotionControllersEvents - OnTriggerEnter : " + other.gameObject.name);

        if (objCol.gameObject.CompareTag("CubeInteractible") || objCol.gameObject.CompareTag("ClippingPlaneInteractible") || objCol.gameObject.CompareTag("ClippingPlaneInteractible2") || objCol.gameObject.CompareTag("ClippingBoxInteractible"))
        {
            if (objCol.gameObject.GetComponent<CustomizedInteractible>().GetComponent<PhotonView>().ownerId == PhotonNetwork.player.ID)
                Debug.Log("Owner in OnTriggerEnter");
            else
            {
                Debug.Log("Client request ownership in OnTriggerEnter ");
                objCol.gameObject.GetComponent<CustomizedInteractible>().GetComponent<PhotonView>().RequestOwnership();
            }

            objCol.gameObject.GetComponentInChildren<CustomizedInteractible>().GetComponent<Renderer>().material = objCol.gameObject.GetComponent<CustomizedInteractible>().enlightenedMaterial;
            contactInteractibles.Add(objCol.gameObject.GetComponent<CustomizedInteractible>());
            //Debug.Log("now one more item in contactInteractibles list : " + other.gameObject.GetComponent<CustomizedInteractible>());
        }
    }

    private void OnTriggerExit(Collider objCol)
    {
        //Debug.Log("MotionControllersEvents - OnTriggerExit : " + other.gameObject.name);

        if (objCol.gameObject.CompareTag("CubeInteractible") || objCol.gameObject.CompareTag("ClippingPlaneInteractible") || objCol.gameObject.CompareTag("ClippingPlaneInteractible2") || objCol.gameObject.CompareTag("ClippingBoxInteractible"))
        {
            contactInteractibles.Remove(objCol.gameObject.GetComponent<CustomizedInteractible>());
            objCol.gameObject.GetComponentInChildren<CustomizedInteractible>().GetComponent<Renderer>().material = objCol.gameObject.GetComponent<CustomizedInteractible>().basicMaterial;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Trigger is pressed
        if (action.GetStateDown(pose.inputSource))
        {
            Debug.Log("MotionControllersEvents - Trigger down : " + pose.inputSource + " of " + PhotonNetwork.player.ID);
            OnControllerDrag();
        }

        // Trigger is released
        if (action.GetStateUp(pose.inputSource))
        {
            Debug.Log("MotionControllersEvents - Trigger up : " + pose.inputSource + " of " + PhotonNetwork.player.ID);
            OnControllerDrop();
        }
    }

    public void OnControllerDrag()
    {
        // Get the nearest interactible object 
        currentInteractible = GetNearestInteractible();

        // Checks if the function returns a value 
        if (currentInteractible)
        {
            if (currentInteractible.CompareTag("ClippingPlaneInteractible") || currentInteractible.CompareTag("ClippingPlaneInteractible2"))
            {
                currentInteractible.transform.parent.parent.parent.GetComponent<Rigidbody>().useGravity = true;
                currentInteractible.transform.parent.parent.parent.GetComponent<Rigidbody>().isKinematic = false;

                // Moves the position of the interactible object to the motion controller one
                Vector3.MoveTowards(transform.position, currentInteractible.transform.parent.parent.parent.position, Time.deltaTime);

                // Attaches the interactible object to the motion controller via the sphere
                Rigidbody targetBody = currentInteractible.transform.parent.parent.parent.GetComponent<Rigidbody>();
                joint.connectedBody = targetBody;

                // Indicates that the active controller is this one
                currentInteractible.activeController = this;
            }
            else if (currentInteractible.CompareTag("ClippingBoxInteractible") || currentInteractible.CompareTag("CubeInteractible"))
            {
                currentInteractible.transform.parent.GetComponent<Rigidbody>().useGravity = true;
                currentInteractible.transform.parent.GetComponent<Rigidbody>().isKinematic = false;

                // Moves the position of the interactible object to the motion controller one
                Vector3.MoveTowards(transform.position, currentInteractible.transform.parent.position, Time.deltaTime);

                // Attaches the interactible object to the motion controller via the sphere
                Rigidbody targetBody = currentInteractible.transform.parent.GetComponent<Rigidbody>();
                joint.connectedBody = targetBody;

                // Indicates that the active controller is this one
                currentInteractible.activeController = this;
            }
        }
    }

    public void OnControllerDrop()
    {
        // Checks if we have something to drop
        if (currentInteractible)
        {
            if (currentInteractible.CompareTag("ClippingPlaneInteractible") || currentInteractible.CompareTag("ClippingPlaneInteractible2"))
            {
                currentInteractible.transform.parent.parent.parent.GetComponent<Rigidbody>().useGravity = false;
                currentInteractible.transform.parent.parent.parent.GetComponent<Rigidbody>().isKinematic = true;
            }
            else if (currentInteractible.CompareTag("ClippingBoxInteractible"))
            {
                currentInteractible.transform.parent.GetComponent<Rigidbody>().useGravity = false;
                currentInteractible.transform.parent.GetComponent<Rigidbody>().isKinematic = true;
            }
            else if (currentInteractible.CompareTag("CubeInteractible"))
            {
                Rigidbody targetBody = currentInteractible.transform.parent.GetComponent<Rigidbody>();
                targetBody.velocity = pose.GetVelocity();
                targetBody.angularVelocity = pose.GetAngularVelocity();
            }

            // Detaches the current interactible object to our motion controller
            joint.connectedBody = null;

            // Clear the memory of variables
            currentInteractible.activeController = null;
            currentInteractible = null;
        }
    }

    private CustomizedInteractible GetNearestInteractible()
    {
        CustomizedInteractible nearest = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach (CustomizedInteractible interactible in contactInteractibles)
        {
            distance = (interactible.transform.position - transform.position).sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = interactible;
            }
        }

        return nearest;
    }
}






// If the player in this computer want to drag an object, he has to ask the owner for ownership and after a positive response, can grab because becomes the owner
// STRUCTURE OF CODE AND IDEAS

//int playerID = photonView.viewID;

//PhotonView draggedCubePV = null;

//if (leftController.GetComponent<MotionControllersEvents>().currentInteractible)
//{
//    draggedCubePV = leftController.GetComponent<MotionControllersEvents>().currentInteractible.gameObject.GetComponent<PhotonView>();

//    if (draggedCubePV.ownerId == playerID)
//        return;

//    draggedCubePV.TransferOwnership(playerID); HAVE TO BE CODED / OVERRIDEN
//    Debug.Log("playerID : " + PhotonNetwork.player + " / cubeOwnerID : " + draggedCubePV.owner + " / MasterClientID : " + PhotonNetwork.masterClient);

//    //photonView.RPC("UpdateMasterClient", PhotonTargets.OthersBuffered, playerID, draggedCubePV);
//}
//else if (rightController.GetComponent<MotionControllersEvents>().currentInteractible)
//{
//    draggedCubePV = rightController.GetComponent<MotionControllersEvents>().currentInteractible.gameObject.GetComponent<PhotonView>();

//    if (draggedCubePV.ownerId == playerID)
//        return;

//    draggedCubePV.TransferOwnership(playerID); HAVE TO BE CODED / OVERRIDEN
//    Debug.Log("playerID : " + PhotonNetwork.player + " / cubeOwnerID : " + draggedCubePV.owner + " / MasterClientID : " + PhotonNetwork.masterClient);

//    //photonView.RPC("UpdateMasterClient", PhotonTargets.OthersBuffered, playerID, draggedCubePV);
//}

//[PunRPC]
//public void UpdateMasterClient(int RPCid, int RPCdraggedCubeId, PhotonMessageInfo info)
//{
//    Debug.Log(rightController.GetComponent<MotionControllersEvents>().currentInteractible.gameObject.GetComponent<PhotonView>());
//    PhotonView.Find(RPCdraggedCubeId).TransferOwnership(RPCid);
//    Debug.Log(RPCid);
//}