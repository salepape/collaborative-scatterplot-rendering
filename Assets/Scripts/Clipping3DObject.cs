using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;

/// <summary>
/// Component to animate and visualize a plane that can be used with per pixel based clipping.
/// </summary>
//[ExecuteInEditMode]
public class Clipping3DObject : MonoBehaviour
{
    private int clippingSideID;
    private int planeCenterID;
    private int planeNormalID;
    private int planeXBoundsID;
    private int planeYBoundsID;
    private int planeZBoundsID;
    private int clipBoxSizeID;
    private int clipBoxInverseTransformID;

    private int clippingSideID2;
    private int planeCenterID2;
    private int planeNormalID2;
    private int planeXBoundsID2;
    private int planeYBoundsID2;
    private int planeZBoundsID2;
    private int clipBoxSizeID2;
    private int clipBoxInverseTransformID2;

    private int clippingSideID3;
    private int clipBoxSizeID3;
    private int clipBoxInverseTransformID3;

    private Vector2 boundsX, boundsY, boundsZ;
    public float ClippingThicknessToAdd;
    public int valueIndexToDisplay;

    protected List<Material> allocatedMaterials = new List<Material>();

    [Tooltip("The renderer(s) that should be affected by the primitive.")]
    [SerializeField]
    protected List<Renderer> renderers = new List<Renderer>();

    public enum Side
    {
        Inside = 1,
        Outside = -1
    }

    [Tooltip("Which side of the primitive to clip pixels against.")]
    [SerializeField]
    protected Side clippingSide = Side.Inside;

    protected MaterialPropertyBlock materialPropertyBlock;



    public void AddRenderer(Renderer _renderer)
    {
        if (_renderer != null && !renderers.Contains(_renderer))
            renderers.Add(_renderer);
    }

    public void RemoveRenderer(Renderer _renderer)
    {
        if (renderers.Contains(_renderer))
            renderers.Remove(_renderer);
    }

    // We need this class to be updated once per frame even when in edit mode. Ideally this would occur after all other objects are updated in LateUpdate(), but because the ExecuteInEditMode attribute only invokes Update() we handle edit mode updating in Update() and runtime updating n LateUpdate()
    protected void Update()
    {
        Initialize();
        UpdateRenderers();

        GameObject[] legends = GameObject.FindGameObjectsWithTag("dataDisplay");
        foreach (GameObject legend in legends)
        {
            legend.GetComponent<RectTransform>().localRotation = Quaternion.LookRotation(legend.GetComponent<RectTransform>().localPosition - GameObject.FindGameObjectWithTag("MainCamera").transform.position);

            if (legend.GetComponent<Timer>().timer > 0.0f)
                Destroy(legend);
        }
    }

    protected void Initialize()
    {
        materialPropertyBlock = new MaterialPropertyBlock();

        if (gameObject.CompareTag("ClippingPlaneInteractible"))
        {
            planeCenterID = Shader.PropertyToID("_ClipPlaneCenter");
            planeNormalID = Shader.PropertyToID("_ClipPlaneNormal");

            planeXBoundsID = Shader.PropertyToID("_PlaneXBounds");
            planeYBoundsID = Shader.PropertyToID("_PlaneYBounds");
            planeZBoundsID = Shader.PropertyToID("_PlaneZBounds");

            clippingSideID = Shader.PropertyToID("_ClipBoxLinkedPlaneSide");
            clipBoxSizeID = Shader.PropertyToID("_ClipBoxLinkedPlaneSize");
            clipBoxInverseTransformID = Shader.PropertyToID("_ClipBoxLinkedPlaneInverseTransform");
        }
        else if (gameObject.CompareTag("ClippingPlaneInteractible2"))
        {
            planeCenterID2 = Shader.PropertyToID("_ClipPlaneCenter2");
            planeNormalID2 = Shader.PropertyToID("_ClipPlaneNormal2");

            planeXBoundsID2 = Shader.PropertyToID("_PlaneXBounds2");
            planeYBoundsID2 = Shader.PropertyToID("_PlaneYBounds2");
            planeZBoundsID2 = Shader.PropertyToID("_PlaneZBounds2");

            clippingSideID2 = Shader.PropertyToID("_ClipBoxLinkedPlaneSide2");
            clipBoxSizeID2 = Shader.PropertyToID("_ClipBoxLinkedPlaneSize2");
            clipBoxInverseTransformID2 = Shader.PropertyToID("_ClipBoxLinkedPlaneInverseTransform2");
        }
        else if (gameObject.CompareTag("ClippingBoxInteractible"))
        {
            clippingSideID3 = Shader.PropertyToID("_ClipBoxSide");
            clipBoxSizeID3 = Shader.PropertyToID("_ClipBoxSize");
            clipBoxInverseTransformID3 = Shader.PropertyToID("_ClipBoxInverseTransform");
        }
    }

