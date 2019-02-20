using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeSelect : MonoBehaviour {

    public GameObject messageText;
    public GameObject startObj;
    public static int mode;
    public static string modeName;

    // Use this for initialization
    void Start () {
        mode = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (mode == 1)
        {
            messageText.GetComponent<TextMesh>().text = modeName+"でよろしいですか？";
            startObj.SetActive(true);
        }else if(mode == 2)
        {
            messageText.GetComponent<TextMesh>().text = modeName;
            this.gameObject.SetActive(false);
        }
	}

    
}
