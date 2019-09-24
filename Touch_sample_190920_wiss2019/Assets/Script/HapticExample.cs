using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticExample : MonoBehaviour {

    //public AudioClip audioClip;
    OVRHapticsClip hapticsClip;

    private AudioSource audioSource;
    private byte[] samples = new byte[40];

    // Use this for initialization
    void Start () {
        //hapticsClip = new OVRHapticsClip(audioClip);

        
        for (int i = 0; i < samples.Length; i++)
        {
            
            if (i % 3 == 0)
            {
                samples[i] = 255;
            }else
            {
                samples[i] = 128;
            }  
        }
        
        hapticsClip = new OVRHapticsClip(samples, samples.Length);
    }

    // Update is called once per frame
    void Update () {
        
    }

    /// <summary>
    /// コントローラを振動させるor振動を停止
    /// </summary>
    /// <param name="num">振動させるかどうか</param>
    public void Play_HapticClip(int num)
    {
        if(num == 1)
        {
            OVRHaptics.RightChannel.Preempt(hapticsClip);
        }
        else if(num == 0)
        {
            OVRHaptics.RightChannel.Clear();
        }
    }
}