    protected void UpdateRenderers()
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Count; ++i)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(materialPropertyBlock);

            if (gameObject.CompareTag("ClippingPlaneInteractible"))
                materialPropertyBlock.SetFloat(clippingSideID, (float)clippingSide);
            else if (gameObject.CompareTag("ClippingPlaneInteractible2"))
                materialPropertyBlock.SetFloat(clippingSideID2, (float)clippingSide);
            else if (gameObject.CompareTag("ClippingBoxInteractible"))
                materialPropertyBlock.SetFloat(clippingSideID3, (float)clippingSide);
           
            UpdateShaderProperties(materialPropertyBlock);

            renderer.SetPropertyBlock(materialPropertyBlock);

            //Debug.Log("boundsX : " + boundsX.ToString("F3") + "; boundsY : " + boundsY.ToString("F3") + "; boundsZ : " + boundsZ.ToString("F3"));

            Vector3 pointPos = renderer.gameObject.transform.position;
            if (isPointInPlane(pointPos))
            {
                string valueToDisplay = Convert.ToString(renderer.gameObject.transform.parent.GetComponent<DataScatterplot>().DisplayLegend(valueIndexToDisplay, pointPos));

                GameObject display = new GameObject();
                display.tag = "dataDisplay";

                TextMeshPro tmp = display.AddComponent<TextMeshPro>();
                tmp.text = valueToDisplay;
                tmp.fontSize = 0.1f;
                tmp.autoSizeTextContainer = true;

                display.transform.SetParent(renderer.gameObject.transform);
                display.GetComponent<RectTransform>().localPosition = renderer.gameObject.transform.position;

                display.AddComponent<Timer>();
            }
        }
    }

    protected void UpdateShaderProperties(MaterialPropertyBlock materialPropertyBlock)
    {
        if (gameObject.CompareTag("ClippingPlaneInteractible") || gameObject.CompareTag("ClippingPlaneInteractible2"))
        {
            Vector3 planeCenter = transform.parent.position;
            Vector3 planeNormal = transform.parent.right;

            // Computes bounding box including rotation by computing the 8 extreme points
            Quaternion quat = Quaternion.identity;
            quat.SetLookRotation(transform.parent.forward, Vector3.Cross(transform.parent.forward, transform.parent.right));

            Vector3 planeExtents = transform.parent.parent.parent.localScale;
            float width = transform.parent.localScale.x;
            float height = planeExtents.y;
            float depth = planeExtents.z;
            //Matrix4x4 m = Matrix4x4.Rotate(Quaternion.Euler(transform.parent.rotation.x, transform.parent.rotation.y, transform.parent.rotation.z));

            Vector3[] corners = new Vector3[8];
            corners[0] = quat * new Vector3(-width / 2, +height / 2, -depth / 2) + planeCenter; // Front top left corner
            corners[1] = quat * new Vector3(+width / 2, +height / 2, -depth / 2) + planeCenter; // Front top right corner
            corners[2] = quat * new Vector3(-width / 2, -height / 2, -depth / 2) + planeCenter; // Front bottom left corner
            corners[3] = quat * new Vector3(+width / 2, -height / 2, -depth / 2) + planeCenter; // Front bottom right corner
            corners[4] = quat * new Vector3(-width / 2, +height / 2, +depth / 2) + planeCenter; // Back top left corner
            corners[5] = quat * new Vector3(+width / 2, +height / 2, +depth / 2) + planeCenter; // Back top right corner
            corners[6] = quat * new Vector3(-width / 2, -height / 2, +depth / 2) + planeCenter; // Back bottom left corner
            corners[7] = quat * new Vector3(+width / 2, -height / 2, +depth / 2) + planeCenter; // Back bottom right corner

            // AABB strategy 
            float minX = corners[0].x;
            float minY = corners[0].y;
            float minZ = corners[0].z;
            float maxX = corners[0].x;
            float maxY = corners[0].y;
            float maxZ = corners[0].z;

            foreach (Vector3 v in corners)
            {
                minX = Math.Min(v.x, minX);
                minY = Math.Min(v.y, minY);
                minZ = Math.Min(v.z, minZ);
                maxX = Math.Max(v.x, maxX);
                maxY = Math.Max(v.y, maxY);
                maxZ = Math.Max(v.z, maxZ);
            }

            boundsX = new Vector2(minX, maxX);
            boundsY = new Vector2(minY, maxY);
            boundsZ = new Vector2(minZ, maxZ);

            // We store values that correspond to world space (repair in top right of the screen, not the one stuck on the plane)
            //Debug.Log("boundsX : " + boundsX.ToString("F3") + "; boundsY : " + boundsY.ToString("F3") + "; boundsZ : " + boundsZ.ToString("F3"));

            Vector3 lossyScale = transform.parent.parent.parent.lossyScale * 0.5f;
            Vector4 boxSize = new Vector4(lossyScale.x, lossyScale.y, lossyScale.z, 0.0f);
            Matrix4x4 boxInverseTransform = Matrix4x4.TRS(transform.parent.parent.parent.position, transform.parent.parent.parent.rotation, Vector3.one).inverse;

            if (gameObject.CompareTag("ClippingPlaneInteractible"))
            {
                materialPropertyBlock.SetVector(planeCenterID, planeCenter);
                materialPropertyBlock.SetVector(planeNormalID, planeNormal);

                materialPropertyBlock.SetVector(planeXBoundsID, boundsX);
                materialPropertyBlock.SetVector(planeYBoundsID, boundsY);
                materialPropertyBlock.SetVector(planeZBoundsID, boundsZ);

                materialPropertyBlock.SetVector(clipBoxSizeID, boxSize);
                materialPropertyBlock.SetMatrix(clipBoxInverseTransformID, boxInverseTransform);
            }
            else if (gameObject.CompareTag("ClippingPlaneInteractible2"))
            {
                materialPropertyBlock.SetVector(planeCenterID2, planeCenter);
                materialPropertyBlock.SetVector(planeNormalID2, planeNormal);

                materialPropertyBlock.SetVector(planeXBoundsID2, boundsX);
                materialPropertyBlock.SetVector(planeYBoundsID2, boundsY);
                materialPropertyBlock.SetVector(planeZBoundsID2, boundsZ);

                materialPropertyBlock.SetVector(clipBoxSizeID2, boxSize);
                materialPropertyBlock.SetMatrix(clipBoxInverseTransformID2, boxInverseTransform);
            }
        }
        else if (gameObject.CompareTag("ClippingBoxInteractible"))
        {
            Vector3 lossyScale = transform.parent.lossyScale * 0.5f;
            Vector4 boxSize = new Vector4(lossyScale.x, lossyScale.y, lossyScale.z, 0.0f);
            Matrix4x4 boxInverseTransform = Matrix4x4.TRS(transform.parent.position, transform.parent.rotation, Vector3.one).inverse;

            materialPropertyBlock.SetVector(clipBoxSizeID3, boxSize);
            materialPropertyBlock.SetMatrix(clipBoxInverseTransformID3, boxInverseTransform);
        }
    }

    bool isPointInPlane(Vector3 pointPos)
    {
        return ((pointPos.x > Math.Min(boundsX[0], boundsX[1]) - ClippingThicknessToAdd && pointPos.x < Math.Max(boundsX[0], boundsX[1]) + ClippingThicknessToAdd) &&
            (pointPos.y > Math.Min(boundsY[0], boundsY[1]) && pointPos.y < Math.Max(boundsY[0], boundsY[1])) &&
            (pointPos.z > Math.Min(boundsZ[0], boundsZ[1]) && pointPos.z < Math.Max(boundsZ[0], boundsZ[1])));
    }




