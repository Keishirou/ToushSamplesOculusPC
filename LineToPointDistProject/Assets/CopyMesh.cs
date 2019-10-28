using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyMesh : MonoBehaviour {
    public Material material;
    public GameObject PointObj;
    Mesh copyMesh,mesh;

    double[,] rotationMatrix = new double[3, 3]; //回転行列を保存する配列
    Quaternion q;

    int[] indecies; //Meshを結ぶ順番を保存する配列（点で表示するから正直適当でいい）
    Vector3[] vertices; //点の位置を保存する配列

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        mesh = new Mesh(); //Meshの頂点を可視化するためのMesh

        copyMesh = PointObj.GetComponent<MeshFilter>().mesh; //可視化したいMesh
        q = PointObj.transform.rotation; //どうやらMeshは移動はしても回転はしないっぽいので回転を取得

        //回転行列(もっときれいな書き方あるかも)http://www.mss.co.jp/technology/report/pdf/19-08.pdf
        rotationMatrix[0, 0] = q.x * q.x - q.y * q.y - q.z * q.z + q.w * q.w;//Math.Pow(q.x,2) - Math.Pow(q.y, 2) - Math.Pow(q.z, 2) + Math.Pow(q.w, 2);
        rotationMatrix[0, 1] = 2.0f * (q.x * q.y - q.z * q.w);
        rotationMatrix[0, 2] = 2.0f * (q.x * q.z + q.y * q.w);
        rotationMatrix[1, 0] = 2.0f * (q.x * q.y + q.z * q.w);
        rotationMatrix[1, 1] = -q.x * q.x + q.y * q.y - q.z * q.z + q.w * q.w;
        rotationMatrix[1, 2] = 2.0f * (q.y * q.z - q.x * q.w);
        rotationMatrix[2, 0] = 2.0f * (q.x * q.z - q.y * q.w);
        rotationMatrix[2, 1] = 2.0f * (q.y * q.z + q.x * q.w);
        rotationMatrix[2, 2] = -q.x * q.x - q.y * q.y + q.z * q.z + q.w * q.w;

        //http://marupeke296.sakura.ne.jp/DXG_No58_RotQuaternionTrans.html
        //rotationMatrix[0, 0] = 1.0f - 2.0f * q.y * q.y - 2.0f * q.z * q.z;
        //rotationMatrix[0, 1] = 2.0f * (q.x * q.y + q.z * q.w);
        //rotationMatrix[0, 2] = 2.0f * (q.x * q.z - q.y * q.w);
        //rotationMatrix[1, 0] = 2.0f * (q.x * q.y - q.z * q.w);
        //rotationMatrix[1, 1] = 1.0f - 2.0f * q.x * q.x - 2.0f * q.z * q.z;
        //rotationMatrix[1, 2] = 2.0 * (q.y * q.z + q.x * q.w);
        //rotationMatrix[2, 0] = 2.0 * (q.x * q.z + q.y * q.w);
        //rotationMatrix[2, 1] = 2.0 * (q.y * q.z - q.x * q.w);
        //rotationMatrix[2, 2] = 1.0f - 2.0f * q.x * q.x - 2.0f * q.y * q.y;

        //mesh.vertices = copyMesh.vertices;
        int[] indecies = new int[copyMesh.vertices.Length]; //Meshを結ぶ順番を保存する配列（点で表示するから正直適当でいい）
        Vector3[] vertices = new Vector3[copyMesh.vertices.Length]; //点の位置を保存する配列

        for (int i = 0;i < copyMesh.vertices.Length; i++)
        {
            //Mesh頂点を可視化したい仮想物体の位置情報＋回転
            vertices[i] = PointObj.transform.position;// + copyMesh.vertices[i];// + PointObj.transform.position;
            vertices[i].x += copyMesh.vertices[i].x * -(float)rotationMatrix[0, 0] + copyMesh.vertices[i].y * -(float)rotationMatrix[0, 1] + copyMesh.vertices[i].z * -(float)rotationMatrix[0, 2];
            vertices[i].y += copyMesh.vertices[i].x * -(float)rotationMatrix[1, 0] + copyMesh.vertices[i].y * -(float)rotationMatrix[1, 1] + copyMesh.vertices[i].z * -(float)rotationMatrix[1, 2];
            vertices[i].z += copyMesh.vertices[i].x * -(float)rotationMatrix[2, 0] + copyMesh.vertices[i].y * -(float)rotationMatrix[2, 1] + copyMesh.vertices[i].z * -(float)rotationMatrix[2, 2];

            //適当な値代入
            indecies[i] = i;
        }

        mesh.vertices = vertices; //点の位置を代入

        #region Meshの表示に必要な処理

        mesh.RecalculateNormals();

        mesh.SetIndices(indecies, MeshTopology.Points, 0);

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (!meshRenderer) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.sharedMaterial = material;

        #endregion
    }
}
