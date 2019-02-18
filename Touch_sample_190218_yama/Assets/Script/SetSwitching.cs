using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSwitching : MonoBehaviour {
    public GameObject[] set;
    int count = 0;

	// Use this for initialization
	void Start () {
        set[count].SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) && OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Space))
        {
            set[count].SetActive(false);
            count++;
            count = count % set.Length;
            set[count].SetActive(true);
            Debug.Log("OK");
        }
    }
}
