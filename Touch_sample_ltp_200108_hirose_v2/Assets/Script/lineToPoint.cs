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
    Vector3 scale_Point;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        inputRay = new Ray(Device_Tip.transform.position, End.transform.position);

        //PointにMeshがある時のみ処理を行う
        if (Point.GetComponent<MeshFilter>())
        {
            mesh = Point.GetComponent<MeshFilter>().mesh; //最近傍を計算したいMesh取得
            //mesh = Point.GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices; //Meshの頂点情報取得
            scale_Point = Point.transform.localScale;

            //Debug.Log(scale_Point);

            tempRotation = Point.transform.rotation;
            //Debug.Log(tempRotation);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].x *= scale_Point.x;
                vertices[i].y *= scale_Point.y;
                vertices[i].z *= scale_Point.z;
                vertices[i] += Point.transform.position; //ローカル座標を変換
            }

            //初期化
            min_Len = DistanceToLine(inputRay, vertices[0]);
            min_Num = 0;

            //Meshとベクトルの最近傍計算
            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    if (min_Len > DistanceToLine(inputRay, vertices[i]))
            //    {
            //        min_Len = DistanceToLine(inputRay, vertices[i]);
            //        min_Num = i;
            //        //Debug.Log(vertices[i]);
            //    }
            //}

            //Tipとベクトルの最近傍計算
            for (int i = 0; i < vertices.Length; i++)
            {
                if (min_Len > Vector3.Distance(vertices[i], Device_Tip.transform.position))
                {
                    min_Len = Vector3.Distance(vertices[i], Device_Tip.transform.position);
                    min_Num = i;
                    //Debug.Log(vertices[i]);
                }
            }

            Debug.DrawLine(Device_Tip.transform.position, End.transform.position);  //Rayを可視化
                                                                                    //Debug.Log(DistanceToLine(inputRay, Point.transform.position));
                                                                                    //Debug.DrawLine(Point.transform.position, PerpendicularFootPoint(Device_Tip.transform.position, End.transform.position, Point.transform.position));  //Rayを可視化
            Debug.DrawLine(vertices[min_Num], PerpendicularFootPoint(Device_Tip.transform.position, End.transform.position, vertices[min_Num]), Color.red);  //Rayを可視化
            //Debug.DrawLine(MeshDeformer.dispv, PerpendicularFootPoint(Device_Tip.transform.position, End.transform.position, MeshDeformer.dispv), Color.red);  //Rayを可視化
        }
        
    }

    public static float DistanceToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    // 点Pから直線ABに下ろした垂線の足の座標を返す
    public Vector3 PerpendicularFootPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        return a + Vector3.Project(p - a, b - a);
    }
}
