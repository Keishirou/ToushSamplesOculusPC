using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tip_contact : MonoBehaviour {
    Vector3 contactPos;
    int contactFlag;

    //string contactObj = "Ball";         // yama 171227 接触する仮想物体の名前（これコメントアウトして切り替えるだけでいい）
    string contactObj = "Cube";

    // Use this for initialization
    void Start () {
        contactFlag = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)      // yama 180731 デバイス先端が指定したオブジェクトに接触した瞬間
    {
        if (collision.gameObject.name == contactObj)
        {
            contactFlag = 1;

            //Debug.Log("contactFlag = " + contactFlag);

            SendMessage("Check_DeviceContact", contactFlag);
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.name == "Floor")
            {
                //Debug.Log("Floor Contact: x = " + contact.point.x + ", y = " + contact.point.y + ", z = " + contact.point.z);
            }
            else if (contact.otherCollider.name == "Ball")
            {
                //Debug.Log("Ball Contact: x = " + contact.point.x + ", y = " + contact.point.y + ", z = " + contact.point.z);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == contactObj)        // yama 180731 デバイス先端が指定したオブジェクトに接触した瞬間
        {
            contactFlag = 0;

            //Debug.Log("contactFlag = " + contactFlag);

            SendMessage("Check_DeviceContact", contactFlag);
        }
    }

    void RVContactDistance(Vector3 realContactPos)
    {
        if (contactFlag == 1)
        {
            float RVdistance = Vector3.Distance(contactPos, realContactPos);
            //Debug.Log("Distance = " + RVdistance);
            //Debug.Log("Contact: x = " + contactPos.x + ", y = " + contactPos.y + ", z = " + contactPos.z);
            //Debug.Log("RealContact: x = " + realContactPos.x + ", y = " + realContactPos.y + ", z = " + realContactPos.z);

            //SendMessage("GetSliderLength", RVdistance);       // yama 171227 ここはスクリプト"DeviceController"に合わせて解除
        }
    }
}
