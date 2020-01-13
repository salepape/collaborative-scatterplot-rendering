using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

// We take 1 base station as a reference point
public class BaseStationsCoor : Photon.MonoBehaviour
{
    public GameObject baseStation1;
    public GameObject baseStation2;

    private IEnumerable<XRNodeState> baseStationsnodes;

    public List<Vector3> baseStationsPos;
    public List<Vector3> baseStationsRot;

    public List<string> stringIndexBaseStations;



    // Start is called before the first frame update
    private void Start()
    {
        GetBaseStationsNodes();
        GetCoorBaseStations();
        GetIndexBaseStations();

        // Provide the coordinate of base stations to the game objects in the scene (game objects attached to the corresponding attributes)
        baseStation1.transform.position = baseStationsPos[0];
        baseStation1.transform.rotation = Quaternion.Euler(baseStationsRot[0]);

        baseStation2.transform.position = baseStationsPos[1];
        baseStation2.transform.rotation = Quaternion.Euler(baseStationsRot[1]);
    }

    private void GetBaseStationsNodes()
    {
        var nodeStates = new List<XRNodeState>();
        InputTracking.GetNodeStates(nodeStates);
        baseStationsnodes = nodeStates.Where(n => n.nodeType == XRNode.TrackingReference);
    }

    // Retrieves base stations coordinates in the headset repair (of the virutal world)
    private void GetCoorBaseStations()
    {
        foreach (var basestation in baseStationsnodes)
        {
            bool hasPos = basestation.TryGetPosition(out Vector3 position);
            baseStationsPos.Add(position);

            bool hasRot = basestation.TryGetRotation(out Quaternion rotation);
            baseStationsRot.Add(rotation.eulerAngles);
        }
    }

    // Computes unique device serial
    private void GetIndexBaseStations()
    {
        var error = ETrackedPropertyError.TrackedProp_Success;
        int nbLighthouses = 0;
        uint i = 0;

        while (nbLighthouses != 2)
        {
            var trackingSystemName = new StringBuilder();
            var serialNumber = new StringBuilder();

            // Obtain the tracking system name
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_TrackingSystemName_String, trackingSystemName, 64, ref error);

            if (trackingSystemName.ToString().Contains("lighthouse"))
            { 
                // Obtain the serial number
                OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String, serialNumber, 64, ref error);

                var serialNumberString = serialNumber.ToString();
                stringIndexBaseStations.Add(serialNumberString);

                ++nbLighthouses;
            }

            ++i;
        }

        if (PhotonNetwork.isMasterClient)
            photonView.RPC("BaseStationSerialNumberMaster", PhotonTargets.OthersBuffered, stringIndexBaseStations[0], stringIndexBaseStations[1]);
        else
        {
            // Could send info from client to master to have correct base station serial numbers but not useful because master doesn't have to do world mapping
        }
    }

    [PunRPC]
    public void BaseStationSerialNumberMaster(string index0, string index1, PhotonMessageInfo info)
    {
        Debug.Log("BaseStationCoor - Pass in PunRPC function ! The sender is : " + info.sender);

        var master = GameObject.FindGameObjectWithTag("Master");
        var listString = master.GetComponent<BaseStationsCoor>().stringIndexBaseStations;
        listString[0] = index0;
        listString[1] = index1;
    }
}
