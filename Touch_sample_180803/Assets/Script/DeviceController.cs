using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceController : MonoBehaviour {
    //先ほど作成したクラス
    public SerialHandler serialHandler;
    int sliderLength;   // yama 180118 スライダを伸ばす距離
    int preLength;    // yama 180118 前フレームにスライダを伸ばした距離
    int sendFlag;   // yama 180118 Arduinoに信号を送るかどうか

    private string message_;
    private bool isNewMessageReceived_ = false;

    GameObject tipD;    // yama 171221 デバイスの先端に配置した空オブジェクト
    GameObject endD;    // yama 180214 デバイスの後端に配置した空オブジェクト
    GameObject tipS;    // yama 171221 スライダの先端に配置した空オブジェクト

    int tip_count = 0;  // yama 171221 空オブジェクトのカウント

    Vector3 sliPos;     // yama 171222 スライダと床の接触位置
    Vector3 overPos;    // yama 171227 スライダとオブジェクトの接触位置（めり込んだ位置）

    bool StoF = false;       // yama 171227 スライダと床が接触しているかどうか
    bool DtoO = false;       // yama 171227 デバイスとオブジェクトが接触しているかどうか

    string device_n = "Device"; // yama 180122 デバイスのオブジェクト名
    string slider_n = "Slider"; // yama 180122 スライダのオブジェクト名

    Ray ray;                // yama 180214 仮想物体と床の接触を判定するためのレイ
    RaycastHit[] hits;      // yama 180214 衝突した物体の情報を確保
    Vector3 hitO, hitF;     // yama 180214 オブジェクト，床との接触情報
    float disOtoF;          // yama 180214 デバイスの接触位置から床までスライドさせる距離

    public static int OFFSET = 5;   // yama 180215 デバイスのめり込みの有無を判断する閾値

    public static int JITTER = 6;  // yama 180720 スライダの位置ずれの閾値

    int deviceL, disEtoO;   // yama 180215 デバイスの長さ，デバイス後端からオブジェクトまでの長さ

    int speed_rank;         // yama 180220 モータの回転速度の段階

    public bool check;      // yama 180220 モータの回転速度を変更するかどうか（仮で作成）

    public const int jitter = 20; // スライダが正しい位置に移動したかを判定する閾値

    void Start()
    {
        sendFlag = 0; //信号を送信するフラグの初期化
        preLength = 0; //前フレームにスライダを伸ばした距離の初期化
        speed_rank = -1; //モータの回転速度の段階の初期化

        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;
    }

    void Update()
    {
        if(tipD != null && endD != null)    // yama 180214 デバイスの先端，後端の情報を取得しているかどうか
        {
            /*デバイスの後端から先端の向きにRay発射*/
            ray = new Ray(endD.transform.position, (tipD.transform.position - endD.transform.position));

            /*Rayが衝突したすべてのオブジェクトを情報を取得*/
            hits = Physics.RaycastAll(ray.origin, ray.direction, 3);//なぜ３？

            /*hitsの中身をすべて参照*/
            foreach (RaycastHit hit in hits)
            {
                /*colliderの名前を判別しRayの衝突位置を取得*/
                if (hit.collider.name == "Ball")
                {
                    hitO = hit.point;
                    //Debug.Log(hit.collider.name + "; hit.x = " + hit.point.x + ", hit.y = " + hit.point.y + ", hit.z = " + hit.point.z);
                }
                else if(hit.collider.name == "Cube")    // yama 180716 Cubeにも対応させたVer
                {
                    hitO = hit.point;
                    //Debug.Log(hit.collider.name + "; hit.x = " + hit.point.x + ", hit.y = " + hit.point.y + ", hit.z = " + hit.point.z);
                }
                else if(hit.collider.name == "Floor")
                {
                    hitF = hit.point;
                }
            }
        }

        //sliderLength = Get_SliderMove();
        sliderLength = Get_HitPointLength();    // yama 180214 スライダを伸ばす距離をレイによって算出

        /*デバイス後端からオブジェクトまでの長さを算出*/
        //0.5*1024とは？？？
        disEtoO = (int)(Vector3.Distance(hitO, endD.transform.position) / 0.5 * 1024);// yama 180214 デバイス後端から仮想物体とレイの接触点までの距離

        if (check)  // yama 180220 モータの回転速度を変更するかどうか（仮で使用）
        {            
            if (disEtoO + OFFSET < deviceL)      // yama 180214 デバイス全体の長さよりもデバイス後端から仮想物体とレイの接触点までの距離が短い場合（めり込んでいる場合，現在は正確に表面上に置くことは困難なためオフセットあり）
            {
                // yama 180220 何度も同じ回転速度を送るのは，無駄なのでここで現在の回転速度と比較
                if (speed_rank != -2)   
                {
                    // yama 180214 ここにスピード調節の関数を呼び出せばOK
                    Debug.Log("めり込み検知！");
                    serialHandler.Write("-2;");

                    speed_rank = -2;
                }
                else
                {
                    if (speed_rank != -1)
                    {
                        serialHandler.Write("-1;");
                        speed_rank = -1;
                    }
                }
            }            

        }


        if (sendFlag == 1)  // yama 180215 この判定で送信処理を行わないと，送信がバグる
        {
            if (0 <= sliderLength && sliderLength < 1024)    // yama 180122 応急処置、本来であれば別の場所で例外処理をするべき
            {
                string str = sliderLength.ToString();
              
                serialHandler.Write(str + ";");     // yama 171127 文字列の送信（スライダの移送距離），";"は終端判定用の文字
                Debug.Log("sliderLength = " + str);

                preLength = sliderLength;   // yama 180731 送信後に現フレームの指定位置を更新
            }

            sendFlag = 0;   // yama 180731 送信が終了すれば送信可能から送信待機に移行
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            serialHandler.Write("0;");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            serialHandler.Write("-1;");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            serialHandler.Write("-2;");
        }

        if (isNewMessageReceived_)
        {
            OnDataReceived(message_);
        }

    }

    /* 受信した信号(message)に対する処理 */
    void OnDataReceived(string message)
    {
        try
        { 
            if (message != string.Empty)        // yama 180719 受信データが空でないか確認してから処理
            {
                Debug.Log("move = " + message);

                /* yama 180719 デバイス静止時にスライダの位置が変化した場合の対応 */
                if (sendFlag == 0)      // yama 180731 ここで判定している二つの条件（デバイス静止時，オブジェクトに接触）は同時に判定するとクラッシュする
                {
                    if (DtoO == true)
                    {
                        int diff = int.Parse(message) - preLength;  // yama 180731 現在のスライダ位置と全フレームで指定したスライダの位置の差

                        if (-JITTER < diff && diff < JITTER)        // yama 180719 スライダの位置ずれが一定範囲内かどうか判定
                        {
                            string str = sliderLength.ToString();
                            //string str = (preLength / 2).ToString();

                            if (0 <= preLength && preLength < 1024) 
                            {
                                serialHandler.Write(str + ";");     // yama 180731 一定範囲内でなければArduinoに更新情報を送信
                                Debug.Log("preLength = " + str);
                            }
                        }
                    }
                }
                /* ここまで */
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    /* 先端に配置した空オブジェクトの取得 */
    void GetTips(GameObject obj)
    {
        if(obj.name.Equals(device_n + "_Tip"))
        {
            tipD = obj;
            tip_count++;
            Debug.Log("Device_Tip position: " + tipD.transform.position);

            if (endD != null)   // yama 180216 デバイス後端の座標がすでに取得できているのであれば
            {
                deviceL = (int)(Vector3.Distance(tipD.transform.position, endD.transform.position) / 0.5 * 1024);   // yama 180214 デバイス全体の長さ（VR空間における）
                Debug.Log("Device_Long: " + deviceL);
            }
        }
        else if(obj.name.Equals(slider_n + "_Tip"))
        {
            tipS = obj;
            tip_count++;
            Debug.Log("Slider_Tip position: " + tipS.transform.position);
        }
    }

    /* 後端に配置した空オブジェクトの取得 */
    void GetEnd(GameObject obj)
    {
        endD = obj;
        Debug.Log("Device_End position: " + endD.transform.position);

        if (tipD != null)   // yama 180216 デバイス先端の座標がすでに取得できているのであれば
        {
            deviceL = (int)(Vector3.Distance(tipD.transform.position, endD.transform.position) / 0.5 * 1024);   // yama 180214 デバイス全体の長さ（VR空間における）
            Debug.Log("Device_Long: " + deviceL);
        }
    }

    /* スライドを動作させる距離の取得と正規化（こちらが正しい距離を算出しているVer） */
    int Get_SliderMove()
    {
        int dis = 1024 - (int)(10240 * Vector3.Distance(sliPos, tipD.transform.position) / 5);
        //int dis = 1024 - (int)(2000 * Vector3.Distance(sliPos, tipD.transform.position));   // yama 180206 ここのモデル式は正確ではないので、しっかり考えること
        float f_dis = Vector3.Distance(sliPos, tipD.transform.position);

        if (dis != preLength && StoF && DtoO)       // yama 180731 前フレームと伸縮位置が変更，かつRayがFloorに接触，かつデバイス先端がオブジェクトに接触している場合
        {
            sendFlag = 1;

            preLength = dis;

            StoF = false;

            //Debug.Log("sliderLength = " + Vector3.Distance(sliPos, tipD.transform.position) / 5);
            //Debug.Log("sliderLength = " + dis);
        }

        return dis;
    }

    /* スライダと床の接触位置を取得 */
    void Get_SliderContact(Vector3 pos)
    {
        sliPos = pos;

        StoF = true;
    }

    /* スライダとオブジェクトの接触位置を取得（めり込んだ距離の判定用） */
    void Get_SliderOverCon(Vector3 pos)
    {
        overPos = pos;
    }

    /* デバイスとオブジェクトが接触したかどうかを判定 */
    void Check_DeviceContact(int flag)
    {
        if(flag == 1)
        {
            DtoO = true;
        }
        else if(flag == 0)
        {
            DtoO = false;
        }      
    }

    /* デバイス後端から延ばしたレイが接触した仮想物体から床までの距離を算出 */
    int Get_HitPointLength()
    {
        if(DtoO)    // yama 180214 デバイスが仮想物体に接触しているか（レイではなくコライダが）
        {
            disOtoF = Vector3.Distance(hitF, hitO);
            //Debug.Log("disOtoF = " + disOtoF);

            int dis = 1024 - (int)(disOtoF / 0.5 * 1024);  // yama 180214 距離をスライダに合わせて正規化（0214時点では 1unit = 20cm）
            //Debug.Log("dis = " + dis);

            if (sliderLength != preLength)
            {
                sendFlag = 1;

                preLength = sliderLength;
            }

            return dis;
        }

        return preLength;
    }

    /* オブジェクトへのめり込みや形状の大きな変化に合わせてスピード調整 */
    int Check_SliderSpeed()
    {
        int speed = 0;

        // yama 171227 オブジェクトにめり込んでいる場合
        if(sliderLength < Vector3.Distance(sliPos, overPos))
        {
            speed = -1;

            return speed;
        }
        // yama 171227 接触しているオブジェクトの形状変化が大きい場合
        //else if (Mathf.Abs(sliderLength - preLength) > 10)
        //{
        //    speed = -1;

        //    return speed;
        //}

        return speed;
    }
}
