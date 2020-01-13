using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCenterCheck : MonoBehaviour {
    public GameObject rHand;
    Matrix4x4 thisMatrix;
    GameObject point;
    GameObject device;

    // Use this for initialization
    void Start () {
        thisMatrix = transform.localToWorldMatrix;

        // yama 171221 オブジェクトの先端に空オブジェクトを生成
        point = new GameObject();
        Vector3 vec = thisMatrix.MultiplyPoint3x4(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
        point.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
        //Debug.Log("mesh1 vertex at " + thisMatrix.MultiplyPoint3x4(CreateVer));
        point.name = this.name + "_Center";

        // yama 171221 作成した空オブジェクトをデバイスオブジェクトの子オブジェクトに変更
        device = GameObject.Find("Device");
        //point.transform.parent = device.transform;
        // yama 181031 デバイスの子オブジェクトだとOculusTouchとの位置関係をもとに座標を求めることが難しかったため，オブジェクトの作成場所を変更
        //point.transform.parent = (device.transform.root.gameObject).transform;
        point.transform.parent = device.transform;

        // yama 171221 空オブジェクト以外を作成する際に使用，オブジェクトの大きさなどを指定
        Transform pointTrans = point.transform;
        pointTrans.localPosition = vec;
        //Transform cubeTrans = point.transform;
        //cubeTrans.localScale = Vector3.one * 0.03f;

        
    }

    // Update is called once per frame
    //private void FixedUpdate()
    //{
        //point.transform.position = (device.transform.root.gameObject).transform.Find("OVRCameraRig/TrackingSpace").transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
        //Vector3 pos = point.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
        //point.transform.position =  new Vector3(pos.x / 2, pos.y / 2, pos.z / 2);
        //point.transform.position = pos;
        //point.transform.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        //point.transform.position = pos;
    //}
}
