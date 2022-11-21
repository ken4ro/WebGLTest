using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;

public static class NetworkHelper
{
    public static string GetNetworkAddress()
    {
        string ret = "";

        var hostName = new Uri(GlobalState.Instance.ApplicationGlobalSettings.FileServer).Host;
        var ip = Dns.GetHostEntry(hostName);
        string ipAddress = null;
        foreach (var address in ip.AddressList)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = address.ToString();
                break;
            }
        }

        IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
        TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();
        foreach (var tcpInfo in tcpConnections)
        {
            if (tcpInfo.State == TcpState.Established && tcpInfo.RemoteEndPoint.Address.ToString() == ipAddress)
            {
                ret = tcpInfo.LocalEndPoint.Address.ToString();
                Debug.Log($"Get Network Address: {ret}");
                break;
            }
        }

        return ret;
    }

    public static string GetEthernetAddress()
    {
        string ret = "";

        var nis = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var ni in nis)
        {
            if (ni.Name.Contains("イーサネット")) // TODO: 確実な方法で実装する
            {
                var properties = ni.GetIPProperties();
                foreach (var unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var address = unicast.Address.ToString();
                        if (string.IsNullOrEmpty(address) || address.Contains("169.254")) continue;
                        ret = address;
                        Debug.Log($"Get Ethernet Address: {ret}");
                        break;
                    }
                }
            }
        }

        return ret;
    }

    public static string GetWiFiAddress()
    {
        string ret = "";
        var nis = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var ni in nis)
        {
            if (ni.Name.Contains("Wi-Fi"))
            {
                var properties = ni.GetIPProperties();
                foreach (var unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var address = unicast.Address.ToString();
                        if (string.IsNullOrEmpty(address) || address.Contains("169.254")) continue;
                        ret = address;
                        Debug.Log($"Get Wi-Fi address: {ret}");
                        break;
                    }
                }
            }
        }

        return ret;
    }

    //public static async UniTask<bool> Ping(string host = "www.google.com")
    //{
    //    try
    //    {
    //        // ホストのIPアドレスを取得
    //        IPAddress[] serverIPs = await Dns.GetHostAddressesAsync(host);
    //        // Ping 送信
    //        UnityEngine.Ping ping = new UnityEngine.Ping(serverIPs[0].ToString());
    //        // 終わるまで待つ
    //        await UniTask.WaitUntil(() => ping.isDone == true);
    //    }
    //    catch (Exception)
    //    {
    //        return false;
    //    }
    //    return true;
    //}

    public static ushort ReverseEndian(ushort x)
    {
        return Convert.ToUInt16((x << 8 & 0xff00) | (x >> 8));
    }

    public static uint ReverseEndian(uint x)
    {
        return (x << 24 | (x & 0xff00) << 8 | (x & 0xff0000) >> 8 | x >> 24);
    }
}
