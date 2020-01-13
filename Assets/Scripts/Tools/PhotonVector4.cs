using UnityEngine;
using ExitGames.Client.Photon;

public class PhotonVector4 
{
    public byte Id { get; set; }

    public static readonly byte[] memVector4 = new byte[4 * 4];

    private void Start()
    {
        PhotonPeer.RegisterType(typeof(Vector4), (byte)'W', SerializeVector4, DeserializeVector4);
    }

    private static short SerializeVector4(StreamBuffer outStream, object customObject)
    {
        Vector4 vo = (Vector4)customObject;
        lock (memVector4)
        {
            byte[] bytes = memVector4;
            int index = 0;
            Protocol.Serialize(vo.x, bytes, ref index);
            Protocol.Serialize(vo.y, bytes, ref index);
            Protocol.Serialize(vo.z, bytes, ref index);
            Protocol.Serialize(vo.w, bytes, ref index);
            outStream.Write(bytes, 0, 4 * 4);
        }

        return 4 * 4;
    }

    private static object DeserializeVector4(StreamBuffer inStream, short length)
    {
        Vector4 vo = new Vector4();
        lock (memVector4)
        {
            inStream.Read(memVector4, 0, 4 * 4);
            int index = 0;
            Protocol.Deserialize(out vo.x, memVector4, ref index);
            Protocol.Deserialize(out vo.y, memVector4, ref index);
            Protocol.Deserialize(out vo.z, memVector4, ref index);
            Protocol.Deserialize(out vo.w, memVector4, ref index);
        }

        return vo;
    }
}
