using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse_position : MonoBehaviour {
    private Vector3 ObjectPos;              // yama 171004 オブジェクトの現在位置
    private Vector3 PreMousePos;            // yama 171004 オブジェクトをクリックした際のマウスの座標
    private Vector3 offset;                 // yama 171004 オブジェクトの座標とマウスの座標の差分

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float fWheel = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(0, 0, fWheel * 0.2f, Space.World);
    }

    void OnMouseDown()
    {
        // マウスカーソルは、スクリーン座標なので、
        // 対象のオブジェクトもスクリーン座標に変換してから計算する。

        // このオブジェクトの位置(transform.position)をスクリーン座標に変換。
        ObjectPos = Camera.main.WorldToScreenPoint(transform.position);
        PreMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ObjectPos.z));

        // ワールド座標上の、マウスカーソルと、対象の位置の差分。
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ObjectPos.z));
    }

    void OnMouseDrag()
    {
        //Debug.Log("x = " + Input.mousePosition.x + ", y = " + Input.mousePosition.y);
        Vector3 currentScreenPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ObjectPos.z));
        Vector3 currentPosition = currentScreenPoint + this.offset;
        currentPosition.z = transform.position.z;               // yama 171004 Z座標のみマウスホイールで移動させるため，変化させず
        transform.position = currentPosition;
    }
}
