using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube_position : MonoBehaviour {
    Vector3 startPos;

    // Use this for initialization
    void Start () {
        startPos = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = startPos;
	}
}
