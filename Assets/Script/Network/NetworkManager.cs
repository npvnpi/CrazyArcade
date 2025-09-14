using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _recvThread;
    private bool _running;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Connect(string host, int port)
    {
        if (_running) return;

        _client = new TcpClient();
        _client.Connect(host, port);
        _stream = _client.GetStream();

        _running = true;
        _recvThread = new Thread(ReceiveLoop);
        _recvThread.Start();

        Debug.Log("서버 연결됨!");
    }

    public void Send(string text)
    {
        if (!_running) return;
        byte[] data = Encoding.UTF8.GetBytes(text);
        _stream.Write(data, 0, data.Length);
    }

    private void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        while (_running)
        {
            try
            {
                int read = _stream.Read(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, read);
                    Debug.Log($"받음: {msg}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RECV-ERR] {ex}");
                break;
            }
        }
    }

    public void Disconnect()
    {
        _running = false;
        _stream?.Close();
        _client?.Close();
        _recvThread?.Join();
    }
}
