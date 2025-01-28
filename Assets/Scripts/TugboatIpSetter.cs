using FishNet.Transporting.Tugboat;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class TugboatIpSetter : MonoBehaviour
{
    [SerializeField]
    private Tugboat tugboat;

    [SerializeField]
    private TMP_InputField ipInput;
    [SerializeField]
    private TextMeshProUGUI yourIPText;

    void Start()
    {
        string ipv4 = IPManager.GetIP(ADDRESSFAM.IPv4);

        ipInput.onValueChanged.AddListener(SetIp);

        yourIPText.text = $"your address:\n {ipv4}";
        tugboat.SetServerBindAddress(ipv4, FishNet.Transporting.IPAddressType.IPv4);
        tugboat.SetClientAddress(ipv4);
    }

    private void SetIp(string newIp)
    {
        tugboat.SetClientAddress(newIp);
    }
}

public class IPManager
{
    public static string GetIP(ADDRESSFAM Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }
}

public enum ADDRESSFAM
{
    IPv4, IPv6
}