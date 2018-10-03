using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_ObjEnd : MonoBehaviour {

    string device_n = "Device"; // yama 180122 デバイスのオブジェクト名
    string function = "GetEnd";    // yama 180122 オブジェクトの先端座標を渡す変数名

    // Use this for initialization
    void Start () {
        Matrix4x4 thisMatrix = transform.localToWorldMatrix;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        Vector3 CreateVer = Vector3.zero;

        // yama 171221 オブジェクトの頂点探索
        foreach (Vector3 vertex in vertices)
        {
            if(vertex.y > CreateVer.y)
            {
                // yama 180214 ローカル座標においてY軸上で最も上に位置する点の検出
                CreateVer = vertex;
            }
        }

        // yama 171221 オブジェクトの先端に空オブジェクトを生成
        GameObject point = new GameObject();
        Vector3 vec = thisMatrix.MultiplyPoint3x4(CreateVer);
        //Debug.Log("mesh1 vertex at " + thisMatrix.MultiplyPoint3x4(CreateVer));
        point.name = this.name + "_End";

        // yama 171221 空オブジェクト以外を作成する際に使用，オブジェクトの大きさなどを指定
        Transform pointTrans = point.transform;
        pointTrans.localPosition = vec;
        //Transform cubeTrans = point.transform;
        //cubeTrans.localScale = Vector3.one * 0.03f;

        // yama 171221 作成した空オブジェクトをデバイスオブジェクトの子オブジェクトに変更
        GameObject device = GameObject.Find(device_n);
        point.transform.parent = device.transform;

        // yama 180214 スクリプトを取り付けたオブジェクトの後端座標を渡す
        device.SendMessage(function, point);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
