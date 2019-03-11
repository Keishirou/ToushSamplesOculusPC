using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CheckCollider : MonoBehaviour {
    string evaluationSet = "Evaluation_Set";
    GameObject targetObj;
    PenetrationData pd;
    DeviceController dc;
    Order_EvaluationObject oe;
    GameObject eSet;

    // Use this for initialization
    void Start () {
        if (GameObject.Find("Evaluation_Set1"))
        {
            eSet = GameObject.Find("Evaluation_Set1");

        }else if (GameObject.Find("Evaluation_Set2"))
        {
            eSet = GameObject.Find("Evaluation_Set2");
        }
        else
        {

        }

        string name = this.gameObject.transform.parent.name.Replace("_Set", "");    // yama 181203 接触対象のオブジェクト名を選択（～_Setの～に当たる名前にする必要あり）

        foreach (Transform child in this.gameObject.transform.parent.transform) // yama 181203 自分と同じ階層にあるターゲットオブジェクトの取得
        {
            if (child.gameObject.name == name)
            {
                targetObj = child.gameObject;
                //Debug.Log("Target_Set");
            }
        }

        if(targetObj == null)
        {
            Debug.Log("ERROR：接触対象のオブジェクトが選択されていません．");
        }

        pd = eSet.GetComponent<PenetrationData>();
        oe = eSet.GetComponent<Order_EvaluationObject>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)  
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.name == "Device")
            {
                //dc = contact.otherCollider.gameObject.GetComponent<DeviceController>();
                //if (dc.DtoO)
                //{
                //    pd.Get_CollisionData(this.name);
                //}
                //Invoke("oe.Touch_Switxh",5.0f);
                oe.Touch_Switxh(); //次の仮想物体を表示

                //3.5秒後に実行する
                //StartCoroutine(DelayMethod(3.5f, oe.Touch_Switxh));

                pd.Get_CollisionData(this.name);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.name == "Device")
            {
                //dc = contact.otherCollider.gameObject.GetComponent<DeviceController>();
                //if (dc.DtoO)
                //{
                //    pd.Get_CollisionData(this.name);
                //}
                //oe.Touch_Switxh(); //次の仮想物体を表示
                pd.Get_CollisionData(this.name);
            }
        }
    }

    /// <summary>
    /// 渡された処理を指定時間後に実行する
    /// </summary>
    /// <param name="waitTime">遅延時間[ミリ秒]</param>
    /// <param name="action">実行したい処理</param>
    /// <returns></returns>
    private IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
}
