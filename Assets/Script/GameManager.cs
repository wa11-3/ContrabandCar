using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int team;
    public int playersCounter;

    public GameObject playerObject;

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        team = 0;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerScript>().NewClientRpc(NetworkManager.Singleton.ConnectedClients.Count);
        }
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
    }
}
