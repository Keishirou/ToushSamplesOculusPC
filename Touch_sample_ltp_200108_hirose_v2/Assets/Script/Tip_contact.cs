using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tip_contact : MonoBehaviour {
    Vector3 contactPos;
    int contactFlag;

    //string contactObj = "Ball";         // yama 171227 接触する仮想物体の名前（これコメントアウトして切り替えるだけでいい）
    string contactObj = "Cube";
    string objTag = "Haptic";       // yama 180822 触感提示を行いたい仮想物体（のTag）

    DeviceController dc;

    // Use this for initialization
    void Start () {
        contactFlag = 0;

        dc = this.GetComponent<DeviceController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)      // yama 180731 デバイス先端が指定したオブジェクトに接触した瞬間
    {
        // yama 180914 関数に引き渡す引数を仮想物体接触点の法線に変更
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.tag == objTag)
            {
                if (contactFlag != 1)
                {
                    contactFlag = 1;
                    dc.Check_DeviceContact(contactFlag);
                }
            }
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        // yama 180914 関数に引き渡す引数を仮想物体接触点の法線に変更
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.tag == objTag)
            {
                if (contactFlag != 1)
                {
                    contactFlag = 1;

                    dc.Check_DeviceContact(contactFlag);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == objTag)        // yama 180822 デバイス先端から離れた仮想物体が設定したTagと一致しているか
        {
            if (contactFlag != 0)
            {
                contactFlag = 0;

                dc.Check_DeviceContact(contactFlag);
            }
        }
    }

    void RVContactDistance(Vector3 realContactPos)
    {
        if (contactFlag == 1)
        {
            float RVdistance = Vector3.Distance(contactPos, realContactPos);
        }
    }
}
