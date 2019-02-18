using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class Order_EvaluationObject : MonoBehaviour
{
    public int evaluationNum;       // yama 191205 評価実験＃
    int repeat;              // yama 181205 評価実験ごとの繰り返し回数
    public GameObject[] pointObj, strokeObj;
    GameObject startObj, goalObj;
    string goalName = "Goal";
    float[] yAngle = { -90, -45, 0, 45, 90 };
    int numP = 0;
    int numS = 0;
    int countRepeat = 0;
    bool nextStroke;
    PenetrationData pd;

    bool nextPoint;     // yama 181228 実験1のとき，次のポイントを表示を許可するかどうか

    struct OrderObj
    {
        public GameObject obj;
        public float angle;
    };
    OrderObj[] order;

    // Use this for initialization
    void Start()
    {
        pd = this.GetComponent<PenetrationData>();

        #region オブジェクトをランダムに並べ替え配列に格納

        // yama 181124 配列をランダムソート
        if (evaluationNum == 1)         // yama 181205 実験1では点で触るだけなのでソートした配列をそのまま使用
        {
            pointObj = pointObj.OrderBy(i => Guid.NewGuid()).ToArray();
            //pointObj[numP].SetActive(true);

            for (int i = 1; i < pointObj.Length; i++)
            {
                pointObj[i].SetActive(false);
            }
        }
        else if (evaluationNum == 2)    // yama 181205 実験1では角度を変更する必要があるため角度とオブジェクトを構造体に格納
        {
            order = new OrderObj[strokeObj.Length * yAngle.Length];

            for (int i = 0; i < strokeObj.Length; i++)
            {
                for (int j = 0; j < yAngle.Length; j++)
                {
                    order[i * yAngle.Length + j].obj = strokeObj[i];
                    order[i * yAngle.Length + j].angle = yAngle[j];
                }
            }

            order = order.OrderBy(i => Guid.NewGuid()).ToArray();   
            order[numS].obj.SetActive(true);
            order[numS].obj.transform.eulerAngles = new Vector3(0, order[numS].angle, 0);

            Debug.Log("Name: " + order[numS].obj.name + ", yAngle = " + order[numS].angle + ", numS = " + numS);

            foreach (Transform child in order[numS].obj.transform)      // yama 181205 ゴールオブジェクトを検索・格納
            {    
                if (goalName == child.gameObject.name)
                {
                    goalObj = child.gameObject;
                    break;
                }
            }
        }

        #endregion

        nextPoint = true;       // yama 181228 最初のオブジェクト表示を許可
        nextStroke = false;

        if(evaluationNum == 1)
        {
            repeat = 4;
        }
        else if(evaluationNum == 2)
        {
            repeat = 1;
        }

        Debug.Log("OE_OK");
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) || Input.GetKeyDown(KeyCode.Space))
        { 
            if (evaluationNum == 2)
            {
                if (nextStroke)
                //if (!goalObj.activeSelf)    // yama 181124 ゴールまでたどり着いているか（タスクが達成されているか）
                {
                    NextOrderStroke();
                    pd.Get_EvaluationObj(order[numS].obj);
                    nextStroke = false;
                }      
            }
        }
    }

    /// <summary>
    /// ポイント評価時の次のオブジェクトを選択
    /// </summary>
    void NextOrderPoint()
    {
        pointObj[numP].SetActive(false);
        pd.Finish_ContactObj();             // yama 181224 接触判定をいったん取り消す（消える前の仮想物体の接触判定が残ってしまうから）

        numP++;
        if (numP == pointObj.Length)
        {
            pointObj = pointObj.OrderBy(i => Guid.NewGuid()).ToArray();
            numP = 0;

            countRepeat++;

            if (countRepeat <= repeat)
            {
                pd.Write_NextRepeat(countRepeat);
            }
            else
            {
                pd.Finish_Evaluation1();
            }
        }

        if (countRepeat <= repeat)
        {
            pointObj[numP].SetActive(true);
        }
    }

    /// <summary>
    /// ストローク評価時の次のオブジェクトを選択
    /// </summary>
    void NextOrderStroke()                  
    {
        order[numS].obj.SetActive(false);
        pd.Finish_ContactObj();             // yama 181224 接触判定をいったん取り消す（消える前の仮想物体の接触判定が残ってしまうから）     

        numS++;

        if(numS == strokeObj.Length * yAngle.Length)
        {
            order = order.OrderBy(i => Guid.NewGuid()).ToArray();
            numS = 0;

            countRepeat++;

            //if (countRepeat <= repeat)
            //{
            //    pd.Write_NextRepeat(countRepeat);
            //    Debug.Log("OK");
            //}
        }

        if (countRepeat <= repeat)
        {
            order[numS].obj.SetActive(true);
            order[numS].obj.transform.eulerAngles = new Vector3(0, order[numS].angle, 0);

            Debug.Log("Name: " + order[numS].obj.name + ", yAngle = " + order[numS].angle + ", numS = " + numS);

            foreach (Transform child in order[numS].obj.transform)
            {
                child.gameObject.SetActive(true);
                if (goalName == child.gameObject.name)
                {
                    goalObj = child.gameObject;
                }
            }
        }
    }

    /// <summary>
    /// 現在Activeになっている評価対象を返す
    /// </summary>
    /// <param name="num">実験番号</param>
    /// <returns>評価対象</returns>
    public GameObject Set_EvaluationObject(int num)
    {
        if(num == 1)
        {
            return pointObj[numP];
        }
        else if(num == 2)
        {
            return order[numS].obj;
        }

        return null;
    }

    /// <summary>
    /// 次の評価対象への移行or移行の許可
    /// </summary>
    public void Set_NextEvaluation()
    {
        if (evaluationNum == 1)
        {
            nextPoint = true;
        }
        else if(evaluationNum == 2)
        {
            nextStroke = true;
        }
    }

    /// <summary>
    /// 評価実験1：次の仮想物体を表示
    /// </summary>
    public void Touch_Switxh()
    {
        if (countRepeat <= repeat)
        {
            if (numP == 0 && !pointObj[numP].activeSelf)
            {
                pointObj[numP].SetActive(true);
                pd.Get_EvaluationObj(pointObj[numP]);

                nextPoint = false;
            }
            else
            {
                if (nextPoint)
                {
                    NextOrderPoint();

                    pd.objT = Set_EvaluationObject(evaluationNum);
                    pd.Get_EvaluationObj(pd.objT);

                    nextPoint = false;
                }
            }
        }
    }

    /// <summary>
    /// スイッチの色を仮想物体の表示を許可するかどうかで切り替えるための変数を返す
    /// </summary>
    /// <returns>仮想物体の表示を切り替えるか否かを判定する変数</returns>
    public bool Set_PointFlag()
    {
        return nextPoint;
    }
}
