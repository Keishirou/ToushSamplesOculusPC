using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_Switch : MonoBehaviour {
    GameObject parent;
    Order_EvaluationObject oe;

	// Use this for initialization
	void Start () {
        parent = this.gameObject.transform.root.gameObject;
        oe = parent.GetComponent<Order_EvaluationObject>();
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
                oe.Touch_Switxh();

            }
        }
    }
}
