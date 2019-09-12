using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider_contact : MonoBehaviour {
    GameObject device;

    string baseObj = "Floor";           // yama 171227 基準となる地面
    string contactObj = "Ball";         // yama 171227 接触する仮想物体の名前
    //string contactObj = "Cube";         // yama 171227 接触する仮想物体の名前

    // Use this for initialization
    void Start () {
        device = GameObject.Find("Device");
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    private void OnCollisionEnter(Collision other)
    {
        int i = 0;

        foreach (ContactPoint contact in other.contacts)
        {
            /*
            if (contact.thisCollider.name == "Floor")
            {
                Debug.Log("Floor sliderContact: x = " + contact.point.x + ", y = " + contact.point.y + ", z = " + contact.point.z);
            }
            else if (contact.thisCollider.name == "Ball")
            {
                Debug.Log("Ball sliderContact: x = " + contact.point.x + ", y = " + contact.point.y + ", z = " + contact.point.z);
            }
            */
            //Debug.Log(contact.otherCollider.name);
            if(contact.otherCollider.name == "Ball")
            {
                Debug.Log(contact.otherCollider.name + " sliderContact: x = " + contact.point.x + ", y = " + contact.point.y + ", z = " + contact.point.z);
            }
        }
        /*
        if (other.gameObject.name == baseObj)
        {
            Vector3 hitPos = other.ClosestPointOnBounds(this.transform.position);
            //Debug.Log("floor sliderHit: x = " + hitPos.x + ", y = " + hitPos.y + ", z = " + hitPos.z);

            //device.SendMessage("RVContactDistance", hitPos);      // yama 171227 オブジェクト"Device"に取り付けたスクリプト"Tip_contact"がactiveのときのみ
            device.SendMessage("Get_SliderContact", hitPos);
        }
        else if(other.gameObject.name == contactObj)
        {
            Vector3 hitPos = other.ClosestPointOnBounds(this.transform.position);

            Debug.Log("obj sliderHit: x = " + hitPos.x + ", y = " + hitPos.y + ", z = " + hitPos.z);

            device.SendMessage("Get_SliderOverCon", hitPos);        // yama 171227 オブジェクトに接触（めり込み）位置の送信
        }
        */
    }
    
    private void OnCollisionStay(Collision other)
    {
        int i = 0;

        foreach (ContactPoint contact in other.contacts)
        {
            /*
            if (contact.thisCollider.name == "Floor")
            {
                Debug.Log("Floor sliderContact: x = " + contact.point.x + ", y = " + contact.point.y + ", z = " + contact.point.z);
            }
            else if (contact.thisCollider.name == "Ball")
            {
                Debug.Log("Ball sliderContact: x = " + contact.point.x + ", y = " + contact.point.y + ", z = " + contact.point.z);
            }
            */
            //Debug.Log(contact.otherCollider.name);
            if (contact.otherCollider.name == "Ball")
            {
                Debug.Log(contact.otherCollider.name + " sliderContact: x = " + contact.point.x + ", y = " + contact.point.y + ", z = " + contact.point.z);
            }
        }
        /*
        if (other.gameObject.name == baseObj)
        {
            Vector3 hitPos = other.ClosestPointOnBounds(this.transform.position);
            //Debug.Log("floor sliderHit: x = " + hitPos.x + ", y = " + hitPos.y + ", z = " + hitPos.z);

            //device.SendMessage("RVContactDistance", hitPos);      // yama 171227 オブジェクト"Device"に取り付けたスクリプト"Tip_contact"がactiveのときのみ
            device.SendMessage("Get_SliderContact", hitPos);
        }
        else if (other.gameObject.name == contactObj)
        {
            Vector3 hitPos = other.ClosestPointOnBounds(this.transform.position);

            Debug.Log("obj sliderHit: x = " + hitPos.x + ", y = " + hitPos.y + ", z = " + hitPos.z);

            device.SendMessage("Get_SliderOverCon", hitPos);        // yama 171227 オブジェクトに接触（めり込み）位置の送信
        }
        */
    }

}
