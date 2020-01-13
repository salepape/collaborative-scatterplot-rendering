using System.Collections.Generic;
using UnityEngine;
using System;

//[ExecuteInEditMode]
public class DataScatterplot : MonoBehaviour
{
    public static DataScatterplot Instance;

    // Name of the input file, no extension
    public string inputfile;

    // List for holding data from CSV reader
    private List<Dictionary<string, object>> pointList;
    private List<string> columnList;

    public Material pointCloudMaterial;

    public float plotScale = 1;

    // The prefab for the data points that will be instantiated
    public GameObject pointPrefab;

    // Full column names
    private string xName;
    private string yName;
    private string zName;

    // Get maximum of each axis
    private float x, y, z;

    private List<Vector3> coorPoints;



    // Use this for initialization
    void Start()
    {
        // Set pointlist to results of function Reader with argument inputfile
        pointList = CSVReader.Read(inputfile);

        // Declare list of strings, fill with keys (column names)
        columnList = new List<string>(pointList[1].Keys);
        coorPoints = new List<Vector3>();

        //// Print number of keys 
        //Debug.Log("There are " + columnList.Count + " columns in the CSV file.");

        //foreach (string key in columnList)
        //    Debug.Log("Column name is " + key);

        // Assign column name from columnList to Name variables
        xName = columnList[0];
        yName = columnList[1];
        zName = columnList[2];

        // Get maximum of each axis
        float xMax = FindMaxValue(xName);
        float yMax = FindMaxValue(yName);
        float zMax = FindMaxValue(zName);

        // Get minimum of each axis
        float xMin = FindMinValue(xName);
        float yMin = FindMinValue(yName);
        float zMin = FindMinValue(zName);

        //Loop through Pointlist
        for (var i = 0; i < pointList.Count; i++)
        {
            // Get value in poinList at i-th "row", in "column" Name, normalize (Convert.ToSingle converts a specified value to a single-precision floating-point number)
            x = (Convert.ToSingle(pointList[i][xName]) - xMin) / (xMax - xMin);
            y = (Convert.ToSingle(pointList[i][yName]) - yMin) / (yMax - yMin);
            z = (Convert.ToSingle(pointList[i][zName]) - zMin) / (zMax - zMin);
            Vector3 coorPoint = new Vector3(x, y, z) * plotScale;
            coorPoints.Add(coorPoint);

            // Instantiate as gameobject variable so that it can be manipulated within loop
            GameObject dataPoint = Instantiate(pointPrefab, coorPoint, Quaternion.identity);

            // Make child of PointHolder object, to keep points within container in hiearchy
            dataPoint.transform.parent = gameObject.transform;

            // Assigns original values to dataPointName
            string dataPointName = pointList[i][xName] + " " + pointList[i][yName] + " " + pointList[i][zName];

            // Assigns name to the prefab
            dataPoint.transform.name = dataPointName;

            // Gets material color 
            dataPoint.GetComponent<Renderer>().material = pointCloudMaterial;

            if(GameObject.FindGameObjectsWithTag("ClippingPlaneInteractible") != null)
                foreach(var clipObject in GameObject.FindGameObjectsWithTag("ClippingPlaneInteractible"))
                    clipObject.GetComponent<Clipping3DObject>().AddRenderer(dataPoint.GetComponent<Renderer>());

            if (GameObject.FindGameObjectsWithTag("ClippingPlaneInteractible2") != null)
                foreach (var clipObject in GameObject.FindGameObjectsWithTag("ClippingPlaneInteractible2"))
                    clipObject.GetComponent<Clipping3DObject>().AddRenderer(dataPoint.GetComponent<Renderer>());

            if (GameObject.FindGameObjectsWithTag("ClippingBoxInteractible") != null)
                foreach (var clipObject in GameObject.FindGameObjectsWithTag("ClippingBoxInteractible"))
                    clipObject.GetComponent<Clipping3DObject>().AddRenderer(dataPoint.GetComponent<Renderer>());
        }      
    }

    private float FindMaxValue(string columnName)
    {
        // Set initial value to first value
        float maxValue = Convert.ToSingle(pointList[0][columnName]);

        //Loop through Dictionary, overwrite existing maxValue if new value is larger
        for (var i = 0; i < pointList.Count; i++)
        {
            if (maxValue < Convert.ToSingle(pointList[i][columnName]))
                maxValue = Convert.ToSingle(pointList[i][columnName]);
        }

        // Spit out the max value
        return maxValue;
    }

    private float FindMinValue(string columnName)
    {
        float minValue = Convert.ToSingle(pointList[0][columnName]);

        //Loop through Dictionary, overwrite existing minValue if new value is smaller
        for (var i = 0; i < pointList.Count; i++)
        {
            if (Convert.ToSingle(pointList[i][columnName]) < minValue)
                minValue = Convert.ToSingle(pointList[i][columnName]);
        }

        return minValue;
    }

    public object DisplayLegend(int index, Vector3 pointPos)
    {
        var i = 0;
        for (; i < pointList.Count; i++)
        {
            if (pointPos.x == coorPoints[i].x && pointPos.y == coorPoints[i].y && pointPos.z == coorPoints[i].z)
                break;
        }

        return pointList[i][columnList[index]];
    }
}
