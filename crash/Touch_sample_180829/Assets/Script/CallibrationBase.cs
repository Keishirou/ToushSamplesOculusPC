using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallibrationBase : MonoBehaviour {

    int callib_count = 0;           // yama 180201 Oculus Touchの座標をいくつ取得したか
    Vector3 rHand_pos, lHand_pos;   // yama 180201 左右のOculus Touchの座標 
    float offset = 0.2f;            // yama 180201 Oculus Touchを置いた際の、中心から接地面までの距離

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))  // yama 180201 右中指トリガーを押したか判定
        {
            if (callib_count == 0)
            {
                rHand_pos = transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
                //Debug.Log(rHand_pos);
                callib_count++;
                Set_Position();
            }
        }
        if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))  // yama 180201 左中指トリガーを押したか判定
        {
            if (callib_count == 1)
            {
                lHand_pos = transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch));
                //Debug.Log(lHand_pos);
                callib_count++;
                Set_Position();
            }
        }
    }

    /* 左右のOculus Touchの座標をもとに接地面の座標を算出
       Callibrationの順番は右->左の順で */
    void Set_Position()
    {
        if (callib_count == 2)
        {
            float base_y = (rHand_pos.y + lHand_pos.y) / 2 - offset;
            Debug.Log(base_y);
            this.transform.position = new Vector3(this.transform.position.x, base_y, this.transform.position.z);
        }
    }
}
