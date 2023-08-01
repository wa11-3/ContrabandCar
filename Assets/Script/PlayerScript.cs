using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerScript : NetworkBehaviour
{
    public NetworkVariable<int> team;

    public GameObject[] cars;

    private void Start()
    {
        NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadEventCompleted:
                if (SceneManager.GetActiveScene().name == "Game" && IsOwner)
                {
                    if (IsHost)
                    {
                        GameManager.Instance.isVan = true;
                        GameObject newCar = Instantiate(cars[0], new Vector3(1, 1, -4.5f * 0), Quaternion.identity);
                        newCar.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.LocalClient.ClientId);
                    }
                }
                if (SceneManager.GetActiveScene().name == "GameOver" && IsHost)
                {
                    var carObjects = GameObject.FindGameObjectsWithTag("Car");
                    foreach (GameObject carObject in carObjects)
                    {
                        Destroy(carObject);
                    }
                }
                break;
        }
    }

    #region ServerRCP
    [ServerRpc]
    public void GotoSceneServerRPC(string sceneName)
    {
        var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {"Game"} " +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }

    [ServerRpc]
    public void SetPlayerTeamServerRPC(ulong cID, int teamType)
    {
        NetworkManager.ConnectedClients[cID].PlayerObject.GetComponent<PlayerScript>().team.Value = teamType;
    }

    [ServerRpc]
    public void SpawnOtherPlayersServerRpc()
    {
        int thiefPos = 0;
        int policePos = -1;
        foreach (ushort cID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (cID != NetworkManager.LocalClientId)
            {
                if (NetworkManager.Singleton.ConnectedClientsList[cID].PlayerObject.GetComponent<PlayerScript>().team.Value == 1)
                {
                    thiefPos += 1;
                    GameObject newCar = Instantiate(cars[1], new Vector3(1, 1, -4.5f * policePos), Quaternion.identity);
                    newCar.GetComponent<NetworkObject>().SpawnWithOwnership(cID);
                }
                else if (NetworkManager.Singleton.ConnectedClientsList[cID].PlayerObject.GetComponent<PlayerScript>().team.Value == 2)
                {
                    policePos += 1;
                    GameObject newCar = Instantiate(cars[2], new Vector3(6, 1, -4.5f * policePos), Quaternion.identity);
                    newCar.GetComponent<NetworkObject>().SpawnWithOwnership(cID);
                }
            }
        }
    }

    [ServerRpc]
    public void RestartCarServerRpc(ulong objectID, int nTeam, ulong cID)
    {
        foreach (NetworkObject netObject in NetworkManager.FindObjectsOfType<NetworkObject>())
        {
            if (objectID == netObject.NetworkObjectId)
            {
                netObject.Despawn();
            }
        }

        GameObject newCar = Instantiate(cars[nTeam - 1], new Vector3(1, 1, -4.5f * nTeam), Quaternion.identity);
        newCar.GetComponent<NetworkObject>().SpawnWithOwnership(cID, true);
    }

    [ServerRpc]
    public void AddForceServerRpc(ulong clienID, ulong objectID, float var1, float var2, float var3, float var4, float var5, float var6)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clienID }
            }
        };

        AddForceCarClientRpc(objectID, var1, var2, var3, var4, var5, var6, clientRpcParams);
    }
    #endregion

    #region ClientRCP
    [ClientRpc]
    public void NewClientRpc(int countP)
    {
        GameManager.Instance.playersCounter = countP;
    }

    [ClientRpc]
    public void AddForceCarClientRpc(ulong objectID, float var1, float var2, float var3, float var4, float var5, float var6, ClientRpcParams clientRpcParams = default)
    {
        foreach (var networkObject in NetworkManager.LocalClient.OwnedObjects)
        {
            if (networkObject.NetworkObjectId == objectID && networkObject.TryGetComponent<Rigidbody>(out Rigidbody rigidCar))
            {
                Debug.Log(networkObject.name);
                Vector3 forceToApply = new Vector3(var1, var2, var3);
                Vector3 positionToApply = new Vector3(var4, var5, var6);
                Debug.Log(forceToApply);
                Debug.Log(positionToApply);
                rigidCar.AddForceAtPosition(forceToApply, positionToApply);
            }
        }
        //    foreach (NetworkObject netObject in NetworkManager.FindObjectsOfType<NetworkObject>())
        //    {
        //        if (netObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbodyCar) && netObject.NetworkObjectId == objectID)
        //        {
        //            Debug.Log(rigidbodyCar.name);
        //            Vector3 forceToApply = new Vector3(var1, var2, var3);
        //            Vector3 positionToApply = new Vector3(var4, var5, var6);
        //            Debug.Log(forceToApply);
        //            Debug.Log(positionToApply);
        //            //rigidbodyCar.AddForceAtPosition(forceToApply, positionToApply);
        //            rigidbodyCar.AddForce(Vector3.forward * 1000, ForceMode.Impulse);
        //        }
        //    }
    }
    #endregion
}
