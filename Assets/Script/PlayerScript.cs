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
        team = new NetworkVariable<int>(GameManager.Instance.team);
        NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadEventCompleted:
                if (SceneManager.GetActiveScene().name == "Game" && IsOwner)
                {
                    SpawnCarServerRpc(GameManager.Instance.team, NetworkManager.LocalClient.ClientId);
                }
                break;
        }
    }

    #region ServerRCP
    [ServerRpc]
    public void SpawnCarServerRpc(int nTeam, ulong cID)
    {
        GameObject newCar = Instantiate(cars[nTeam - 1], new Vector3(1, 1, -4.5f * nTeam), Quaternion.identity);
        newCar.GetComponent<NetworkObject>().SpawnWithOwnership(cID);
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
        //foreach (NetworkObject netObject in NetworkManager.FindObjectsOfType<NetworkObject>())
        //{
        //    if (netObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbodyCar) && netObject.NetworkObjectId == clienID)
        //    {
        //        Debug.Log(rigidbodyCar.name);
        //        Vector3 forceToApply = new Vector3(var1, var2, var3);
        //        Vector3 positionToApply = new Vector3(var4, var5, var6);
        //        Debug.Log(forceToApply);
        //        Debug.Log(positionToApply);
        //        //rigidbodyCar.AddForceAtPosition(forceToApply, positionToApply);
        //        rigidbodyCar.AddForce(Vector3.forward * 1000);
        //    }
        //}
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
        foreach (NetworkObject netObject in NetworkManager.FindObjectsOfType<NetworkObject>())
        {
            if (netObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbodyCar) && netObject.NetworkObjectId == objectID)
            {
                Debug.Log(rigidbodyCar.name);
                Vector3 forceToApply = new Vector3(var1, var2, var3);
                Vector3 positionToApply = new Vector3(var4, var5, var6);
                Debug.Log(forceToApply);
                Debug.Log(positionToApply);
                //rigidbodyCar.AddForceAtPosition(forceToApply, positionToApply);
                rigidbodyCar.AddForce(Vector3.forward * 1000, ForceMode.Impulse);
            }
        }
    }
    #endregion
}
