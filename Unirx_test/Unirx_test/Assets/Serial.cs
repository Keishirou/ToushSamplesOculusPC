using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO.Ports;
using UnityEngine;
using UniRx;

public class Serial : MonoBehaviour
{
    private int temp;
    public string portName;
    public int baurate;

    SerialPort serial;
    bool isLoop = true;

    void Start()
    {
        this.serial = new SerialPort(portName, baurate, Parity.None, 8, StopBits.One);

        try
        {
            this.serial.Open();
            Scheduler.ThreadPool.Schedule(() => ReadData()).AddTo(this);
            //Scheduler.ThreadPool.Schedule(() => Write("1")).AddTo(this);
        }
        catch (Exception e)
        {
            Debug.Log("can not open serial port");
        }
    }

   void Update()
    {
        //Write("1");
    }

    public void ReadData()
    {
        while (this.isLoop)
        {
            string message = this.serial.ReadLine();
            Debug.Log(message);
            /*テスト*/
            temp = int.Parse(message)+1;
        }
    }

    void OnDestroy()
    {
        this.isLoop = false;
        this.serial.Close();
    }
    public void Write(string a)
    {
        string message;
        try
        {
            message = temp.ToString() + ";";
            serial.Write(message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}