void OnDrawGizmos()
    {
        ////////////////////////////// 
        Gizmos.color = Color.green;
        Vector3 planeCenter = transform.parent.position;
        Vector3 planeNormal = transform.parent.right;
        Gizmos.DrawLine(planeCenter, planeCenter + 0.5f * planeNormal);

        //////////////////////////////
        Gizmos.color = Color.black;
        Quaternion quat = Quaternion.identity;
        quat.SetLookRotation(transform.parent.forward, Vector3.Cross(transform.parent.forward, transform.parent.right));

        Vector3 planeExtents;
        if (gameObject.CompareTag("ClippingPlaneInteractible") || gameObject.CompareTag("ClippingPlaneInteractible2"))
            planeExtents = transform.parent.parent.parent.localScale;
        else
            planeExtents = transform.parent.localScale;

        float width = transform.parent.localScale.x;
        float height = planeExtents.y;
        float depth = planeExtents.z;

        Vector3[] corners = new Vector3[8];
        corners[0] = quat * new Vector3(-width / 2, +height / 2, -depth / 2) + planeCenter; // Front top left corner
        corners[1] = quat * new Vector3(+width / 2, +height / 2, -depth / 2) + planeCenter; // Front top right corner
        corners[2] = quat * new Vector3(-width / 2, -height / 2, -depth / 2) + planeCenter; // Front bottom left corner
        corners[3] = quat * new Vector3(+width / 2, -height / 2, -depth / 2) + planeCenter; // Front bottom right corner
        corners[4] = quat * new Vector3(-width / 2, +height / 2, +depth / 2) + planeCenter; // Back top left corner
        corners[5] = quat * new Vector3(+width / 2, +height / 2, +depth / 2) + planeCenter; // Back top right corner
        corners[6] = quat * new Vector3(-width / 2, -height / 2, +depth / 2) + planeCenter; // Back bottom left corner
        corners[7] = quat * new Vector3(+width / 2, -height / 2, +depth / 2) + planeCenter; // Back bottom right corner

        Gizmos.DrawLine(corners[0], corners[2]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[6], corners[4]);
        Gizmos.DrawLine(corners[4], corners[0]);

        Gizmos.DrawLine(corners[1], corners[3]);
        Gizmos.DrawLine(corners[3], corners[7]);
        Gizmos.DrawLine(corners[7], corners[5]);
        Gizmos.DrawLine(corners[5], corners[1]);

        if (gameObject.CompareTag("ClippingPlaneInteractible") || gameObject.CompareTag("ClippingPlaneInteractible2"))
        {
            Gizmos.DrawRay(corners[0], -planeNormal);
            Gizmos.DrawRay(corners[2], -planeNormal);
            Gizmos.DrawRay(corners[6], -planeNormal);
            Gizmos.DrawRay(corners[4], -planeNormal);
        }
    }
}















