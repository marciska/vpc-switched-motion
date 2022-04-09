using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PosRot = RosMessageTypes.Custom.PosRotMsg;

/// <summary>
/// Publishes the pose of the gameobject this script is attached to every N seconds.
/// </summary>
public class PosRotPublisher : MonoBehaviour
{
    // Configs
    [Tooltip("The ROS-topic the position is being published to")]
    public string topicName = "pos_rot";
    [Tooltip("Publish frequency in Hz")]
    [Range(1f, 200f)]
    public float publishMessageFrequency = 50f;

    // Cached references
    ROSConnection ros;

    // states
    float timeElapsed; // Used to determine how much time has elapsed since the last message was published


    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PosRot>(topicName);
    }

    void FixedUpdate()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > 1f / publishMessageFrequency)
        {
            SendPose();
            timeElapsed = 0;
        }
    }

    void SendPose()
    {
        // Get angle-axis rotation
        float rotAngle = 0.0f;
        Vector3 rotAxis = Vector3.zero;
        this.transform.rotation.ToAngleAxis(out rotAngle, out rotAxis);
        rotAngle *= Mathf.Deg2Rad;

        // create ROS message
        PosRot posRot = new PosRot(
            this.transform.position.x,
            this.transform.position.y,
            this.transform.position.z,
            rotAngle * rotAxis.x,
            rotAngle * rotAxis.y,
            rotAngle * rotAxis.z
        );

        // Finally send the message to server_endpoint.py running in ROS
        ros.Publish(topicName, posRot);
    }

    // On program exit, unregister all topics
    void OnApplicationQuit()
    {
        // TODO: Unregister
    }
}