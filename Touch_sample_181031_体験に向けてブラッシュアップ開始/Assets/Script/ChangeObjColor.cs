using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeObjColor : MonoBehaviour {

    private GameObject myObj;
    public Material[] _material;
    private int i;

    // Use this for initialization
    void Start () {
        i = 1;
    }
	
	// Update is called once per frame
	void Update () {
        this.GetComponent<Renderer>().material = _material[i];
    }

    /// <summary>
    /// デバイスの色を伸縮可能か否かで変更
    /// </summary>
    /// <param name="flag">マテリアルの番号</param>
    public void Change_Color(int flag)
    {         
        i = flag;
    }
}