//public void AddRenderer(Renderer _renderer)
//{
//    if (_renderer != null && !renderers.Contains(_renderer))
//    {
//        renderers.Add(_renderer);

        //Material material = GetMaterial(_renderer, false);

        //if (material != null)
        //{
        //    ToggleClippingFeature(material, true);
        //}
//    }
//}

//public void RemoveRenderer(Renderer _renderer)
//{
//    if (renderers.Contains(_renderer))
//    {
        //Material material = GetMaterial(_renderer, false);

        //if (material != null)
        //{
        //    ToggleClippingFeature(material, false);
        //}

//        renderers.Remove(_renderer);
//    }
//}

//protected void ToggleClippingFeature(Material material, bool keywordOn)
//{
//if (keywordOn)
//{
//    material.EnableKeyword(Keyword);
//    material.SetFloat(KeywordProperty, 1.0f);
//}
//else
//{
//    material.DisableKeyword(Keyword);
//    material.SetFloat(KeywordProperty, 0.0f);
//}
//}

//protected Material GetMaterial(Renderer _renderer, bool trackAllocations = true)
//{
//    if (_renderer == null)
//    {
//        return null;
//    }

//    if (Application.isEditor && !Application.isPlaying)
//    {
//        return _renderer.sharedMaterial;
//    }
//    else
//    {
//        Material material = _renderer.material;

//        if (trackAllocations && !allocatedMaterials.Contains(material))
//        {
//            allocatedMaterials.Add(material);
//        }

//        return material;
//    }
//}





