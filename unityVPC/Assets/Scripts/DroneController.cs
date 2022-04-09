using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using TwistMsg = RosMessageTypes.Geometry.TwistMsg;

/// <summary>
/// Simple Drone Controller.
/// First-order system behavior.
/// Sets the body velocity of the corresponding gameobject to the subscribed command_velocity sent over ROS.
/// </summary>
public class DroneController : MonoBehaviour
{
    public enum ControlMode { Keyboard, ROS };

    // Configs
    [Header("Physical behavior settings")]
    [Tooltip("Maximum linear speed of drone")]
    [Min(0)] public float maxLinearSpeed = 5; // [m/s]
    [Tooltip("Maximum angular speed of drone")]
    [Min(0)] public float maxAngularSpeed = 2; // [rad/s]
    [Tooltip("Time constant for first-order system behavior: dy = 1/tau * err")]
    [Range(0.01f, 5f)] public float tau = 1;

    // Cached component references
    [Tooltip("Rigid body of drone")]
    [SerializeField] Rigidbody myRigidBody;
    [Tooltip("Target controller script to notify when drone logged in")]
    [SerializeField] TargetController tc;
    [Tooltip("How the drone is controlled")]
    public ControlMode mode = ControlMode.ROS;

    // ROS settings
    [Header("ROS settings")]
    [Tooltip("The name of the topic to subscribe to")]
    public string topicName = "cmd_vel";
    [Tooltip("Maximal time between 2 velocity command messages [s]")]
    [Range(0.01f, 5f)] public float ROSTimeout = 1f;

    // states
    ROSConnection ros;
    float lastCmdReceived;
    Vector3 targetVelocity;
    Vector3 targetAngularVelocity;


    // Start is called before the first frame update
    void Start()
    {
        if (mode == ControlMode.ROS)
        {
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<TwistMsg>(topicName, ReceiveROSCmd);
        }
        if (!myRigidBody)
            myRigidBody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// FixedUpdate is called together with the physics update.
    /// See: https://stackoverflow.com/questions/34447682/what-is-the-difference-between-update-fixedupdate-in-unity
    /// </summary>
    void FixedUpdate()
    {
        if (mode == ControlMode.ROS)
            CheckROSTimeout();
        else if (mode == ControlMode.Keyboard)
            KeyboardInput();
        SetVelocity();
    }

    /// <summary>
    /// Processes keyboard input commands and sets target velocities accordingly.
    /// </summary>
    void KeyboardInput()
    {
        targetVelocity.Set(
            Input.GetAxis("X-Axis") * maxLinearSpeed,
            Input.GetAxis("Y-Axis") * maxLinearSpeed,
            Input.GetAxis("Z-Axis") * maxLinearSpeed
        );
        targetAngularVelocity.Set(
            Input.GetAxis("RotX") * maxAngularSpeed,
            Input.GetAxis("RotY") * maxAngularSpeed,
            Input.GetAxis("RotZ") * maxAngularSpeed
        );
    }

    /// <summary>
    /// Checks for ROS Timeout.
    /// If so, brings drone to a stop.
    /// </summary>
    void CheckROSTimeout()
    {
        if (Time.time - lastCmdReceived > ROSTimeout)
        {
            targetVelocity = Vector3.zero;
            targetAngularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Sets target linear & angular velocity based on the received ROS message.
    /// Resets ROS timeout counter.
    /// </summary>
    void ReceiveROSCmd(TwistMsg cmd_vel)
    {
        targetVelocity.Set((float)cmd_vel.linear.x, (float)cmd_vel.linear.y, (float)cmd_vel.linear.z);
        targetAngularVelocity.Set((float)cmd_vel.angular.x, (float)cmd_vel.angular.y, (float)cmd_vel.angular.z);
        targetVelocity = transform.rotation * targetVelocity;
        lastCmdReceived = Time.time;
        tc.StartTrajectory();
    }

    /// <summary>
    /// Sets linear & angular velocity with a first-order system behavior (Euler-forward).
    /// Also checks if maximum velocity constraints are satisfied.
    /// </summary>
    void SetVelocity()
    {
        Vector3 vel = myRigidBody.velocity;
        Vector3 angvel = myRigidBody.angularVelocity;

        // Calculate new velocities
        vel += (targetVelocity - vel) * Time.deltaTime / tau;
        angvel += (targetAngularVelocity - angvel) * Time.deltaTime / tau;
        float velMag = vel.magnitude;
        float angvelMag = angvel.magnitude;

        // Check constraints
        if (velMag > maxLinearSpeed)
            vel /= velMag / maxLinearSpeed;
        if (angvelMag > maxAngularSpeed)
            angvel /= angvelMag / maxAngularSpeed;

        // Update velocities
        myRigidBody.velocity = vel;
        myRigidBody.angularVelocity = angvel;
    }

    // On program exit, unsubscribe from all topics
    void OnApplicationQuit()
    {
        // TODO
        // if (ros)
        //     ros.Unsubscribe(topicName);
    }
}
