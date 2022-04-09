using UnityEngine;
using UnityEngine.Assertions;
using Unity.Robotics.ROSTCPConnector;
using VisMea = RosMessageTypes.Custom.VisMeaMsg;

/// <summary>
/// Virtual Camera: Publishes the visual measurment f of designated feature points every N seconds.
/// </summary>
public class VirtualCamPublisher_old : MonoBehaviour
{
    // Configs
    [Header("Publish Configs")]
    [Tooltip("The ROS-topic the position is being published to")]
    public string topicName = "virtual_cam";
    [Tooltip("Publish frequency in Hz")]
    [Range(1f, 200f)]
    public float publishMessageFrequency = 50f;

    [Header("Virtual Camera Configs")]
    [Tooltip("Focal length [m] to emulate")]
    [Range(0.01f, 100f)]
    public float focal = 20;
    [Tooltip("Feature points to track (must be 4!)")]
    public Transform[] featurePoints;

    // Cached references
    ROSConnection ros;

    // states
    float timeElapsed; // Used to determine how much time has elapsed since the last message was published


    void Start()
    {
        // This simple virtual camera script enforces that you track exactly 4 feature points!
        Assert.AreEqual(featurePoints.Length, 4);

        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<VisMea>(topicName);
    }

    private void FixedUpdate()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > 1f / publishMessageFrequency)
        {
            SendVisualMeasurement();
            timeElapsed = 0;
        }
    }

    /// <summary>
    /// Sends a VisMea ros message
    /// TODO: Check if coordinate system of VisMea is same as in the paper! It looks like the x-axis might be inverted, 
    /// </summary>
    private void SendVisualMeasurement()
    {
        // get relative positions pco$
        Vector3 pco1 = transform.InverseTransformPoint(featurePoints[0].position);
        Vector3 pco2 = transform.InverseTransformPoint(featurePoints[1].position);
        Vector3 pco3 = transform.InverseTransformPoint(featurePoints[2].position);
        Vector3 pco4 = transform.InverseTransformPoint(featurePoints[3].position);

        // Only send visual measurement if target is in front of camera
        if (pco1.z > 0 && pco2.z > 0 && pco3.z > 0 && pco4.z > 0)
        {
            // create ros message
            VisMea visualMeasurement = new VisMea(
                focal / pco1.z * pco1.x,
                focal / pco1.z * pco1.y,
                focal / pco2.z * pco2.x,
                focal / pco2.z * pco2.y,
                focal / pco3.z * pco3.x,
                focal / pco3.z * pco3.y,
                focal / pco4.z * pco4.x,
                focal / pco4.z * pco4.y
            );

            // finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, visualMeasurement);
        }
    }

    // On program exit, unregister all topics
    void OnApplicationQuit()
    {
        // TODO: Unregister
    }
}
