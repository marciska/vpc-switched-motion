using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using SwitchingMsg = RosMessageTypes.Std.Int32Msg;

public class TargetController_old : MonoBehaviour
{
    // configs
    [Header("General settings")]
    [Tooltip("Center of the trajectory")]
    public Transform center;
    [Tooltip("The ROS-topic the switching is being published to")]
    public string topicName = "switching";
    [Tooltip("Publish frequency in Hz")]
    [Range(1f, 200f)]
    public float publishMessageFrequency = 50f;
    [Tooltip("Bird only starts trajectory if this is set to true")]
    [SerializeField] bool started = false;

    [Header("Van-der-Pol oscillator settings")]
    public float vEpsilon = 1f;
    [Range(0f, 5f)]
    public float vSpeed = 1f;
    [Range(1f, 10f)]
    public float vScale = 2f;

    [Header("Duffing oscillator settings")]
    public float dAlpha = 1f;
    public float dBeta = 1f;
    public float dGamma = 1f;
    public float dDelta = 1f;
    public float dOmega = 1f;
    [Range(0f, 5f)]
    public float dSpeed = 1f;
    [Range(1f, 10f)]
    public float dScale = 2f;

    [Header("Quartic oscillator settings")]
    public float qK = 1f;
    [Range(0f, 5f)]
    public float qSpeed = 1f;
    [Range(1f, 10f)]
    public float qScale = 2f;

    [Header("Lorenz attractor settings")]
    public float lSigma = 10f;
    public float lRho = 26f;
    public float lBeta = 8f / 3f;
    [Range(0f, 5f)]
    public float lSpeed = 1f;
    [Range(0.01f, 10f)]
    public float lScale = 2f;

    // Cached references
    ROSConnection ros;

    // states
    Vector3 offset = Vector3.zero;
    [SerializeField] int psi = 0; // trajectory index
    float timeElapsed; // Used to determine how much time has elapsed since the last message was published

    // Start is called before the first frame update
    void Start()
    {
        if (center)
            offset = center.position;

        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<SwitchingMsg>(topicName);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timeElapsed > 1f / publishMessageFrequency)
        {
            SendSwitching();
            timeElapsed = 0;
        }

        if (!started)
            return;

        // Update position and orientation
        var vel = GetVelocity();
        transform.position += vel * Time.deltaTime;
        if (vel.sqrMagnitude != 0f)
            transform.rotation = Quaternion.LookRotation(vel);
    }

    private void SendSwitching()
    {
        // create ROS message
        SwitchingMsg switching = new SwitchingMsg(psi);

        // Finally send the message to server_endpoint.py running in ROS
        ros.Publish(topicName, switching);
    }

    Vector3 GetVelocity()
    {
        var pos = transform.position - offset;

        // Obtain Velocity
        Vector3 vel = Vector3.zero;
        switch (psi)
        {
            case 1:
                vel = vSpeed * VanDerPol(pos / vScale, vEpsilon);
                break;
            case 2:
                vel = qSpeed * Quartic(pos / qScale, qK);
                break;
            case 3:
                vel = dSpeed * Duffing(pos / dScale, dAlpha, dBeta, dGamma, dDelta, dOmega);
                break;
            case 4:
                vel = lSpeed * Lorenz(pos / lScale, lSigma, lRho, lBeta);
                break;
            default:
                break;
        }

        return vel;
    }

    public Vector3 VanDerPol(Vector3 position, float epsilon)
    {
        Vector3 velocity = Vector3.zero;
        velocity.x = position.z;
        velocity.y = 0;
        velocity.z = -position.x + epsilon * (1 - Mathf.Pow(position.x, 2)) * position.z;
        return velocity;
    }

    public Vector3 Duffing(Vector3 position, float alpha, float beta, float gamma, float delta, float omega)
    {
        Vector3 velocity = Vector3.zero;
        velocity.x = position.z;
        velocity.y = 0;
        velocity.z = -delta * position.z - alpha * position.x - beta * Mathf.Pow(position.x, 3) + gamma * Mathf.Cos(omega * Time.time);
        return velocity;
    }

    public Vector3 Quartic(Vector3 position, float k)
    {
        Vector3 velocity = Vector3.zero;
        velocity.x = position.z;
        velocity.y = 0;
        velocity.z = -k * Mathf.Pow(position.x, 3) + k * position.x;
        return velocity;
    }

    public Vector3 Lorenz(Vector3 position, float sigma, float rho, float beta)
    {
        var x = position.x;
        var y = position.z;
        var z = position.y;
        Vector3 velocity = Vector3.zero;
        velocity.x = sigma * (y - x);
        velocity.y = x * (rho - z) - y;
        velocity.z = x * y - beta * z;
        return velocity;
    }

    public void SwitchTrajectory()
    {
        psi += 1;
        if (psi > 2)
            psi = 1;
    }

    public void StartTrajectory()
    {
        if (!started)
            started = true;
    }
}
