using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;

public class CarScript : NetworkBehaviour
{
    public GameObject followCamera;
    public GameObject virtualCamera;

    public bool isStarted;
    public bool isCounting;

    private Rigidbody rigidCar;
    public Vector3 pointC;
    public Vector3 impulseC;
    public Vector3 normalC;

    public GameObject canvas;

    public AudioSource audioBeep;

    void Start()
    {
        isStarted = false;
        rigidCar = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            followCamera.SetActive(true);
            virtualCamera.SetActive(true);
            canvas = GameObject.FindGameObjectWithTag("Canvas");
            canvas.GetComponentInChildren<Canvas>().enabled = true;
            if (gameObject.name.StartsWith("Van"))
            {
                audioBeep = GetComponent<AudioSource>();
            }
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.H) && IsOwner)
        {
            //rigidCar.AddForceAtPosition(impulseC, pointC);
            //Debug.Log(impulseC.normalized);
            canvas = GameObject.FindWithTag("Canvas");
        }

        if (Input.GetKey(KeyCode.R) && this.gameObject.TryGetComponent<NetworkObject>(out NetworkObject netObject) && IsOwner)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerScript>().RestartCarServerRpc(
                netObject.NetworkObjectId,
                GameManager.Instance.team,
                NetworkManager.Singleton.LocalClient.ClientId);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject netObject) && IsOwner)
        {
            pointC = collision.GetContact(0).point;
            impulseC = collision.GetContact(0).impulse;
            normalC = collision.GetContact(0).normal;

            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerScript>().AddForceServerRpc(
                        netObject.OwnerClientId,
                        netObject.NetworkObjectId,
                        impulseC.x,
                        impulseC.y,
                        impulseC.z,
                        pointC.x,
                        pointC.y,
                        pointC.z);
        }
        //    //    rigidCar.AddForceAtPosition(normalC, pointC);
        //    //if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject netObject) && IsOwner)
        //    //{
        //    //    pointC = collision.GetContact(0).point;
        //    //    impulseC = collision.GetContact(0).impulse;
        //    //    normalC = collision.GetContact(0).normal;

        //    //    Debug.Log(netObject.NetworkObjectId);
        //    //    Debug.Log(collision.GetContact(0).point);
        //    //    Debug.Log(collision.GetContact(0).impulse);
        //    //    Debug.Log(collision.GetContact(0).normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner && other.CompareTag("Target"))
        {
            GameManager.Instance.winner = "Contrabandist";
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerScript>().GotoSceneServerRPC("GameOver");
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsOwner && GameManager.Instance.isVan && !isStarted)
        {
            isStarted = true;
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerScript>().SpawnOtherPlayersServerRpc();
            StartCoroutine(VelocityChecker());
        }
    }

    IEnumerator VelocityChecker()
    {
        isCounting = false;
        while (true)
        {
            if (rigidCar.velocity.magnitude <= 2.5f && !isCounting)
            {
                isCounting = true;
                StartCoroutine(CountingBreak());
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator CountingBreak()
    {
        int counter = 0;
        while (true)
        {
            audioBeep.Play();
            counter += 1;
            if (counter >= 10)
            {
                GameManager.Instance.winner = "Police";
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerScript>().GotoSceneServerRPC("GameOver");
                Destroy(gameObject);
            }
            yield return new WaitForSeconds(1f);
            if (rigidCar.velocity.magnitude > 2.5f && isCounting)
            {
                break;
            }
        }
        isCounting = false;
    }
}
