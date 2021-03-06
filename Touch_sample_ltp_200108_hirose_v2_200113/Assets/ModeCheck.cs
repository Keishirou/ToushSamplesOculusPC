﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeCheck : MonoBehaviour {

    public string modeName;
    public int modeNum;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.name == "Device")
            {
                //Debug.Log("Device");
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                ModeSelect.mode = modeNum;
                ModeSelect.modeName = modeName;
            }
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        gameObject.GetComponent<Renderer>().material.color = Color.white;
    }
}
