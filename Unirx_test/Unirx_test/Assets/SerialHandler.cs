using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using UniRx;

public class SerialHandler : MonoBehaviour
{
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived;

    public string portName = "COM3"; // ポート名(Macだと/dev/tty.usbmodem1421など)
    public int baudRate = 38400;  // ボーレート(Arduinoに記述したものに合わせる)

    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;

    private string message_;
    private bool isNewMessageReceived_ = false;

    bool isLoop = true;

    void Awake()
    {
        //Open();
    }

    void Start()
    {
        this.serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);

        try
        {
            this.serialPort_.Open();
            Scheduler.ThreadPool.Schedule(() => ReadData()).AddTo(this);
        }
        catch (Exception e)
        {
            Debug.Log("can not open serial port");
        }
    }

    void Update()
    {
        //if (isNewMessageReceived_)
        //{
        //    OnDataReceived(message_);
        //}
        //isNewMessageReceived_ = false;
    }

    void OnDestroy()
    {
        //Close();
        this.isLoop = false;
        this.serialPort_.Close();
    }

    private void Open()
    {
        serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        //または
        //serialPort_ = new SerialPort(portName, baudRate);
        serialPort_.Open();

        isRunning_ = true;

        thread_ = new Thread(Read);
        thread_.Start();
    }

    private void Close()
    {
        isNewMessageReceived_ = false;
        isRunning_ = false;

        if (thread_ != null && thread_.IsAlive)
        {
            thread_.Join();
        }

        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }

    private void Read()
    {
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                message_ = serialPort_.ReadLine();
                isNewMessageReceived_ = true;


                //Debug.Log(message_);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

    public void ReadData()
    {
        while (this.isLoop)
        {
            string message = this.serialPort_.ReadLine();
            Debug.Log(message);
        }
    }

    public void Write(string message)
    {
        try
        {
            serialPort_.Write(message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

}