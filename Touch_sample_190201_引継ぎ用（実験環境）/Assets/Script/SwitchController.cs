using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour {
    public GameObject s;
    Order_EvaluationObject oe;
    ChangeObjColor cc;

	// Use this for initialization
	void Start () {
        oe = this.GetComponent<Order_EvaluationObject>();
        cc = s.gameObject.GetComponent<ChangeObjColor>();

        if(oe.evaluationNum == 1)
        {
            s.SetActive(true);
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (oe.evaluationNum == 1)
        {
            s.SetActive(true);
        }
        else
        {
            s.SetActive(false);
        }

        if (oe.Set_PointFlag())
        {
            cc.Change_Color(1);
        }
        else
        {
            cc.Change_Color(0);
        }
	}
}
