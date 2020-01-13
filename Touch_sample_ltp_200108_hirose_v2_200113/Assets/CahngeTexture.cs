using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CahngeTexture : MonoBehaviour {

    public Material[] _material;           // 割り当てるマテリアル. 
    private int i;
    
    // Use this for initialization
    void Start () {
        i = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            i++;
            //if (i == 3)
            //{
            //    i = 0;
            //}

            this.GetComponent<Renderer>().material = _material[i%_material.Length];
        }
    }
}
