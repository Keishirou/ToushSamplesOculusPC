using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smaple_Haptic : MonoBehaviour {

    public AudioSource audioSource;
    public OVRHapticsClip hapticClip;

    private static Smaple_Haptic thisObj;

    // Use this for initialization
    void Start () {
        thisObj = this;
        hapticClip = new OVRHapticsClip(audioSource.clip);
    }
	
	// Update is called once per frame
	void Update () {
       
    }

    public static void PlaySE()
    {
        if (thisObj != null)
        {
            thisObj.audioSource.Play();
            OVRHaptics.RightChannel.Mix(thisObj.hapticClip);
            OVRHaptics.LeftChannel.Mix(thisObj.hapticClip);
        }
    }
}
