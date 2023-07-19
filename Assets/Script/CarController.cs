using Unity.Netcode;
using UnityEngine;
using QFSW.QC;

public class CarController : NetworkBehaviour
{
    public GameObject followCamera;
    public GameObject virtualCamera;

    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;

    // Settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    public Rigidbody rigidCar;
    public Vector3 pointC;
    public Vector3 impulseC;
    public Vector3 normalC;


    public void Start()
    {
        rigidCar = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            followCamera.SetActive(true);
            virtualCamera.SetActive(true);
            //if (GameManager.Instance.team == 0)
            //{
            //    NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerScript>().SpawnWaitCarServerRpc(
            //        NetworkManager.LocalClient.ClientId,
            //        NetworkManager.LocalTime.FixedTime);
            //}
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.H) && IsOwner)
        {
            rigidCar.AddForceAtPosition(impulseC, pointC);
            Debug.Log(impulseC.normalized);
        }

        if (Input.GetKey(KeyCode.R) && this.gameObject.TryGetComponent<NetworkObject>(out NetworkObject netObject) && IsOwner)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerScript>().RestartCarServerRpc(
                netObject.NetworkObjectId,
                GameManager.Instance.team,
                NetworkManager.Singleton.LocalClient.ClientId);
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log(rigidCar.velocity.magnitude);
        if (IsOwner)
        {
            GetInput();
            HandleMotor();
            HandleSteering();
            UpdateWheels();

        }
        //GetInput();
        //HandleMotor();
        //HandleSteering();
        //UpdateWheels();
    }

    private void GetInput()
    {
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");

        // Acceleration Input
        verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject netObject) && IsOwner)
        {
            Debug.Log("Hola");
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
        //    rigidCar.AddForceAtPosition(normalC, pointC);
        //}
        //if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject netObject) && IsOwner)
        //{
        //    pointC = collision.GetContact(0).point;
        //    impulseC = collision.GetContact(0).impulse;
        //    normalC = collision.GetContact(0).normal;

        //    Debug.Log(netObject.NetworkObjectId);
        //    Debug.Log(collision.GetContact(0).point);
        //    Debug.Log(collision.GetContact(0).impulse);
        //    Debug.Log(collision.GetContact(0).normal);
        //}
    }

    [Command]
    public void AddForcePoint(Vector3 impulse, Vector3 point)
    {
        Debug.Log(impulse);
        Debug.Log(point);

        rigidCar.AddForceAtPosition(impulse, point);
    }
}
