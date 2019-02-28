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
            if(oe.evaluationNum == 1)
            {
                ModeSelect.mode = 3; //緑色の立方体に触れてください
            }
            else
            {
                if (ModeSelect.mode < 7)
                {
                    ModeSelect.mode = 6; //黒い線をなぞってみてください\n緑の立方体が始点です

                }
                // Debug.Log("6");
            }
        }
        else
        {
            cc.Change_Color(0);
            if (oe.evaluationNum == 1)
            {
                ModeSelect.mode = 4; //赤い点に触れてみてください
            }
        }
	}
}
