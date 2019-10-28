using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lineToPoint : MonoBehaviour {
    public GameObject End;          //デバイスの末端についているSphere_End
    public GameObject Device_Tip;
    public GameObject Point;
    public static Ray inputRay;
    Mesh mesh;// = GetComponent<MeshFilter>().mesh;
    Vector3[] vertices;// = mesh.vertices;
    Quaternion tempRotation;
    float min_Len;
    int min_Num;
    
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        inputRay = new Ray(Device_Tip.transform.position, End.transform.position);
        mesh = Point.GetComponent<MeshFilter>().mesh;
        //mesh = Point.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        tempRotation = Point.transform.rotation;
        Debug.Log(tempRotation);

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += Point.transform.position;
        }

        min_Len = DistanceToLine(inputRay, vertices[0]);
        min_Num = 0;

        for (int i=0; i < vertices.Length; i++)
        {
            if(min_Len > DistanceToLine(inputRay, vertices[i]))
            {
                min_Len = DistanceToLine(inputRay, vertices[i]);
                min_Num = i;
                //Debug.Log(vertices[i]);
            }
        }

        Debug.DrawLine(Device_Tip.transform.position, End.transform.position);  //Rayを可視化
        //Debug.Log(DistanceToLine(inputRay, Point.transform.position));
        //Debug.DrawLine(Point.transform.position, PerpendicularFootPoint(Device_Tip.transform.position, End.transform.position, Point.transform.position));  //Rayを可視化
        Debug.DrawLine(vertices[min_Num], PerpendicularFootPoint(Device_Tip.transform.position, End.transform.position, vertices[min_Num]));  //Rayを可視化
    }

    public static float DistanceToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    // 点Pから直線ABに下ろした垂線の足の座標を返す
    Vector3 PerpendicularFootPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        return a + Vector3.Project(p - a, b - a);
    }
}
