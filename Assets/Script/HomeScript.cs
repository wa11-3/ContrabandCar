using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;

public class HomeScript : MonoBehaviour
{
    public Button[] uiButtons;
    public TMP_InputField ipIF;
    public TMP_Text counterPlayer;
    public TMP_Text infoTx;

    private void Update()
    {
        counterPlayer.text = $"{GameManager.Instance.playersCounter}/6";
    }

    public void OnClickInitButton(string serverType)
    {
        if (serverType == "host")
        {
            NetworkManager.Singleton.StartHost();
            infoTx.text = GetLocalIPAddress();
        }
        else if (serverType == "client")
        {
            if (string.IsNullOrWhiteSpace(ipIF.text))
            {
                infoTx.text = "Debes Colocar una IP";
            }
            else
            {
                try
                {
                    NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = ipIF.text;
                    NetworkManager.Singleton.StartClient();
                    infoTx.text = "";
                    uiButtons[0].gameObject.SetActive(false);
                    uiButtons[1].gameObject.SetActive(false);
                    uiButtons[2].gameObject.SetActive(true);
                    uiButtons[3].gameObject.SetActive(true);
                    ipIF.gameObject.SetActive(false);
                }
                catch
                {
                    infoTx.text = "IP Invalida";
                }
            }
        }

    }

    public void OnClickTeamButton(int teamType)
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerScript>().SetPlayerTeamServerRPC(
            NetworkManager.Singleton.LocalClientId,
            teamType);

        if (NetworkManager.Singleton.IsHost)
        {
            uiButtons[4].gameObject.SetActive(true);
        }
    }

    public void OnClickReadyButton()
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerScript>().GotoSceneServerRPC("Game");
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
