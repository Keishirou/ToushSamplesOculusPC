using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshDeformerInput_Sphere : MonoBehaviour
{
    public float force = 5f;        //力    元：10f
    public float forceOffset = 0.1f; //力の相殺？
    //public GameObject Tip;
    public GameObject End;          //デバイスの末端についているSphere_End
    public GameObject Device_Tip;
    // public GameObject Device; //hirose 190906
    //public DeviceController dc;   //hirose 190906
    public SerialHandler serialHandler; //hirose 190906
    //[SerializeField]
    //CubeSphere cube;    //hirose 190801
    [SerializeField]
    float force_max_ball1 = 5f; // hirose 190912
    [SerializeField]
    float force_max_ball2 = 3f; // hirose 190912
    [SerializeField]
    float force_max_ball3 = 0f; // hirose 190912
    [SerializeField]
    float force_max = 5f;
    int slider_possision = 0; //hirose 190909

    void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        HandleInput();  //デバイスがオブジェクトに接触しているか判定し，処理を行う
        //}
    }
    void HandleInput()
    {
        //Ray inputRay = new Ray(End.transform.position, Tip.transform.position);//Camera.main.ScreenPointToRay(Input.mousePosition);
        //Ray inputRay = new Ray(End.transform.position, -End.transform.up);
        //Vector3 ETvec = End - Tip;
        Ray inputRay = new Ray(End.transform.position, -End.transform.up);      //デバイスの末端のSphereからデバイスの真下にRayを飛ばす
        RaycastHit hit;     //レイキャストによる情報を得るための構造体
        Debug.DrawLine(End.transform.position, End.transform.position - End.transform.up);  //Rayを可視化
        if (Physics.Raycast(inputRay, out hit, 0.9f)) //sphereが先のとき0.023f, 0.01f  End:0.97f//デバイスの末端のSphereからデバイスの先端までの間でRayの衝突判定を行う
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();      //Rayと衝突したメッシュの情報を取得する

            if (deformer)
            {
                Vector3 point = hit.point;      //Rayの衝突点
                point += hit.normal * forceOffset; //衝突点のベクトルを正規化したものにforceOffsetを掛ける   力を加える点を表面から引き離している
                Vector3 device_tip = Device_Tip.transform.position;     // hirose 190822    デバイスの先端の座標を取得
                //Vector3 displacement_device = device_tip - point;     // hirose 190822 デバイスとオブジェクトの変位
                float displacement_device = device_tip.magnitude - point.magnitude;
                Debug.Log("displacement_device" + displacement_device);

                if (hit.collider.tag == "Ball1")
                {
                    force_max = force_max_ball1;        // hirose 190912
                }
                else if(hit.collider.tag == "Ball2")
                {
                    force_max = force_max_ball2;        // hirose 190912
                }
                else if (hit.collider.tag == "Ball3")
                {
                    force_max = force_max_ball3;        // hirose 190912
                }
                //force = 578.41f * displacement_device * displacement_device + 39.459f * displacement_device +1.6284f;   // hirose 190902    仮想物体へ加える力を計算
                force =41.305f * displacement_device * displacement_device - 28.133f * displacement_device -1.3372f;  //hirose 190909
                //force = 2f;
                if (force >= 0f)
                {
                    if (force > force_max)
                    {
                        force = force_max;
                    }
                    deformer.AddDeformingForce(point, force);   //接触点に力を加える

                    //int slider_possision = (int)(force * 102);  // hirose 190906    スライダーの伸縮する長さを計算（要式計算）
                    slider_possision = (int)(101.43f * force + 194.76f);   //hirose 190909
                    string str = Convert.ToString(slider_possision);    // hirose 190906

                    serialHandler.Write(str + ";");    // hirose 190906
                    Debug.Log("HIT");
                }

                //Debug.DrawLine(Camera.main.transform.position, point);
                
                //transform.gameObject.AddComponent<MeshCollider>();  //hirose 190805
                //MeshCollider mc;  //hirose 190806
                //mc = cube.transform.gameObject.AddComponent<MeshCollider>(); //hirose 190805
                //Destroy(cube.gameObject.GetComponent<MeshCollider>());    //hirose 190806
                //cube.meshcollider = mc;
                //cube.meshcollider.sharedMesh = deformer.deformingMesh;   //hirose 190801
                //cube.ReCreateColliders(); //hirose 190801
                //dc = Device.GetComponent<DeviceController>(); //hirose 190906


            }
           
        }
        else
        {
            serialHandler.Write("1023;");    // hirose 190912
        }
    }
}