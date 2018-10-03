using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticExample : MonoBehaviour {

    public AudioClip audioClip;
    OVRHapticsClip hapticsClip;

    private AudioSource audioSource;
    private byte[] samples = new byte[40];

    // Use this for initialization
    void Start () {
        hapticsClip = new OVRHapticsClip(audioClip);

        /*
        for (int i = 0; i < samples.Length; i++)
        {
            
            if (i % 3 == 0)
            {
                samples[i] = 192;
            }else
            {
                samples[i] = 128;
            }  
        }
        */
        //hapticsClip = new OVRHapticsClip(samples, samples.Length);
    }
	
	// Update is called once per frame
	void Update () {
        // Aボタンで振動
        
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            OVRHaptics.RightChannel.Mix(hapticsClip);

            audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        
    }

    void OnCollisionEnter(Collision collision)
    {
        
            OVRHaptics.RightChannel.Mix(hapticsClip);

            audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.Play();
        
    }
}
