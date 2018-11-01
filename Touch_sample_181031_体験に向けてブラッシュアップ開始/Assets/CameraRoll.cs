using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRoll : MonoBehaviour {
    int temp = 45;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (temp > 0)
        {
            this.transform.Rotate(new Vector3(-1, 0, 0));
        }
        temp--;
	}
}
