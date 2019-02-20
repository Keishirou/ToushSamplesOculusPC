using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_Switch : MonoBehaviour {
    GameObject parent;
    Order_EvaluationObject oe;
    private int count;

	// Use this for initialization
	void Start () {
        parent = this.gameObject.transform.root.gameObject;
        oe = parent.GetComponent<Order_EvaluationObject>();
        count = 0;
        //ModeSelect.mode = 3;
        //ModeSelect.modeName = "緑色の立方体に触れ，出現した立体上の赤い球に触れてみてください";
	}
	
	// Update is called once per frame
	void Update () {
        if (count == 6)
        {
            //oe.evaluationNum = 2;
            //ModeSelect.mode = 5;
        }
	}

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.name == "Device")
            {
                oe.Touch_Switxh();
                count++;
            }
        }
    }

}
