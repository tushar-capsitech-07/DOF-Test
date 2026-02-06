using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;

public class DiscoveryHandler : MonoBehaviour
{
    private UdpClient udpClient;
    private const int discoveryPort = 47777;
    public string hostIP = "";

    public void StartBroadcasting(string ip)
    {
        hostIP = ip;
        StartCoroutine(BroadcastLoop());
    }

    private IEnumerator BroadcastLoop()
    {
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);

        while (true)
        {
            byte[] data = Encoding.UTF8.GetBytes("GAME_HOST_AT:" + hostIP);
            udpClient.Send(data, data.Length, endPoint);
            yield return new WaitForSeconds(2f);
        }
    }

    public void StartListening(Action<string> onGameFound)
    {
        StartCoroutine(ListenLoop(onGameFound));
    }

    private IEnumerator ListenLoop(Action<string> onGameFound)
    {
        UdpClient listener = new UdpClient(discoveryPort);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, discoveryPort);

        while (true)
        {
            if (listener.Available > 0)
            {
                byte[] data = listener.Receive(ref endPoint);
                string message = Encoding.UTF8.GetString(data);
                if (message.StartsWith("GAME_HOST_AT:"))
                {
                    string foundIP = message.Split(':')[1];
                    onGameFound?.Invoke(foundIP);
                    break;
                }
            }
            yield return null;
        }
        listener.Close();
    }
}