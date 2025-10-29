using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentBrakeForce;
    private bool isBraking;

    // Settings
    [SerializeField] private float motorForce, brakeForce, maxSteerAngle;
    
    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private volatile bool isDrowsy = false;

    private void Start()
    {
        // Start TCP connection in a separate thread
        receiveThread = new Thread(ConnectToPython);
        receiveThread.Start();
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space) || isDrowsy;
    }

    private void HandleMotor(){
    frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
    frontRightWheelCollider.motorTorque = verticalInput * motorForce;

    // NO SMOOTHNESS, JUST RAW, HARDCORE BRAKING
    currentBrakeForce = isBraking ? brakeForce : 0f;

    ApplyBraking();
    }

    private void ApplyBraking()
    {
        float brutalBrake = isBraking ? 99999f : 0f; 
        frontRightWheelCollider.brakeTorque = brutalBrake;
        frontLeftWheelCollider.brakeTorque = brutalBrake;
        rearLeftWheelCollider.brakeTorque = brutalBrake;
        rearRightWheelCollider.brakeTorque = brutalBrake;
        if (isBraking){
            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;
        }
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
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void ConnectToPython()
    {
        client = new TcpClient("127.0.0.1", 65432);
        stream = client.GetStream();

        while (true)
        {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, buffer.Length);
            isDrowsy = buffer[0] == '1';
        }
    }

    private void OnApplicationQuit()
    {
        receiveThread.Abort();
        client.Close();
    }
}
