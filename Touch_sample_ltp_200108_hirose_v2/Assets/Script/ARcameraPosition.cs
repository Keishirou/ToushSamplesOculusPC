using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARcameraPosition : MonoBehaviour {
    public GameObject ARcamera;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //this.transform.localPosition = Vector3.zero;
        //this.transform.localRotation = Quaternion.identity;

        this.transform.localPosition = ARcamera.transform.position;
        this.transform.localRotation = Quaternion.identity;
    }
}
