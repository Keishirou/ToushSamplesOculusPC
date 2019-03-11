using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeSelect : MonoBehaviour {

    public GameObject messageText;
    public GameObject startObj;
    public GameObject pointDemoObj;
    public GameObject strokeDemoObj;
    public GameObject[] testObj;
    public static int mode;
    public static string modeName;

    // Use this for initialization
    void Start () {
        mode = 1; //初期化

        //1点
        pointDemoObj.GetComponent<Order_EvaluationObject>().enabled = false;
        pointDemoObj.GetComponent<PenetrationData>().enabled = false;
        //pointDemoObj.GetComponent<SwitchController>().enabled = false;
        pointDemoObj.SetActive(false);

        //はじめ
        startObj.SetActive(false);

        //なぞり
        strokeDemoObj.GetComponent<Order_EvaluationObject>().enabled = false;
        strokeDemoObj.GetComponent<PenetrationData>().enabled = false;
        //strokeDemoObj.GetComponent<SwitchController>().enabled = false;
        strokeDemoObj.SetActive(false);
        foreach (GameObject temp in testObj)
        {
            temp.SetActive(false);
            //temp.GetComponent<ModeCheck>().enabled = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (mode == 1)
        {
            //messageText.GetComponent<TextMesh>().text = modeName+"でよろしいですか？";
            //messageText.GetComponent<TextMesh>().text = "STARTボタンを押してください";
            messageText.GetComponent<TextMesh>().text = "Please touch the START button!";
            startObj.SetActive(true);
        }else if(mode == 2)
        {
            messageText.GetComponent<TextMesh>().text = modeName;
            pointDemoObj.SetActive(true);
            pointDemoObj.GetComponent<Order_EvaluationObject>().enabled = true;
            pointDemoObj.GetComponent<PenetrationData>().enabled = true;
            //pointDemoObj.GetComponent<SwitchController>().enabled = true;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            //this.gameObject.SetActive(false);
            //mode = 3;
        }else if(mode == 3)
        {
            //messageText.GetComponent<TextMesh>().text = "緑色の立方体に触れてください\n残り"+(3 - Order_EvaluationObject.pCount)+"/3 回";
            messageText.GetComponent<TextMesh>().text = "Please touch the GREEN CUBE!\n" + (3 - Order_EvaluationObject.pCount) + "/3 times left!";

        }
        else if(mode == 4)
        {
            messageText.GetComponent<TextMesh>().text = "Please touch the RED BALL!\n" + (3 - Order_EvaluationObject.pCount)+ "/3 times left!";

        }else if(mode == 5)
        {
            pointDemoObj.GetComponent<Order_EvaluationObject>().enabled = false;
            pointDemoObj.GetComponent<PenetrationData>().enabled = false;
            //pointDemoObj.GetComponent<SwitchController>().enabled = false;
            pointDemoObj.SetActive(false);
            strokeDemoObj.SetActive(true);
            strokeDemoObj.GetComponent<Order_EvaluationObject>().enabled = true;
            strokeDemoObj.GetComponent<PenetrationData>().enabled = true;
            //strokeDemoObj.GetComponent<SwitchController>().enabled = true;

            //messageText.GetComponent<TextMesh>().text = "黒い線をなぞってみてください\n緑の立方体が始点です";
        }else if(mode == 6)
        {
            //messageText.GetComponent<TextMesh>().text = "黒い線をなぞってみてください\n緑の立方体が始点です\n残り" + (3 - Order_EvaluationObject.sCount) + "/3 回";
            messageText.GetComponent<TextMesh>().text = "Let's trace the BLACK LINE!\nThe starting point is the GREEN CUBE.\n" + (3 - Order_EvaluationObject.sCount) + "/3 times left!";

        }
        else if (mode == 7)
        {
            strokeDemoObj.GetComponent<Order_EvaluationObject>().enabled = false;
            strokeDemoObj.GetComponent<PenetrationData>().enabled = false;
            //strokeDemoObj.GetComponent<SwitchController>().enabled = false;
            strokeDemoObj.SetActive(false);
            messageText.GetComponent<TextMesh>().text = "Finally, Please touch freely!!";

            foreach(GameObject temp in testObj)
            {
                temp.SetActive(true);
                //temp.GetComponent<ModeCheck>().enabled = false;
            }

        }
    }

    
}
