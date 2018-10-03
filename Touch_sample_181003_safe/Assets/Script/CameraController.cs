using UnityEngine;
using UnityEngine.VR;
using System.Collections;

/// <summary>
/// サブカメラ位置の固定（動画撮影時などに使用）
/// SubCameraのInspecotr内のTargetEyeをNone(Main Display)にしておくこと
/// 参考URL：http://monopocket.net/blog/vr/unity-hmd-position/
/// </summary>
public class CameraController : MonoBehaviour {
    Vector3 basePos;
    Quaternion baseRot;
    public bool mirror;

    void Start()
    {
        basePos = this.transform.position;
        baseRot = this.transform.rotation;

        VRmirror(mirror);              
    }

    void Update () {
        this.transform.position = basePos;
        this.transform.rotation = baseRot;

        VRmirror(mirror);
    }

    void VRmirror(bool mirror)
    {
        if (mirror) // yama 180
        {
            this.GetComponent<Camera>().targetDisplay = 0;
            UnityEngine.VR.VRSettings.showDeviceView = false;
        }
        else
        {
            this.GetComponent<Camera>().targetDisplay = 1;
            UnityEngine.VR.VRSettings.showDeviceView = true;
        }
    }

}