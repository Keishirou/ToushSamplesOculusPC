using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order_PracticeObject : MonoBehaviour {
    public GameObject device;
    public GameObject evalSet;
    DeviceController dc;
    PenetrationData pd;

    public GameObject[] practiceObj;

    int num = 0;
    int prePattern = 4;

    // Use this for initialization
    void Start () {
        InitPracticeObj();
        practiceObj[num].SetActive(true);

        dc = device.GetComponent<DeviceController>();
        pd = evalSet.GetComponent<PenetrationData>();
    }

    // Update is called once per frame
    void Update()
    {
        // yama 181228 評価を行う提示パターンをPenetrationData.csと同期させる
        if (prePattern != pd.feedbackPattern)
        {
            dc.Set_FeedbackPattern(pd.feedbackPattern);
            prePattern = pd.feedbackPattern;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) || Input.GetKeyDown(KeyCode.Space))
        {
            NextOrderObj();
        }
    }

    /// <summary>
    /// 練習用仮想物体の初期化(すべてFalseに)
    /// </summary>
    void InitPracticeObj()
    {
        for(int i = 0; i < practiceObj.Length; i++)
        {
            practiceObj[i].SetActive(false);
        }
    }

    /// <summary>
    /// 次の仮想物体の表示
    /// </summary>
    void NextOrderObj()
    {
        practiceObj[num].SetActive(false);
        Finish_ContactObj();

        num++;

        if (num == practiceObj.Length)
        {
            num = num % practiceObj.Length;
        }

        practiceObj[num].SetActive(true);
    }

    /// <summary>
    /// 仮想物体切り替わりの際に接触判定をいったん消す
    /// </summary>
    public void Finish_ContactObj()
    {
        dc.Check_DeviceContact(0);
    }
}
