using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class CustomizedInteractible : Photon.MonoBehaviour
{
    // Attribute that indicates the controller that currently grabs the scene object with this script attached to
    [HideInInspector]
    public MotionControllersEvents activeController = null;

    public Material basicMaterial;
    public Material enlightenedMaterial;

    private Vector3 newPos;
    private Quaternion newRot;

    private Bounds sphereBounds;
    private Bounds planeBounds;
    //private Vector3[] sphereVertices;

    private void Start()
    {
        // Defines how many times per second PhotonNetwork should send a package (default is 20)
        PhotonNetwork.sendRate = 50;

        // Defines how many times per second OnPhotonSerializeView should be called on PhotonViews (default is 10)
        PhotonNetwork.sendRateOnSerialize = 50;
    }

    private void Update()
    {
        //sphereBounds = GetComponentInParent<MeshFilter>().mesh.bounds;
        //planeBounds = GetComponent<MeshFilter>().mesh.bounds;

        //if (sphereBounds.Intersects(planeBounds))
        //    Debug.Log("Intersection !");

        //sphereVertices = GetComponentInParent<MeshFilter>().mesh.vertices;

        //if (!photonView.isMine)
        //{
        //    // Minimizes the difference of position of interactible objects between the 2 players
        //    float dist = Vector3.Distance(transform.position, newPos);

        //    if (dist < 2f)
        //    {
        //        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 5);
        //        transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * 5);
        //    }
        //    else
        //    {
        //        transform.position = newPos;
        //        transform.rotation = newRot;
        //    }
        //}
    }

    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //Debug.Log("CustomizedInteractible - OnPhotonSerializeView");

        if (stream.isWriting)
        {
            stream.SendNext(transform.parent.position);
            stream.SendNext(transform.parent.rotation);

            stream.SendNext(transform.parent.GetComponent<Rigidbody>().position);
            stream.SendNext(transform.parent.GetComponent<Rigidbody>().rotation);
            stream.SendNext(transform.parent.GetComponent<Rigidbody>().velocity);
            stream.SendNext(transform.parent.GetComponent<Rigidbody>().angularVelocity);
        }
        else
        {
            newPos = (Vector3)stream.ReceiveNext();
            newRot = (Quaternion)stream.ReceiveNext();

            Vector3 newPos2 = (Vector3)stream.ReceiveNext();
            Quaternion newRot2 = (Quaternion)stream.ReceiveNext();
            Vector3 newVel = (Vector3)stream.ReceiveNext();
            Vector3 newAngVel = (Vector3)stream.ReceiveNext();

            if (!photonView.isMine)
            {
                //Debug.Log("CubePersoSerialize - isNotMine part running");

                transform.parent.position = newPos;
                transform.parent.rotation = newRot;

                transform.parent.GetComponent<Rigidbody>().position = newPos2;
                transform.parent.GetComponent<Rigidbody>().rotation = newRot2;
                transform.parent.GetComponent<Rigidbody>().velocity = newVel;
                transform.parent.GetComponent<Rigidbody>().angularVelocity = newAngVel;
            }
        }
    }
}
