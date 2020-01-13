using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 参考URL：http://baba-s.hatenablog.com/entry/2017/12/20/000200
/// </summary>
public class FPSCounter : MonoBehaviour {
    [SerializeField]
    private float m_updateInterval = 0.5f;

    private float m_accum;
    private int m_frames;
    private float m_timeleft;
    private float m_fps;

    void Awake()
    {
        Debug.Log("Awake");
        //Application.targetFrameRate = 90;     // yama 180913 ここはビルドしない限り，モニタのリフレッシュレートで固定される
    }

    // Use this for initialization
    void Start () {
        //Debug.Log("Start");
    }
	
	// Update is called once per frame
	void Update () {
        m_timeleft -= Time.deltaTime;
        m_accum += Time.timeScale / Time.deltaTime;
        m_frames++;

        if (0 < m_timeleft) return;

        m_fps = m_accum / m_frames;
        m_timeleft = m_updateInterval;
        m_accum = 0;
        m_frames = 0;
    }

    private void OnGUI()
    {
        GUILayout.Label("FPS: " + m_fps.ToString("f2"));
    }
}
