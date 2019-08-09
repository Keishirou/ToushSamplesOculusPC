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
    
    public bool DtoO = false;       // yama 171227 デバイスとオブジェクトが接触しているかどうか
    bool preDtoO = false;   // yama 181031 前フレームでデバイスとオブジェクトが接触していたか

    Ray ray;                // yama 180214 仮想物体と床の接触を判定するためのレイ
    RaycastHit[] hits;      // yama 180214 衝突した物体の情報を確保
    Vector3 hitO, hitF;     // yama 180214 オブジェクト，床との接触情報
    float disOtoF;          // yama 180214 デバイスの接触位置から床までスライドさせる距離

    public static int OFFSET = 5;   // yama 180215 デバイスのめり込みの有無を判断する閾値

    public static int JITTER = 10;  // yama 181022 スライダの位置ずれの閾値

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

    public Vector3[] preTipVec, preEndVec;  // yama 180907 t-nフレーム前のデバイス先端・後端の移動距離
    int preVecNum;  // yama 180907 伸縮予測に使用するフレーム数

    Vector3 tempTipD;                       // yama 181128 デバイスを事前に伸縮させるために利用する変数
    Vector3 tempEndD;   // yama 181119 デバイスのめり込み量を出力するために必要な変数

    OVRHapticsClip hapticsClip;
    private byte[] samples = new byte[40];
    HapticExample he;

    // yama 181215 この変数をまだ活用できていないので注意（正確な距離出すには使用する必要あるかも）
    const double rvDis = 0.521 * 0.9;
    int hitCount = 0;
    int dis_HitOtoTipD;

    Vector3 hitNormalO, hitNormalF;     // yama 181215 仮想物体と床の法線

    private int feedbackPattern = 1;//1;         // yama 181220 どのフィードバックで行うか指定

    void Start()
    {
        sendFlag = 0;
        preLength = 0;
        speed_rank = -1;
        slideMode = 1;

        preVecNum = 0;

        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;

        he = this.GetComponent<HapticExample>();

        if(feedbackPattern != 0)
        {
            serialHandler.Write("1023;");
        }
    }

    private void Update()
    {
        #region 現フレームで伸縮させるべき距離の計算

        if (tipD != null && endD != null)    // yama 180214 デバイスの先端，後端の情報を取得しているかどうか
        {
            //Debug.Log("Middle; hit.x = " + tipD.transform.position.x + ", hit.y = " + tipD.transform.position.y + ", hit.z = " + tipD.transform.position.z);
            ray = new Ray(endD.transform.position, (tipD.transform.position - endD.transform.position));
            hits = Physics.RaycastAll(ray.origin, ray.direction, 3);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.tag == objTag)
                {
                    hitO = hit.point;

                    hitNormalO = hit.normal;    // yama 181215 デバイスの方向ベクトルが接触した位置の仮想物体の法線

                    hitCount++;     // yama 181214 接触前伸縮をスムーズに行うために追加
                }
                else if (hit.collider.tag == baseTag)
                {
                    hitF = hit.point;           // yama 181215 デバイスの方向ベクトルが接触した位置の床の法線

                    hitNormalF = hit.normal;

                    hitCount++;     // yama 181214 接触前伸縮をスムーズに行うために追加
                }
            }
        }
        tempTipD = tipD.transform.position;
        tempEndD = endD.transform.position; // yama 181119 デバイスのめり込み量を出力するために必要な変数

        /* yama 180821 デバイスがめり込んでいるかどうかを判定 */
        if (Check_PenetrateObject(hitO, tempEndD))
        {
            //Debug.Log("OK1");
            sliderLength = Get_HitPointLength(hitO, hitF);    // yama 180214 スライダを伸ばす距離をレイによって算出
        }
        else
        {
            //Debug.Log("OK2");
            sliderLength = Get_HitPointLength(tempTipD, hitF);    // yama 180214 スライダを伸ばす距離をレイによって算出
        }


        /* ここまで */

        #endregion

        #region 仮想物体と机の接触位置をもとに伸縮予測の計算

        //if (DtoO)
        //{
        //    // yama 180807 仮想物体に触れて2フレーム目以降かどうか
        //    if (preHitO != null && preHitF != null)
        //    {
        //        // yama 180807 デバイスが移動してスライダの伸縮に変化があるか
        //        if (preHitO != hitO || preHitF != hitF)
        //        {
        //            // yama 180807 次フレームの接触点を計算（本来ならばここでテクスチャの長さ（仮想物体の大きさ）をかける必要あり）
        //            // yama 180807 加算する値が等倍だとあまりスムーズに動いている気がしなかったので，2倍に
        //            nextHitO = hitO + (hitO - preHitO) * 2f;
        //            nextHitF = hitF + (hitF - preHitF) * 2f;

        //            nextLength = Get_HitPointLength(nextHitO, nextHitF);
        //            //Debug.Log("sliderLength = " + sliderLength);
        //            //Debug.Log("nexrLength = " + nextLength);
        //        }
        //    }
        //    preHitO = hitO;
        //    preHitF = hitF;
        //}

        #endregion

        #region デバイスの先端・後端座標をもとに伸縮予測の計算

        // yama 181214 ここまで修正完了，以下のコメントアウトはがたつきに影響があるようなので考慮する必要あり

        dis_HitOtoTipD = (int)(Vector3.Distance(tempTipD, hitO) / 0.5 * 1000);
        float dot = Vector3.Dot(tempTipD - tempEndD, tempTipD - hitO);
        if (dis_HitOtoTipD <= 20 || dot > 0)
        //if (DtoO)
        {
            if (preTipD != Vector3.zero && preEndD != Vector3.zero)
            {
                if (preTipD != tipD.transform.position || preEndD != endD.transform.position)
                {
                    #region 加重移動平均の算出と伸縮予測

                    nextTipD = tipD.transform.position;
                    nextEndD = endD.transform.position;

                    Vector3 nextMotionTip, nextMotionEnd;
                    int sumWeight = (preVecNum + 1) * preVecNum / 2;

                    nextMotionTip = Vector3.zero;
                    nextMotionEnd = Vector3.zero;

                    for (int i = 0; i < preVecNum; i++)
                    {
                        //nextMotionTip += preTipVec[i] * (i + 1);
                        //nextMotionEnd += preEndVec[i] * (i + 1);

                        /* yama 180910 HMD起動時フレームレートが1/2になり，結果としてフレーム間の移動距離が大幅に増大してしまう
                                       そこで対策として，移動距離がHMD停止時よりもどれだけ増加しているか確認し，大体15倍だったので予測移動距離を1/15にした */
                        nextMotionTip += preTipVec[i] * (i + 1);
                        nextMotionEnd += preEndVec[i] * (i + 1);
                    }

                    //Debug.Log("nextMotionTip = " + nextMotionTip.sqrMagnitude);

                    nextTipD += nextMotionTip / sumWeight;
                    nextEndD += nextMotionEnd / sumWeight;

                    //Debug.Log("add:x = " + nextMotionTip.x / sumWeight + ", y = " + nextMotionTip.y / sumWeight + ", z = " + nextMotionTip.z / sumWeight);
                    //Debug.Log("nextTipD:x = " + nextTipD.x + ", y = " + nextTipD.y + ", z = " + nextTipD.z);

                    #endregion

                    //nextTipD = tipD.transform.position + (tipD.transform.position - preTipD);     // yama 180813 値が小さいため，情報落ちしないように次フレームのデバイス先端の座標を算出
                    //nextEndD = endD.transform.position + (endD.transform.position - preEndD);     // yama 180813 値が小さいため，情報落ちしないように次フレームのデバイス後端の座標を算出

                    //nextTipD = tipD.transform.position + (tipD.transform.position * 2048.0f - preTipD * 2048.0f) / 1024.0f;     // yama 180813 値が小さいため，情報落ちしないように次フレームのデバイス先端の座標を算出
                    //nextEndD = endD.transform.position + (endD.transform.position * 2048.0f - preEndD * 2048.0f) / 1024.0f;     // yama 180813 値が小さいため，情報落ちしないように次フレームのデバイス後端の座標を算出

                    //Debug.Log("Pre; hit.x = " + preTipD.x + ", hit.y = " + preTipD.y + ", hit.z = " + preTipD.z);
                    //Debug.Log("Now; hit.x = " + tipD.transform.position.x + ", hit.y = " + tipD.transform.position.y + ", hit.z = " + tipD.transform.position.z);
                    //Debug.Log("Next; hit.x = " + nextTipD.x + ", hit.y = " + nextTipD.y + ", hit.z = " + nextTipD.z);

                    //Debug.Log("Pre; hit.x = " + preTipD.x + ", hit.y = " + preTipD.y + ", hit.z = " + preTipD.z);
                    //Debug.Log("Now; hit.x = " + tipD.transform.position.x + ", hit.y = " + tipD.transform.position.y + ", hit.z = " + tipD.transform.position.z);
                    //Debug.Log("Next; hit.x = " + nextTipD.x + ", hit.y = " + nextTipD.y + ", hit.z = " + nextTipD.z);

                    //Debug.Log("Distance = " + Vector3.Distance(preTipD, tipD.transform.position));

                    ray = new Ray(endD.transform.position, (nextTipD - nextEndD));
                    hits = Physics.RaycastAll(ray.origin, ray.direction, 3);

                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider.tag == objTag)
                        {
                            nextHitO = hit.point;
                        }
                        else if (hit.collider.tag == baseTag)
                        {
                            nextHitF = hit.point;
                        }
                    }

                    /* yama 180821 デバイスがめり込んでいるかどうかを判定 */
                    if (Check_PenetrateObject(nextHitO, nextEndD))
                    {
                        //Debug.Log("OK3");
                        nextLength = Get_HitPointLength(nextTipD, nextHitF);    // yama 180214 スライダを伸ばす距離をレイによって算出
                    }
                    else
                    {
                        //Debug.Log("OK4");
                        nextLength = Get_HitPointLength(nextHitO, nextHitF);    // yama 180214 スライダを伸ばす距離をレイによって算出
                    }
                    /* ここまで */

                    //Debug.Log("distance = " + nextLength);
                }
            }
            // yama 180830 初めて接触したときはsliderLengthの値をそのまま使う
            else
            {
                nextLength = sliderLength;
                //Debug.Log("before sliderLength = " + sliderLength);
                //Debug.Log("before nextLength = " + nextLength);
            }

            #region 0907追加分

            if (preTipD != Vector3.zero && preEndD != Vector3.zero)
            {
                // yama 180907 使用するフレーム数分の移動距離が格納されていないとき
                if (preVecNum < preTipVec.Length)
                {
                    preTipVec[preVecNum] = tipD.transform.position - preTipD;
                    preEndVec[preVecNum] = endD.transform.position - preEndD;
                    preVecNum++;
                }
                // yama 180907 使用するフレーム数分の移動距離が格納されているとき
                else
                {
                    if (preVecNum > 0)
                    {
                        for (int i = 0; i < preTipVec.Length - 1; i++)
                        {
                            preTipVec[i] = preTipVec[i + 1];
                            preEndVec[i] = preEndVec[i + 1];
                        }
                        //Debug.Log("tipD.x = " + tipD.transform.position.x + ", preTipD.x = " + preTipD.x);
                        preTipVec[preVecNum - 1] = tipD.transform.position - preTipD;
                        preEndVec[preVecNum - 1] = endD.transform.position - preEndD;
                    }
                }
            }

            #endregion

            preTipD = tipD.transform.position;      // yama 180820 tipDはこれ以上使用しないので，前フレームの値を更新
            preEndD = endD.transform.position;
        }
        // yama 180830 接触していないときはpreTipD，preEndDを更新しない（伸縮予測の値がバグらないようにするため）
        else
        {
            preTipD = Vector3.zero;
            preEndD = Vector3.zero;

            for (int i = 0; i < preVecNum; i++)
            {
                preTipVec[i] = Vector3.zero;
                preEndVec[i] = Vector3.zero;
            }
            preVecNum = 0;

            // yama 181031 前フレームまでデバイスが仮想物体に触れていた場合
            if (preDtoO)
            {
                // yama 181031 負の数字を送ることでArduino側でモータを停止させる
                serialHandler.Write("-1;");
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

        if (feedbackPattern == 0)       // yama 181220 力覚フィードバックで行う場合
        {
            if (sendFlag == 1)  // yama 180215 この判定で送信処理を行わないと，送信がバグる
            {
                string str;

                if (!preDtoO && DtoO)
                {
                    serialHandler.Write("-3;");
                    preDtoO = DtoO;
                }

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

                        //Debug.Log("sliderLength = " + sliderLength);
                        //Debug.Log("nextLength = " + nextLength);

                        //this.GetComponent<ChangeObjColor>().Change_Color(0);  // yama 180822 伸縮可能時のマテリアルの番号
                    }
                    else
                    {
                        //this.GetComponent<ChangeObjColor>().Change_Color(1);  // yama 180822 伸縮不可能時のマテリアルの番号
                    }
                }

                preLength = sliderLength;   // yama 180731 送信後に現フレームの指定位置を更新

                sendFlag = 0;   // yama 180731 送信が終了すれば送信可能から送信待機に移行
            }
            else
            {
                // yama 181215 ここでスピードを変更すべきだが，がたつきが酷いため変更せず
                if (preDtoO && !DtoO)
                {
                    serialHandler.Write("-2;");
                    preDtoO = DtoO;
                    //Debug.Log("OK2");
                }
                if (hitCount == 2) // yama 181212 デバイスのレイが床とオブジェクトの両方に接触しているとき
                {
                    //Write_PreLength(); // yama 181212 事前の伸縮を実行
                }
            }
            hitCount = 0; // yama 181212 レイのヒット数をリセット
        }
        else if (feedbackPattern == 1)      // yama 181220 振動フィードバックで行う場合
        {
            Play_HpticFeedback();   // yama 181206 対象に一定以上近づいていればコントローラが振動
        }

        #endregion

        #region キーボード操作

        if (Input.GetKeyDown(KeyCode.A))
        {
            serialHandler.Write("-1;");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            serialHandler.Write("-2;");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            serialHandler.Write("-3;");
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

        if (isNewMessageReceived_)
        {
            OnDataReceived(message_);
        }

        //Debug.Log("time = " + Time.deltaTime);
        Debug.Log(feedbackPattern);
    }

    /* 受信した信号(message)に対する処理 */
    void OnDataReceived(string message)
    {
        try
        { 
            if (message != string.Empty)        // yama 180719 受信データが空でないか確認てから処理
            {
                if (feedbackPattern == 0)       // yama 181224 伸縮処理を行うフィードバックの場合
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
                else
                {
                    int length = int.Parse(message);

                    if((length + JITTER) > 1023)
                    {
                        serialHandler.Write("1023;");
                    }
                }
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
        if(obj.name.Equals(this.name + "_Tip"))
        {
            tipD = obj;

            if (endD != null)   // yama 180216 デバイス後端の座標がすでに取得できているのであれば
            {
                deviceL = (int)(Vector3.Distance(tipD.transform.position, endD.transform.position) / 0.5 * 1024);   // yama 180214 デバイス全体の長さ（VR空間における）
                Debug.Log("Device_Long: " + deviceL);

                
            }
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

    /// <summary>
    /// デバイスとオブジェクトが接触したかどうかを判定
    /// </summary>
    /// <param name="flag">仮想物体に触れたかどうかを0．1で</param>
    public void Check_DeviceContact(int flag)
    {
        preDtoO = DtoO;
        
        if (flag == 1)
        {
            DtoO = true;
        }
        else if(flag == 0)
        {
            DtoO = false;      
        }      
    }

    /// <summary>
    /// デバイスのベクトルと仮想物体の接触点の法線のなす角から伸縮を行うか判定
    /// </summary>
    /// <param name="conNormal">仮想物体の接触点の法線</param>
    void Check_ContactAngle(Vector3 conNormal)
    {
        // yama 180914 デバイスの方向ベクトルと仮想物体接触点とのなす角
        float angle = Vector3.Angle(tipD.transform.position - endD.transform.position, conNormal);

        // yama 181120 ここのコメントアウトを取れば，接触角度による判定が可能
        //if (angle < 110)
        //{
        //    Check_DeviceContact(0);
        //}
        //else
        //{
        //    Check_DeviceContact(1);
        //}

        Check_DeviceContact(1);
    }

    /// <summary>
    /// デバイス後端から延ばしたレイが接触した仮想物体から床までの距離を算出
    /// </summary>
    /// <param name="objPoint"> 計算する距離の始点 </param>
    /// <param name="floorPoint"> 計算する距離の終点 </param>
    /// <returns> 算出した距離 </returns>
    int Get_HitPointLength(Vector3 objPoint, Vector3 floorPoint)
    {     
        if (sendFlag == 0)  // yama 180830 送信待ち状態のときのみ計算（理由は実装履歴参照）
        {
            if (DtoO)    // yama 180214 デバイスが仮想物体に接触しているか（レイではなくコライダが）
            {
                disOtoF = Vector3.Distance(floorPoint, objPoint);
                //Debug.Log("disOtoF = " + disOtoF);

                // yama 181025 伸縮距離計算の係数を1.06->1.08に変更
                int dis = 1024 - (int)(disOtoF / 0.5 * 1024);  // yama 180214 距離をスライダに合わせて正規化（0214時点では 1unit = 20cm）
                                                               //Debug.Log("dis = " + dis);

                if (sliderLength != preLength)
                {                 
                    sendFlag = 1;

                    preLength = sliderLength;
                }

                return dis;
            }
        }

        return preLength;   // yama 180830 送信待ち状態でなければ前回の距離を返す
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

        if (dis + JITTER < deviceL)      // yama 180214 デバイス全体の長さよりもデバイス後端から仮想物体とレイの接触点までの距離が短い場合（めり込んでいる場合，現在は正確に表面上に置くことは困難なためオフセットあり）
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 仮想物体へのデバイスのめり込み量を返す
    /// </summary>
    /// <returns></returns>
    public string Set_PenetrationDistance()
    {
        string data;
        // yama 181121 現在10cmがVR空間内ではAvaterのローカル空間で0.5なので0.5で割る，その後mmで正規化するために10cm=100mmなので100をかける
        float dis = (float)(Vector3.Distance(hitO, tempTipD) / 0.5 * 100);

        double diff = dis * 100 - (int)(dis * 100);

        if (diff >= 0.5)
        {
            dis = (int)(dis * 100 + 1);
            dis = dis / 100;
        }
        else
        {
            dis = (int)(dis * 100);
            dis = dis / 100;
        }

        data = dis.ToString();

        return data;
    }

    /// <summary>
    /// 仮想物体へのデバイスのY座標のめり込み量を返す
    /// </summary>
    /// <returns></returns>
    public string Set_PenetYDistance()
    {
        string data;

        Vector3 down = tempTipD + new Vector3(0, 0.5f, 0);
        Vector3 point = new Vector3(0, 0, 0);
        double dis;

        Ray rayY = new Ray(down, new Vector3(0, -1, 0));        // yama 1218 デバイス先端の真上からY軸方向下向きにレイを照射
        RaycastHit[] hitYs = Physics.RaycastAll(rayY.origin, rayY.direction, 3);

        foreach (RaycastHit hit in hitYs)
        {

            if (hit.collider.tag == objTag)
            {
                if (point.y < hit.point.y)      // yama 181212 現在のヒット場所よりも高いとき
                {
                    point = hit.point;

                }
            }
        }

        dis = (Vector3.Distance(hitO, tempTipD) / 0.5 * 100);

        double diff = dis * 100 - (int)(dis * 100);

        if (diff >= 0.5)
        {
            dis = (int)(dis * 100 + 1);
            dis = dis / 100;
        }
        else
        {
            dis = (int)(dis * 100);
            dis = dis / 100;
        }

        if (point.y < tempTipD.y)
        {
            dis = -dis;
        }

        data = dis.ToString();

        return data;
    }

    /// <summary>
    /// 仮想物体に触れていない場合であってもある程度長さを変化させておくことで，実際に触れる瞬間の違和感を軽減する関数
    /// </summary>
    void Write_PreLength()
    {
        int diff = (int)(Vector3.Distance(hitO, tempTipD)  / 0.5 * 1000);  // yama 181128 デバイス先端が仮想物体表面にどれだけ近づいたか
        
        if (diff > (JITTER + 20) && diff < 500 && !DtoO)        // yama 181128 一定以上近づいている&デバイスが仮想物体に触れていない場合
        {
            string str = (1024 - (int)(Vector3.Distance(hitO, hitF) / 0.5 * 1024)).ToString();
            serialHandler.Write(str + ";");
            //Debug.Log("slider = " + str);
        }
    }

    /// <summary>
    /// コントローラがオブジェクトに接触していれば振動させる
    /// </summary>
    void Play_HpticFeedback()
    {
        int diff = (int)(Vector3.Distance(hitO, tempTipD) / rvDis * 1024);  // yama 181128 デバイス先端が仮想物体表面にどれだけ近づいたか

        if (diff < 20)      // yama 181206 一定距離以内であれば
        {
            //int dis = deviceL - (int)(Vector3.Distance(hitO, tempEndD) / rvDis * 1024);
            //Debug.Log("dis = " + dis);
            he.Play_HapticClip(1);
        }
        else
        {
            he.Play_HapticClip(0);
        }
    }

    /// <summary>
    /// デバイス先端とオブジェクトの接触点の距離を返す
    /// </summary>
    /// <returns>デバイス先端とオブジェクトの接触点の距離</returns>
    public int Set_DisHitOtoTipD()
    {
        return dis_HitOtoTipD;
    }

    public Vector3 Set_tipD()
    {
        return tempTipD;
    }

    public Vector3 Set_hitO()
    {
        return hitO;
    }

    /// <summary>
    /// 2つのベクトルのなす角度を計算
    /// </summary>
    /// <param name="A">ベクトルA</param>
    /// <param name="B">ベクトルB</param>
    /// <returns>なす角度</returns>
    public double AngleOf2Vector(Vector3 A, Vector3 B)
    {
        double sita = 90;

        //ベクトルAとBの長さを計算する
        float length_A = A.magnitude;
        float length_B = B.magnitude;

        if (length_A > 0 && length_B > 0)
        {
            //内積とベクトル長さを使ってcosθを求める
            float cos_sita = Vector3.Dot(A, B) / (length_A * length_B);

            //cosθからθを求める
            sita = Mathf.Acos(cos_sita);

            //ラジアンでなく0～180の角度でほしい場合はコメント外す
            sita = sita * 180.0 / Mathf.PI;
        }

        return sita;
    }

    /// <summary>
    /// 床の法線とデバイス接触点の法線とのなす角を返す
    /// </summary>
    /// <returns>2つの法線のなす角</returns>
    public double Set_AngleRVNormal()
    {
        double angle = AngleOf2Vector(hitNormalO, hitNormalF);

        return angle;
    }

    public double Set_AngleDeVNormal()
    {
        double angle = AngleOf2Vector(hitNormalO, tempEndD - tempTipD);

        return angle;
    }

    public double Set_AngleDeRNormal()
    {
        double angle = AngleOf2Vector(hitNormalF, tempEndD - tempTipD);

        return angle;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="num"></param>
    public void Set_FeedbackPattern(int num)
    {
        feedbackPattern = num;
        if(feedbackPattern == 0)
        {
            serialHandler.Write("-3;");
        }
        else
        {
            serialHandler.Write("-2;");
            serialHandler.Write("1023;");
        }
    }
}
