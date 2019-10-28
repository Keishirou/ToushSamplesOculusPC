using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMove : MonoBehaviour {
    public GameObject End;          //デバイスの末端についているSphere_End
    public GameObject Device_Tip;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //transform.Rotate((Device_Tip.transform.position - End.transform.position - transform.position) * Time.deltaTime, Space.World);
        // transformを取得
        Transform myTransform = this.transform;

        // ワールド座標基準で、現在の回転量へ加算する
        myTransform.Rotate(1.0f, 0.0f, 0.0f);

        //transform.RotateAround(new Vector3(0, 1, 0), transform.up, 10 * Time.deltaTime);
    }
}
