using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using SwitchingMsg = RosMessageTypes.Std.Int32Msg;
using System;

public class TargetController : MonoBehaviour
{
    // configs
    [Header("General settings")]
    [Tooltip("The ROS-topic the switching is being published to")]
    public string topicName = "switching";
    [Tooltip("Publish frequency in Hz")]
    [Range(1f, 200f)]
    public float publishMessageFrequency = 50f;
    [Tooltip("Bird only starts trajectory if this is set to true")]
    [SerializeField] bool started = false;
    public bool automaticSwitch = true;
    public Vector3[] automaticSwitchPoints;
    [Range(0f, 1f)] public float automaticSwitchPointsWidth = 0.2f;
    public bool drawGizmos = true;

    [Header("Van-der-Pol 1 oscillator settings")]
    public float epsilon1 = .5f;
    [Range(0f, 5f)]
    public float speed1 = 1f;
    [Range(1f, 10f)]
    public float scale1 = 1f;
    [Tooltip("Center of the trajectory")]
    public Transform center1;

    [Header("Van-der-Pol 2 oscillator settings")]
    public float epsilon2 = 1.5f;
    [Range(0f, 5f)]
    public float speed2 = .5f;
    [Range(1f, 10f)]
    public float scale2 = 1f;
    [Tooltip("Center of the trajectory")]
    public Transform center2;

    // Cached references
    ROSConnection ros;

    // states
    Vector3 offset1 = Vector3.zero;
    Vector3 offset2 = Vector3.zero;
    [SerializeField] int psi = 0; // trajectory index
    bool insideAutomaticSwitchingPoint = true;
    float timeElapsed; // Used to determine how much time has elapsed since the last message was published

    // Start is called before the first frame update
    void Start()
    {
        SetTrajectoryCenters();

        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<SwitchingMsg>(topicName);
    }

    private void SetTrajectoryCenters()
    {
        if (center1)
            offset1 = center1.position;
        if (center2)
            offset2 = center2.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > 1f / publishMessageFrequency)
        {
            SendSwitching();
            timeElapsed = 0;
        }

        if (!started)
            return;

        // Check for automatic switching
        if (automaticSwitch)
            CheckSwitching();

        // Update position and orientation
        var vel = GetVelocity();
        transform.position += vel * Time.deltaTime;
        if (vel.sqrMagnitude != 0f)
            transform.rotation = Quaternion.LookRotation(vel);
    }

    private void CheckSwitching()
    {
        // Check if within automatic switching point width
        bool isWithin = false;
        foreach (var point in automaticSwitchPoints)
        {
            if ((transform.position - point).sqrMagnitude <= automaticSwitchPointsWidth * automaticSwitchPointsWidth)
            {
                isWithin = true;
                break;
            }
        }

        // do automatic switch if we JUST NOW entered the switching point.
        // if we didn't leave it from before, do nothing
        if (!insideAutomaticSwitchingPoint && isWithin)
            SwitchTrajectory();

        // update "insideSwitchingPoint" flag
        insideAutomaticSwitchingPoint = isWithin;
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
        // Obtain Velocity
        Vector3 vel = Vector3.zero;
        Vector3 pos = Vector3.zero;
        switch (psi)
        {
            case 1:
                pos = transform.position - offset1;
                vel = speed1 * VanDerPol(pos / scale1, epsilon1);
                break;
            case 2:
                pos = transform.position - offset2;
                vel = speed2 * VanDerPol(pos / scale2, epsilon2);
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

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        // Draw a yellow sphere at the automatic switching point's position
        Gizmos.color = Color.yellow;
        foreach (var point in automaticSwitchPoints)
            Gizmos.DrawSphere(point, automaticSwitchPointsWidth);

        // Draw lines for trajectories
        const int M = 150;
        const float dt = 0.1f;
        SetTrajectoryCenters();
        Gizmos.color = Color.magenta;
        Vector3[] positions = new Vector3[M];
        positions[0] = transform.position;
        for (int i = 1; i < M; i++)
        {
            positions[i] = positions[i - 1] + dt * speed1 * VanDerPol((positions[i - 1] - offset1) / scale1, epsilon1);
            Gizmos.DrawLine(positions[i - 1], positions[i]);
        }
        Gizmos.color = Color.green;
        for (int i = 1; i < M; i++)
        {
            positions[i] = positions[i - 1] + dt * speed2 * VanDerPol((positions[i - 1] - offset2) / scale2, epsilon2);
            Gizmos.DrawLine(positions[i - 1], positions[i]);
        }
    }
}
