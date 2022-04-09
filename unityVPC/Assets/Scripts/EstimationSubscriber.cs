using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PosRot = RosMessageTypes.Custom.PosRotMsg;

/// <summary>
/// Sets the pose of the estimate smoothly by listening to messages on ROS
/// </summary>
public class EstimationSubscriber : MonoBehaviour
{
    // Config
    [Tooltip("The name of the topic to subscribe to")]
    public string topicName = "estim_pose";
    [Tooltip("Smoothes the pose of the receiving messages over time")]
    public float smoothing = 5f;
    [Tooltip("If true, the estimate is hidden at simulation start")]
    public bool hideAtStart = true;

    // states
    ROSConnection ros;
    Vector3 toPosition;
    Quaternion toRotation;
    bool unhide = false;

    void Start()
    {
        // hide renderer before no message received
        if (hideAtStart && !unhide)
        {
            var rends = GetComponentsInChildren<MeshRenderer>();
            foreach (var r in rends)
                r.enabled = false;
        }

        // Set position & rotation
        toPosition = transform.position;
        toRotation = transform.rotation;

        // setup ros node
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PosRot>(topicName, setPosRot);
    }

    /// <summary>
    /// Sets given position and rotation from ROS message
    /// </summary>
    void setPosRot(PosRot cmd_posRot)
    {
        // store new pose
        toPosition = new Vector3(cmd_posRot.x, cmd_posRot.y, cmd_posRot.z);
        Vector3 axisAngle = new Vector3(cmd_posRot.rx, cmd_posRot.ry, cmd_posRot.rz);
        toRotation = Quaternion.AngleAxis(axisAngle.magnitude * Mathf.Rad2Deg, axisAngle.normalized);

        // "unhide" / show gameobject
        if (!unhide)
        {
            transform.position = toPosition;
            transform.rotation = toRotation;
            var rends = GetComponentsInChildren<MeshRenderer>();
            foreach (var r in rends)
                r.enabled = true;
            unhide = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, toPosition, smoothing * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, smoothing * Time.deltaTime);
        // transform.SetPositionAndRotation(position, rotation);
    }
}
