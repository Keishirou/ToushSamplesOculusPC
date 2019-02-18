using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class PenetrationData : MonoBehaviour {
    bool start;                 // yama 181120 計測を開始するかどうかを判定する変数
    public string partcipantName;
    public int feedbackPattern;         // yama 181220 どのフィードバックで行うか指定（DeviceContoroll.csにもこれが反映される）
    public GameObject device;
    public GameObject objT, objP, objS, objG;   
    DeviceController dc;        // yama 181120 めり込み量を計算するスクリプトを格納
    Order_EvaluationObject oe;  // yama 181203 次の評価対象を選択するためのスクリプトを格納
    StreamWriter swIn;            // yama 181120 計測したデータを出力するためのCSVファイル
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();    // yama 181120 ストップウォッチ用の変数
    int eNum;       // yama 181203 実施する評価実験の番号
    bool contactP;  // yama 181203 評価実験1：ポイントオブジェクトに接触しているかどうか

    int prePattern = 4;

    // Use this for initialization
    void Start () {
        start = false;
        dc = device.GetComponent<DeviceController>();

        oe = this.GetComponent<Order_EvaluationObject>();
        eNum = oe.evaluationNum;
        //Debug.Log("PD_OK");
        //objT = oe.Set_EvaluationObject(eNum);
        //Debug.Log("PD_OK");
        //Get_EvaluationObj(objT);

        Debug.Log("PD_OK");
    }

    // Update is called once per frame
    void Update()
    {
        // yama 181224 DeviceContoroll.csに評価を行う提示パターンを受け渡し
        if (prePattern != feedbackPattern)
        {
            dc.Set_FeedbackPattern(feedbackPattern);
            prePattern = feedbackPattern;
        }

        // yama 181228 練習環境から切り替わった際，評価対象のオブジェクトはここで選択される
        if(objT == null)
        {         
            objT = oe.Set_EvaluationObject(eNum);

            if (objT.activeSelf)
            {
                Get_EvaluationObj(objT);
            }
        }

        // yama 190201 実験1実施時
        if (eNum == 1)
        {          
            // yama 190201 実験実施時はコントローラ，デバックや動作確認の際はキーボードで仮想物体の切り替え
            if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) || Input.GetKeyDown(KeyCode.Space))
            {
                if (contactP)
                {
                    if (!oe.Set_PointFlag())
                    {
                        #region ファイルに出力するための変数を集め，合体

                        string pene = dc.Set_PenetrationDistance();     // yama 181120 現在のめり込み量を取得
                        string peneY = dc.Set_PenetYDistance();         // yama 181120 現在のY軸方向のめり込み量を取得
                        Vector3 tipPos = dc.Set_tipD();                 // yama 190201 現フレームのデバイス先端座標
                        Vector3 hitPos = dc.Set_hitO();                 // yama 190201 現フレームのデバイスの方向ベクトルと仮想ぶった表面の交点（真値）
                        double sitaRV = dc.Set_AngleRVNormal();         // yama 190201 接触点の法線と床の法線とのなす角
                        double sitaDeV = dc.Set_AngleDeVNormal();       // yama 190201 接触点の法線とデバイスの方向ベクトルのなす角
                        double sitaDeR = dc.Set_AngleDeRNormal();       // yama 190201 床の法線とデバイスの方向ベクトルのなす角
                        string txt = pene + "," + peneY + ",,," + tipPos.x + "," + tipPos.y + "," + tipPos.z + ",,," + +hitPos.x + "," + hitPos.y + "," + hitPos.z + ",," + sitaRV + "," + sitaDeV + "," + sitaDeR;

                        Text_Save(txt);

                        #endregion

                        objP.GetComponent<ChangeObjColor>().Change_Color(0);

                        oe.Set_NextEvaluation();

                        timer.Stop();
                        timer.Reset();
                    }
                }
                else
                {
                    Debug.Log("ERROR：ポイントに対して接触していません．");
                }
            }

            // yama 190201 デバックや動作確認時，途中で終了するとそれまでのデータが保存されないので，強制終了用
            if (Input.GetKeyDown(KeyCode.M))
            {
                swIn.Flush();
                swIn.Close();
                Debug.Log("Save Finish");
            }

            contactP = false;
        }
        // yama 190201 実験2実施時
        else if (eNum == 2)
        {
            // yama 190201 実験実施時はコントローラ，デバックや動作確認の際はキーボードで仮想物体の切り替え
            if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) || Input.GetKeyDown(KeyCode.Space))
            {
                objT = oe.Set_EvaluationObject(eNum);

                if (objT.activeSelf)
                {
                    Debug.Log("Name: " + objT.name);
                    Get_EvaluationObj(objT);
                }
            }

            if (start)
            {
                #region ファイルに出力するための変数を集め，合体

                string pene = dc.Set_PenetrationDistance();     // yama 181120 現在のめり込み量を取得
                string peneY = dc.Set_PenetYDistance();     // yama 181120 現在のY軸方向のめり込み量を取得
                Vector3 tipPos = dc.Set_tipD();
                Vector3 hitPos = dc.Set_hitO();
                double sitaRV = dc.Set_AngleRVNormal();
                double sitaDeV = dc.Set_AngleDeVNormal();
                double sitaDeR = dc.Set_AngleDeRNormal();
                string txt = pene + "," + peneY + ",,," + tipPos.x + "," + tipPos.y + "," + tipPos.z + ",,," + +hitPos.x + "," + hitPos.y + "," + hitPos.z + ",," + sitaRV + "," + sitaDeV + "," + sitaDeR;

                Text_Save(txt);

                #endregion
            }
        }
    }
    
    /// <summary>
    /// めり込み量をテキストデータとして出力
    /// </summary>
    /// <param name="txt">めり込み量</param>
    public void Text_Save(string txt)
    {
        string str = txt;
        string time = ((float)timer.ElapsedMilliseconds / 1000).ToString();   // yama 181120 スタートしてからの経過時間

        if (eNum == 1)
        {
            str = objT.name.Replace("_Set", "") + ", " + str + ", " + time;         // yama 181203
        }
        if (eNum == 2)
        {
            
            str = time + ", " + str;         // yama 181120 時間とめり込み量を合わせた文字列を作成
            
        }
        swIn.WriteLine(str);      // yama 181120 CSVファイルに経過時間とめり込み量を出力
    }

    /// <summary>
    /// 計測の開始・終了の有無を判定する関数
    /// </summary>
    /// <param name="objName">スタート，もしくはゴールに設定したオブジェクト名</param>
    public void Get_CollisionData(string objName)
    {
        
        if (eNum == 1)
        {
            //Debug.Log("objName:" + objName);
            if (objName == objP.name)
            {
                contactP = true;
            }
        }
        else if (eNum == 2)
        {
            if (objName == objS.name && dc.Set_DisHitOtoTipD() < 10)
            {
                if (!start)
                {
                    start = true;

                    int angle = (int)objT.transform.eulerAngles.y;
                    swIn = new StreamWriter(Directory.GetCurrentDirectory() + "/Result/" + partcipantName + "/Penetration" + eNum + "Data_Pattern" + feedbackPattern + "_" + objT.name + "_angle" + angle + ".csv", true, Encoding.GetEncoding("Shift_JIS")); //true=追記 false=上書き
                    swIn.WriteLine("経過時間(s),めり込み量(mm),Y軸めり込み量(mm),,デバイス先端,X座標,Y座標,Z座標,,仮想物体との接触位置,X座標,Y座標,Z座標,,接触点の法線と床の法線とのなす角,接触点の法線とデバイスの方向ベクトルのなす角,床の法線とデバイスの方向ベクトルのなす角");      // yama 181120 CSVファイルに経過時間とめり込み量を出力

                    timer.Start();
                    objS.SetActive(false);
                    Debug.Log("Save Start");
                }
            }
            else if (objName == objG.name)
            {
                if (start)
                {
                    start = false;
                    swIn.Flush();
                    swIn.Close();
                    timer.Stop();
                    timer.Reset();
                    objG.SetActive(false);
                    Debug.Log("Save Finish");

                    oe.Set_NextEvaluation();
                }
            }
        }   
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    public void Get_EvaluationObj(GameObject obj)
    {
        if (eNum == 1)
        {   
            foreach (Transform child in obj.transform) // yama 181203 自分と同じ階層にあるポイントオブジェクトの取得
            {
                
                if (child.gameObject.name == "Point")
                {
                    objP = child.gameObject;
                    objP.GetComponent<ChangeObjColor>().Change_Color(1);
                }
            }

            if (swIn == null)
            {
                string path = Directory.GetCurrentDirectory() + "/Result/" + partcipantName;
                SafeCreateDirectory(path);
                swIn = new StreamWriter(Directory.GetCurrentDirectory() + "/Result/" + partcipantName + "/Penetration" + eNum + "Data_Pattern" + feedbackPattern + ".csv", false, Encoding.GetEncoding("Shift_JIS")); //true=追記 false=上書き
                swIn.WriteLine("1順目,めり込み量(mm),Y軸めり込み量(mm),,デバイス先端,X座標,Y座標,Z座標,,仮想物体との接触位置,X座標,Y座標,Z座標,,接触点の法線と床の法線とのなす角,接触点の法線とデバイスの方向ベクトルのなす角,床の法線とデバイスの方向ベクトルのなす角,時間");      // yama 181120 CSVファイルに経過時間とめり込み量を出力
                Debug.Log("Save_Start");
            }

            timer.Start();      // yama 181228 次の仮想物体が表示された瞬間から時間計測
        }
        else if (eNum == 2)
        {
            foreach (Transform child in obj.transform) // yama 181203 自分と同じ階層にあるスタート，ゴールオブジェクトの取得
            {
                if (child.gameObject.name == "Goal")
                {
                    objG = child.gameObject;
                    Debug.Log("Goal_Set");
                }
                else if (child.gameObject.name == "Start")
                {
                    objS = child.gameObject;
                    Debug.Log("Start_Set");
                }
            }
        }
    }

    /// <summary>
    /// 評価実験1：N順目の始まりを記録
    /// </summary>
    public void Write_NextRepeat(int n)
    {
        swIn.WriteLine((n + 1) + "順目,めり込み量(mm),Y軸めり込み量(mm),,デバイス先端,X座標,Y座標,Z座標,,仮想物体との接触位置,X座標,Y座標,Z座標,,接触角度");
    }

    /// <summary>
    /// 評価実験1：終了
    /// </summary>
    public void Finish_Evaluation1()
    {
        swIn.Flush();
        swIn.Close();

        Finish_ContactObj();
    }

    /// <summary>
    /// 仮想物体切り替わりの際に接触判定をいったん消す
    /// </summary>
    public void Finish_ContactObj()
    {
        dc.Check_DeviceContact(0);
    }

    /// <summary>
    /// 指定したパスにディレクトリが存在しない場合
    /// すべてのディレクトリとサブディレクトリを作成します
    /// </summary>
    public static DirectoryInfo SafeCreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return null;
        }
        return Directory.CreateDirectory(path);
    }
}
