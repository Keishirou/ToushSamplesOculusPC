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

    public static int JITTER = 10;  // yama 180720 スライダの位置ずれの閾値

    int deviceL, disEtoO;   // yama 180215 デバイスの長さ，デバイス後端からオブジェクトまでの長さ

    int speed_rank;         // yama 180220 モータの回転速度の段階

    public bool check;      // yama 180220 モータの回転速度を変更するかどうか（仮で作成）

    #region 伸縮予測で使用する変数

    Vector3 preHitO, preHitF;       // yama 180807 前フレームの接触位置
    Vector3 nextHitO, nextHitF;     // yama 180807 予測した次フレームの接触位置

    Vector3 preTipD, preEndD;
    Vector3 nextTipD, nextEndD;

    int nextLength;                 // yama 180807 予測した次フレームの伸縮距離

    #endregion

    int slideMode;
        
    string objTag = "Haptic";   // yama 180822 接触感を停止したい仮想物体のタグ名
    string baseTag = "Base";    // yama 180822 ベース（実物体の机）となる仮想物体のタグ名

    GameObject device;

    void Start()
    {
        sendFlag = 0;
        preLength = 0;
        speed_rank = -1;
        slideMode = 1;

        device = GameObject.Find("Device");

        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;

    }

    void Update()
    {

        #region 現フレームで伸縮させるべき距離の計算

        if (tipD != null && endD != null)    // yama 180214 デバイスの先端，後端の情報を取得しているかどうか
        {
            ray = new Ray(endD.transform.position, (tipD.transform.position - endD.transform.position));
            hits = Physics.RaycastAll(ray.origin, ray.direction, 3);

            foreach (RaycastHit hit in hits)
            {
                //if (hit.collider.name == "Ball")
                //{
                //    hitO = hit.point;
                //    //Debug.Log(hit.collider.name + "; hit.x = " + hit.point.x + ", hit.y = " + hit.point.y + ", hit.z = " + hit.point.z);
                //}
                //else if(hit.collider.name == "Cube")    // yama 180716 Cubeにも対応させたVer
                //{
                //    hitO = hit.point;
                //    //Debug.Log(hit.collider.name + "; hit.x = " + hit.point.x + ", hit.y = " + hit.point.y + ", hit.z = " + hit.point.z);
                //}
                //else if(hit.collider.name == "Floor")
                //{
                //    hitF = hit.point;
                //}
                if (hit.collider.tag == objTag)
                {
                    hitO = hit.point;
                }
                else if (hit.collider.tag == baseTag)
                {
                    hitF = hit.point;
                }
            }
        }

        /* yama 180821 デバイスがめり込んでいるかどうかを判定 */
        if (Check_PenetrateObject(hitO, endD.transform.position))
        {
            sliderLength = Get_HitPointLength(tipD.transform.position, hitF);    // yama 180214 スライダを伸ばす距離をレイによって算出
        }
        else
        {
            sliderLength = Get_HitPointLength(hitO, hitF);    // yama 180214 スライダを伸ばす距離をレイによって算出
        }
        /* ここまで */
        
        #endregion

        #region 仮想物体と机の接触位置をもとに伸縮予測の計算

        //// yama 180807 仮想物体に触れて2フレーム目以降かどうか
        //if (preHitO != null && preHitF != null)
        //{
        //    // yama 180807 デバイスが移動してスライダの伸縮に変化があるか
        //    if (preHitO != hitO || preHitF != hitF)
        //    {
        //        // yama 180807 次フレームの接触点を計算（本来ならばここでテクスチャの長さ（仮想物体の大きさ）をかける必要あり）
        //        // yama 180807 加算する値が等倍だとあまりスムーズに動いている気がしなかったので，2倍に
        //        nextHitO = hitO + (hitO - preHitO) * 3f;
        //        nextHitF = hitF + (hitF - preHitF) * 3f;

        //        nextLength = Get_HitPointLength(nextHitO, nextHitF);
        //        //Debug.Log("sliderLength = " + sliderLength);
        //        Debug.Log("nexrLength = " + nextLength);
        //    }
        //}
        //preHitO = hitO;
        //preHitF = hitF;

        #endregion

        #region デバイスの先端・後端座標をもとに伸縮予測の計算

        if (DtoO)
        {
            if (preTipD != null && preEndD != null)
            {
                if (preTipD != tipD.transform.position || preEndD != endD.transform.position)
                {
                    nextTipD = tipD.transform.position + (tipD.transform.position * 2048.0f - preTipD * 2048.0f) / 1024.0f;     // yama 180813 値が小さいため，情報落ちしないように次フレームのデバイス先端の座標を算出
                    nextEndD = endD.transform.position + (endD.transform.position * 2048.0f - preEndD * 2048.0f) / 1024.0f;     // yama 180813 値が小さいため，情報落ちしないように次フレームのデバイス後端の座標を算出

                    //Debug.Log("Pre; hit.x = " + preTipD.x + ", hit.y = " + preTipD.y + ", hit.z = " + preTipD.z);
                    //Debug.Log("Now; hit.x = " + tipD.transform.position.x + ", hit.y = " + tipD.transform.position.y + ", hit.z = " + tipD.transform.position.z);
                    //Debug.Log("Next; hit.x = " + nextTipD.x + ", hit.y = " + nextTipD.y + ", hit.z = " + nextTipD.z);

                    ray = new Ray(endD.transform.position, (nextTipD - nextEndD));
                    hits = Physics.RaycastAll(ray.origin, ray.direction, 3);

                    foreach (RaycastHit hit in hits)
                    {
                        //if (hit.collider.name == "Ball")
                        //{
                        //    hitO = hit.point;
                        //    //Debug.Log(hit.collider.name + "; hit.x = " + hit.point.x + ", hit.y = " + hit.point.y + ", hit.z = " + hit.point.z);
                        //}
                        //else if (hit.collider.name == "Cube")    // yama 180716 Cubeにも対応させたVer
                        //{
                        //    hitO = hit.point;
                        //    //Debug.Log("Next " + hit.collider.name + "; hit.x = " + hit.point.x + ", hit.y = " + hit.point.y + ", hit.z = " + hit.point.z);

                        //    //hitObjTex = hit.textureCoord;  // yama 180807 現フレームの衝突箇所を取得（ColliderはBox→Meshに変更しないと位置を取得不可）
                        //    //Debug.Log("hitObjTex.x = " + hitObjTex.x + ", hitObjTex.y = " + hitObjTex.y);
                        //}
                        //else if (hit.collider.name == "Floor")
                        //{
                        //    hitF = hit.point;

                        //    //hitFloTex = hit.textureCoord;  // yama 180807 現フレームの衝突箇所を取得（ColliderはBox→Meshに変更しないと位置を取得不可）
                        //    //Debug.Log("hitFloTex.x = " + hitFloTex.x + ", hitFloTex.y = " + hitFloTex.y);
                        //}

                        if (hit.collider.tag == objTag)
                        {
                            hitO = hit.point;
                        }
                        else if (hit.collider.tag == baseTag)
                        {
                            hitF = hit.point;
                        }
                    }

                    /* yama 180821 デバイスがめり込んでいるかどうかを判定 */
                    //if (Check_PenetrateObject(hitO, nextEndD))
                    //{
                    //    nextLength = Get_HitPointLength(nextTipD, hitF);    // yama 180214 スライダを伸ばす距離をレイによって算出
                    //}
                    //else
                    //{
                    //    nextLength = Get_HitPointLength(hitO, hitF);    // yama 180214 スライダを伸ばす距離をレイによって算出
                    //}
                    /* ここまで */

                    nextLength = Get_HitPointLength(hitO, hitF);        // yama 180822 代案が思いつかなかったので，少し伸縮距離は変化するが仮想物体の接触点から床までの長さに変更
                }
            }            
        }

        #endregion

        #region めり込み判定とモータ回転速度変更

        disEtoO = (int)(Vector3.Distance(hitO, endD.transform.position) / 0.5 * 1024);                      // yama 180214 デバイス後端から仮想物体とレイの接触点までの距離

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

        #endregion

        #region 算出した伸縮距離をArduinoに送信

        if (sendFlag == 1)  // yama 180215 この判定で送信処理を行わないと，送信がバグる
        {
            string str;

            if (slideMode == 0)         // yama 180808 伸縮予測を行わない場合
            {
                if (0 <= sliderLength && sliderLength < 1024)    // yama 180122 応急処置、本来であれば別の場所で例外処理をするべき
                { 
                    str = sliderLength.ToString();
                    serialHandler.Write(str + ";");
                }
            }
            else if (slideMode == 1)     // yama 180808 伸縮予測を行う場合
            {                
                if (0 <= nextLength && nextLength < 1024)          // yama 180807 伸縮予測用に変更 
                {
                    str = nextLength.ToString();
                    serialHandler.Write(str + ";");

                    Debug.Log("sliderLength = " + sliderLength);
                    Debug.Log("nextLength = " + nextLength);

                    device.GetComponent<ChangeObjColor>().Change_Color(0);  // yama 180822 伸縮可能時のマテリアルの番号
                }
                else
                {
                    device.GetComponent<ChangeObjColor>().Change_Color(1);  // yama 180822 伸縮不可能時のマテリアルの番号
                }
            }               
                        
            preLength = sliderLength;   // yama 180731 送信後に現フレームの指定位置を更新
        
            sendFlag = 0;   // yama 180731 送信が終了すれば送信可能から送信待機に移行
        }

        #endregion

        #region キーボード操作

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
        if (Input.GetKeyDown(KeyCode.Z))        // yama 180808 伸縮予測を行わないモード
        {
            slideMode = 0;
            Debug.Log("slideMode = " + slideMode);
        }
        if (Input.GetKeyDown(KeyCode.X))        // yama 180808 伸縮予測を行うモード
        {
            slideMode = 1;
            Debug.Log("slideMode = " + slideMode);
        }

        #endregion

        /* yama 180828 デバイス先端・後端ともに一定以上動作した場合のみ前フレームの座標を更新 */
        if (Vector3.Distance(preTipD, tipD.transform.position) > 0.001)
        {
            preTipD = tipD.transform.position;      // yama 180820 tipDはこれ以上使用しないので，前フレームの値を更新
        }
        if (Vector3.Distance(preEndD, endD.transform.position) > 0.001)
        {
            preEndD = endD.transform.position;      // yama 180820 endDはこれ以上使用しないので，前フレームの値を更新
        }
        /* ここまで */

        //float tipDis = Vector3.Distance(preTipD, tipD.transform.position);
        //float endDis = Vector3.Distance(preEndD, endD.transform.position);

        //float tipDis = Vector3.Distance(preTipD * 100.0f, tipD.transform.position * 100.0f);
        //float endDis = Vector3.Distance(preEndD * 100.0f, endD.transform.position * 100.0f);

        //Debug.Log("preTipD.x = " + tipD.transform.position.x + ", preTipD.y = " + tipD.transform.position.y + ", preTipD.z = " + tipD.transform.position.z);
        //Debug.Log("preEndD.x = " + endD.transform.position.x + ", preEndD.y = " + endD.transform.position.y + ", preEndD.z = " + endD.transform.position.z);

        //Debug.Log("tipDis = " + tipDis);
        //Debug.Log("endDis = " + endDis);

        //preTipD = tipD.transform.position;      // yama 180820 tipDはこれ以上使用しないので，前フレームの値を更新
        //preEndD = endD.transform.position;      // yama 180820 endDはこれ以上使用しないので，前フレームの値を更新

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
            if (message != string.Empty)        // yama 180719 受信データが空でないか確認てから処理
            {
                //Debug.Log("move = " + message);

                /* yama 180719 デバイス静止時にスライダの位置が変化した場合の対応 */
                if (sendFlag == 0)      // yama 180731 ここで判定している二つの条件（デバイス静止時，オブジェクトに接触）は同時に判定するとクラッシュする
                {
                    if (DtoO == true)
                    {
                        int diff = int.Parse(message) - preLength;  // yama 180731 現在のスライダ位置と全フレームで指定したスライダの位置の差
                        //Debug.Log("diff = " + diff);

                        if (-JITTER < diff && diff < JITTER)        // yama 180719 スライダの位置ずれが一定範囲内かどうか判定
                        {
                            //string str = sliderLength.ToString();     // yama 180807 一時コメントアウト
                            //string str = nextLength.ToString();         // yama 180807 伸縮予測後のスライダの位置
                            //string str = (preLength / 2).ToString();

                            //if (0 <= preLength && preLength < 1024)
                            //if (0 <= nextLength && nextLength < 1024)   // yama 180807 更新情報を送るのであればnextの長さを判定するべきでは？
                            //{
                            //    serialHandler.Write(str + ";");     // yama 180731 一定範囲内でなければArduinoに更新情報を送信
                            //    Debug.Log("preLength = " + str);
                            //}
                            string str;
                            if (slideMode == 0)
                            {
                                str = sliderLength.ToString();
                                if (0 <= sliderLength && sliderLength < 1024)   // yama 180807 更新情報を送るのであればnextの長さを判定するべきでは？
                                {
                                    serialHandler.Write(str + ";");     // yama 180731 一定範囲内でなければArduinoに更新情報を送信
                                    //Debug.Log("preLength = " + str);
                                }
                            }
                            else if (slideMode == 1)
                            {
                                str = nextLength.ToString();
                                if (0 <= nextLength && nextLength < 1024)   // yama 180807 更新情報を送るのであればnextの長さを判定するべきでは？
                                {
                                    serialHandler.Write(str + ";");     // yama 180731 一定範囲内でなければArduinoに更新情報を送信
                                    //Debug.Log("preLength = " + str);
                                }
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

    /* 後端に配置したからオブジェクトの取得 */
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

    /// <summary>
    /// デバイス後端から延ばしたレイが接触した仮想物体から床までの距離を算出
    /// </summary>
    /// <param name="objPoint"> 計算する距離の始点 </param>
    /// <param name="floorPoint"> 計算する距離の終点 </param>
    /// <returns> 算出した距離 </returns>
    int Get_HitPointLength(Vector3 objPoint, Vector3 floorPoint)
    {
        if(DtoO)    // yama 180214 デバイスが仮想物体に接触しているか（レイではなくコライダが）
        {
            disOtoF = Vector3.Distance(floorPoint, objPoint);
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

    /// <summary>
    /// デバイス先端が仮想物体にめり込んでいるか判定（戻り値はtrue，false）
    /// </summary>
    /// <param name="hitP"> デバイス後端から延ばしたレイと仮想物体の接触箇所 </param>
    /// <param name="end"> デバイスの後端座標 </param>
    /// <returns> めり込んでいる場合はtrue，それ以外の場合はfalse </returns>
    bool Check_PenetrateObject(Vector3 hitP, Vector3 end)
    {
        int dis = (int)(Vector3.Distance(hitP, end) / 0.5 * 1024);

        if (disEtoO + OFFSET < deviceL)      // yama 180214 デバイス全体の長さよりもデバイス後端から仮想物体とレイの接触点までの距離が短い場合（めり込んでいる場合，現在は正確に表面上に置くことは困難なためオフセットあり）
        {
            return false;
        }

        return true;
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
