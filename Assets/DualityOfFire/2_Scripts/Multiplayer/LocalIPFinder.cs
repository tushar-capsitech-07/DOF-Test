using System.Net.NetworkInformation;
using System.Net.Sockets;

public static class LocalIPFinder
{
    public static string GetLocalIPv4()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ipAddr = ip.Address.ToString();
                        if (ipAddr.StartsWith("192.") || ipAddr.StartsWith("172.") || ipAddr.StartsWith("10."))
                        {
                            return ipAddr;
                        }
                    }
                }
            }
        }
        return "127.0.0.1";  
    }
}