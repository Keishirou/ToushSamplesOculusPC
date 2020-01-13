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
    public GameObject Device_Tip;   //デバイス先端のオブジェクト（sphere_tip）
    //public GameObject Sphere_Tip_Ray;  //デバイス先端より前にあるオブジェクト
    public GameObject Device_RTip_Max;  //デバイスが最大まで伸びている位置に対応させたオブジェクト
    // public GameObject Device; //hirose 190906
    //public DeviceController dc;   //hirose 190906
    public SerialHandler serialHandler; //hirose 190906
    public DeviceController deviceController;

    //public GameObject SoftSphere;   //hirose 191115 接触する仮想物体（lineToPointがついてるオブジェクト）
    public lineToPoint ltp;         //hirose 191115 ltpを使用し，Rayと床との接点と床のメッシュの頂点位置との誤差を軽減する試み

    //[SerializeField]
    //CubeSphere cube;    //hirose 190801
    [SerializeField]
    float force_max_ball1 = 5f; // hirose 190912    Ball1の力の上限
    [SerializeField]
    float force_max_ball2 = 3f; // hirose 190912    Ball2の力の上限
    [SerializeField]
    float force_max_ball3 = 0f; // hirose 190912    Ball3の力の上限
    [SerializeField]
    float force_max = 8f;       //  hirose 190912   力の上限
    [SerializeField]
    int slider_possision = 0; //hirose 190909       デバイスの長さ



    int hit_flag = 0;

    public float k = 20f;           // hirose 191101 弾性定数？
    //public float k_min = 10f;       // hirose 191101 弾性定数の最小値
    float k_min = 12f;          //hirose 191128 kの最小値（これ以上小さくすると床以上に仮想物体が凹んでしまう）
    //float devicelen_v_max = 0.51f; //0.527f hirose 191031
    //float devicelen_v_min = 0.007f; //0.016 hirose191031
    //float devicelen_v;                // hirose 191031
    float displacement_device_max;      // hirose 191006 仮想世界におけるデバイスの最大長
   // GameObject yamamotoTip;

    int slider_length_allowable_error = 10; // hirose 191101 デバイスの長さの許容誤差   10
    int pre_slide_length = 0;               // hirose 191101 前フレームのデバイスの長さ
    int count = 0;                          // hirose 191104 デバイスの長さが変わらない場合にデータをn回に1回送信するためのカウント
    public int count_max = 3;               // hirose 191104 上記のn

    int max_force_flag = 0;

    void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        HandleInput();  //デバイスがオブジェクトに接触しているか判定し，処理を行う
        //}

    }
    public void HandleInput()
    {
        //Ray inputRay = new Ray(End.transform.position, Tip.transform.position);//Camera.main.ScreenPointToRay(Input.mousePosition);
        //Ray inputRay = new Ray(End.transform.position, -End.transform.up);
        //Vector3 ETvec = End - Tip;
        //Ray inputRay = new Ray(End.transform.position, -End.transform.up);      //デバイスの末端のSphereからデバイスの真下にRayを飛ばす
        Ray inputRay = deviceController.GetRay();       //デバイスマネージャーからデバイスのRayを取得

        RaycastHit hitO;     //レイキャストによる情報を得るための構造体 オブジェクトとの接点
        Debug.DrawLine(End.transform.position, End.transform.position - End.transform.up);  //Rayを可視化
        if (Physics.Raycast(inputRay, out hitO, 0.83f)) //sphereがEnd:0.9f//デバイスの末端のSphereからデバイスの先端までの間でRayの衝突判定を行う
        {
            MeshDeformer deformer = hitO.collider.GetComponent<MeshDeformer>();      //Rayと衝突したメッシュの情報を取得する
            hit_flag = 1;
            if (deformer)
            {
                Vector3 point = hitO.point;      //Rayとオブジェクトとの衝突点
                point += hitO.normal * forceOffset; //衝突点のベクトルを正規化したものにforceOffsetを掛ける   力を加える点を表面から引き離している
                //Vector3 device_tip = Device_Tip.transform.position;     // hirose 190822    デバイスの先端の座標を取得
                //Vector3 displacement_device = device_tip - point;     // hirose 190822 デバイスとオブジェクトの変位
                //float displacement_device = device_tip.magnitude - point.magnitude;
                GameObject yamamotoTip = deviceController.GetDeviceTip();   // デバイス先端
                GameObject yamamotoEnd = deviceController.GetDeviceEnd();   // デバイス後端

                Vector3 hit_sphere_point_on_ray = ltp.PerpendicularFootPoint(yamamotoTip.transform.position, yamamotoEnd.transform.position, hitO.point);       // hirose 191119 Rayとコライダーの接点の頂点からRayに垂線を下した座標
                //Vector3 hit_sphere_point_on_ray = ltp.PerpendicularFootPoint(yamamotoTip.transform.position, yamamotoEnd.transform.position, hit_collider_point);   // hirose 191119
                //hit_sphere_point_on_ray += hitO.normal * forceOffset; // hirose 191118
                float displacement_device = Vector3.Distance(yamamotoTip.transform.position, hit_sphere_point_on_ray);    // hirose 191118 Rayとコライダーとの接点とデバイス先端の差（MTG資料のdist）

                //float displacement_device = Vector3.Distance(device_tip, point);
                //float displacement_device = Vector3.Distance(yamamotoTip.transform.position, point);    //Rayとコライダーとの接点とデバイス先端の差（MTG資料のdist）
                //Debug.Log("displacement_device" + displacement_device);

                /* 力の上限を接触した物体の力上限に変更 */
                if (hitO.collider.tag == "Ball1")
                {
                    force_max = force_max_ball1;        // hirose 190912    
                }
                else if(hitO.collider.tag == "Ball2")
                {
                    force_max = force_max_ball2;        // hirose 190912
                }
                else if (hitO.collider.tag == "Ball3")
                {
                    force_max = force_max_ball3;        // hirose 190912
                }

                
                //force = 578.41f * displacement_device * displacement_device + 39.459f * displacement_device +1.6284f;   // hirose 190902    仮想物体へ加える力を計算
                //force =41.305f * displacement_device * displacement_device - 28.133f * displacement_device -1.3372f;  //hirose 190909     sphereの位置を変えたため使えない

                // hirose 191128 kの値が最小値未満にならないようにする処理
                if(k < k_min)
                {
                    k = k_min;
                }


                //force = k * displacement_device * displacement_device;  //hirose 191001   (79f) デバイスの位置に合わせてFを計算する式 fmax = 5f, k = 60f, k_min = 48f
                force = k * displacement_device;  //hirose 191001   (79f) デバイスの位置に合わせてFを計算する式  k_min = 20f, fmax = 3f

                //force = 2f;
                //force = force_max;

                if (force >= 0f)
                {
                    /* 力が上限を超えた場合の処理 */
                    max_force_flag = 0;
                    if (force >= force_max)
                    {
                        force = force_max;      // forceを制限
                        max_force_flag = 1;     // hirose 191023 Aruduinoに送るためのフラグ
                        displacement_device_max = force / k;        // hirose 191101  弾性定数kによりdistの最大値を計算（kのFmaxのdist）
                        //displacement_device_max = Mathf.Sqrt(displacement_device_max);  //hirose 191101　弾性定数kによりdistの最大値を計算
                        displacement_device = displacement_device_max;  //hirose 191101　distを制限

                    }
                    
                    //force = k_min * displacement_device * displacement_device;  //hirose 191101 kmin(kmin < k)でFを再計算
                    //force = k_min * displacement_device;  //hirose 191101 kmin(kmin < k)でFを再計算
                    //Debug.Log("デバイスとコライダーの差：" + displacement_device);

                    RaycastHit hit_table;                           // hirose 191002 床とRayの接触判定
                    if (Physics.Raycast(inputRay, out hit_table)) // hirose 191002 床とRayの接触判定
                    {
                        Vector3 yamamotoHit = deviceController.GetHitBase();    //Rayと床の接点
                        //Vector3 hit_table_point = hit_table.point;     //hirose 191030  Rayと床との接点
                                                                       //Vector3 checkpoint = hit_table.point;      // hirose 191002 Rayの衝突点
                                                                       //float chack_distance = device_tip.magnitude - checkpoint.magnitude; // hirose 191002    デバイスの先端とRayと机の接触点の差
                                                                       //Debug.Log("デバイスと机の差：" + chack_distance);
                                                                       //slider_possision = (int)(65409f * chack_distance * chack_distance * chack_distance + 17800f * chack_distance * chack_distance - 1927.9f * chack_distance + 98.326f);  
                                                                       // ↑ hirose 191002 伸縮計算　力でデバイスの長さを計算する式で発生する問題を解決しようとした式

                        Vector3 hit_floor_point_on_ray = ltp.PerpendicularFootPoint(yamamotoTip.transform.position, yamamotoEnd.transform.position, yamamotoHit);   // hirose 191108 床のメッシュの接触点からRayに垂線を下した座標


                        //slider_possision = (int)(65409f * chack_distance * chack_distance * chack_distance + 17800f * chack_distance * chack_distance - 1928f * chack_distance + 98f);  // hirose 191002 伸縮計算
                        //slider_possision = (int)(100f* force - 20f * chack_distance + 100f);

                        //slider_possision = (int)(-4.1071f * force * force + 129.39f * force + 102.5f);    // hirose 191003   最初のデバイスの位置を計算する式（力を利用）

                        //slider_possision = (int)(-1.6667f * force * force * force * force + 15.926f * force * force * force - 42.778f * force * force + 132.33f * force + 99.365f); // hirose 191003    途中でカクつく
                        //force = 397.85f * chack_distance * chack_distance * chack_distance + 89.632f * chack_distance * chack_distance - 24.662f * chack_distance - 0.044f;

                        //float disOtoF = Vector3.Distance(device_tip, hit_table_point);

                        //float deviceLen = Vector3.Distance(yamamotoTip.transform.position, yamamotoEnd.transform.position);
                        //devicelen_v = devicelen_v_max - devicelen_v_min;  //hirose 191031
                        //float disOtoF = Vector3.Distance(yamamotoTip.transform.position, yamamotoHit);    //デバイスの長さを制限できない

                        //float len_v_max = Vector3.Distance(hitO.point, yamamotoHit);    // hirose 191101

                        //float lend_v_max = Vector3.Distance(yamamotoEnd.transform.position, yamamotoTip.transform.position);    // hirose 191108
                        float lend_v_max = Vector3.Distance(Device_RTip_Max.transform.position, yamamotoTip.transform.position);    // hirose 191108 仮想世界における実際のデバイスの最大長
                        //float len_v_max = Vector3.Distance(hitO.point, yamamotoHit);    // hirose 191101 コライダーとの接点と床との接点の差
                        float len_v_max = Vector3.Distance(hitO.point, hit_floor_point_on_ray);    // hirose 191115 コライダーとの接点と床との接点の差
                        float disOtoF = len_v_max - displacement_device;              // hirose 191101 仮想世界におけるデバイスの伸縮する長さ

                        //int dis = 1024 - (int)(disOtoF / 0.5 * 1024); //正規化
                        //int dis =(int)(disOtoF * 1024.0f / deviceLen);

                        //int dis = (int)(1023f - (disOtoF / devicelen_v * 1023f + devicelen_v_min));
                        //int dis = (int)(1023f - (disOtoF / (0.52f - 0.007f) * 1023f));
                        //int dis = (int)(1023f - (disOtoF / (0.39f - 0.001f) * 1023f));

                        //slider_possision = (int)(1023f - (disOtoF / (0.39f - 0.001f) * 1023f));
                        slider_possision = (int)(1023f - (disOtoF / lend_v_max * 1023f));   // hirose 191108    デバイスの伸縮する長さを正規化
                        //slider_possision = 1024 - (int)((disOtoF / 0.5f * 1024f));      //hirose 200108



                        //slider_possision = (int)(disOtoF / lend_v_max * 1023f);           // hirose 191108
                        //Debug.Log("デバイスと床との距離：" + dis);

                        //Debug.Log("仮想空間でのデバイスの最大長：" + lend_v_max);
                        //Debug.Log("仮想物体のコライダーとの接点と床との差：" + len_v_max);

                        //slider_possision -= (int)k; // hirose 191118  デバイスの押し込む感覚を出すためにデバイスの長さを長くする

                        //Debug.Log("デバイスと床との距離：" + slider_possision);

                        //slider_possision = Get_HitFloorPointLength(device_tip, hit_table_point);

                        /* デバイスの長さを調整する処理 */                     
                        if (slider_possision < 0)
                        {
                            slider_possision = 0;
                        }
                        else if (slider_possision > 1023)
                        {
                            slider_possision = 1023;
                        }
                        

                        //Debug.Log("長さ：" + slider_possision);
                    }
                    deformer.AddDeformingForce(point, force, k);   //接触点に力を加える
                    /*
                    // hirose 191101 Aruduinoに送信する情報量を制限
                    if (Mathf.Abs(pre_slide_length - slider_possision) > slider_length_allowable_error || count >= count_max)
                    { // || pre_slide_length > slider_possision
                        string str = Convert.ToString(slider_possision);    // hirose 190906 Aruduinoに送信するために文字列化
                        string str_flag = Convert.ToString(max_force_flag);
                        serialHandler.Write(str + "," + str_flag + ";");    // hirose 190906 Aruduinoに送信
                        //serialHandler.Write(str + ";");                             // hirose 190906 Aruduinoに送信
                        count = 0;  // hirose 191106 送信を制限するためのカウントをリセット
                    }
                    */
                    //serialHandler.Write("0");
                    pre_slide_length = slider_possision;    //1フレーム前のデバイスの長さを保存
                    //Debug.Log("force_flag:" + max_force_flag);
                    count++;  // hirose 191106  送信を制限するためのカウントを増やす

                    //Debug.Log("HIT");
                }
             }
         }
        else
        {
            hit_flag = 0;
            //serialHandler.Write("0;");
            //string str = Convert.ToString(slider_possision);    // hirose 190906

            //serialHandler.Write(str + ";");    // hirose 190906

            //serialHandler.Write("1023;");    // hirose 190912
            // string str = Convert.ToString(slider_possision);    // hirose 190906
            //serialHandler.Write(str + ",0;");    // hirose 190906
        }

    }

    /* デバイスの先端の座標と床とRayの接触点の差によりデバイスの長さを求める */  //hirose 191030
    int Get_HitFloorPointLength(Vector3 objPoint, Vector3 floorPoint)
    {
        float disOtoF = Vector3.Distance(floorPoint, objPoint);
        //Debug.Log("disOtoF = " + disOtoF);

        // yama 181025 伸縮距離計算の係数を1.06->1.08に変更
        int dis = 1024 - (int)(disOtoF / 0.5 * 1024);  // yama 180214 距離をスライダに合わせて正規化（0214時点では 1unit = 20cm）
                                                       //Debug.Log("dis = " + dis);

        return dis;
    }

    public int send_hit_flag()
    {
        return hit_flag;
    }
    public float send_force()
    {
        return force;
    }
    public int send_force_flag()
    {
        Debug.Log("send_flag: "+ max_force_flag);
        return max_force_flag;
    }
    public int send_slider_length()
    {
        Debug.Log("send_slider"+ slider_possision);
        return slider_possision;
    }
}