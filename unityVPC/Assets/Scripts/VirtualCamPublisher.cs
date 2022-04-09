using UnityEngine;
using UnityEngine.Assertions;
using Unity.Robotics.ROSTCPConnector;
using VisMea = RosMessageTypes.Custom.VisMeaMsg;

/// <summary>
/// Virtual Camera: Publishes the visual measurment f of designated feature points in MATLAB coordinate system (right-handed system).
/// </summary>
public class VirtualCamPublisher : MonoBehaviour
{
    // Configs
    [Header("Publish Configs")]
    [Tooltip("The ROS-topic the position is being published to")]
    public string topicName = "virtual_cam";
    [Tooltip("Publish frequency in Hz")]
    [Range(1f, 200f)]
    public float publishMessageFrequency = 50f;

    [Header("Virtual Camera Configs")]
    [Tooltip("Target Transform object")]
    public Transform target;
    [Tooltip("Feature points in MATLAB coordinate system")]
    public Vector3[] fp = new Vector3[4];
    [Tooltip("Focal length [m] to emulate")]
    [Range(0.01f, 100f)]
    public float focal = 20;

    // Cached references
    ROSConnection ros;

    // states
    Matrix4x4 unity2matlab;
    float timeElapsed; // Used to determine how much time has elapsed since the last message was published


    // Start is called before the first frame update
    void Start()
    {
        // this simple virtual camera script enforces that you track exactly 4 feature points!
        Assert.AreEqual(fp.Length, 4);

        // create transformation matrix
        unity2matlab = Matrix4x4.zero;
        unity2matlab[0, 0] = 1;
        unity2matlab[2, 1] = 1;
        unity2matlab[1, 2] = 1;
        unity2matlab[3, 3] = 1;

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
    /// </summary>
    private void SendVisualMeasurement()
    {
        // get gco in MATLAB coordinate system
        var gwc = unity2matlab * transform.localToWorldMatrix * unity2matlab;
        var gwo = unity2matlab * target.localToWorldMatrix * unity2matlab;
        var gco = gwc.inverse * gwo;
        // Debug.Log("gwo:\n" + gwo);
        // Debug.Log("gwc:\n" + gwc);
        // Debug.Log("gco:\n" + gco);

        // transform feature points to relative camera coordinate system
        var pco1 = gco.MultiplyPoint3x4(fp[0]);
        var pco2 = gco.MultiplyPoint3x4(fp[1]);
        var pco3 = gco.MultiplyPoint3x4(fp[2]);
        var pco4 = gco.MultiplyPoint3x4(fp[3]);

        // Only send visual measurement if target is in front of camera
        if (pco1.y > 0 && pco2.y > 0 && pco3.y > 0 && pco4.y > 0)
        {
            // create ros message
            VisMea visualMeasurement = new VisMea(
                focal / pco1.y * pco1.x,
                focal / pco1.y * pco1.z,
                focal / pco2.y * pco2.x,
                focal / pco2.y * pco2.z,
                focal / pco3.y * pco3.x,
                focal / pco3.y * pco3.z,
                focal / pco4.y * pco4.x,
                focal / pco4.y * pco4.z
            );

            // finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, visualMeasurement);
        }
    }

    // // Update is called once per frame
    // void Update()
    // {
    //     var gwc = transform.localToWorldMatrix;
    //     var gwo = target.localToWorldMatrix;
    //     var gco = gwc.inverse * gwo;

    //     // convert
    //     var gwc_ = unity2matlab * gwc * unity2matlab;
    //     var gwo_ = unity2matlab * gwo * unity2matlab;
    //     var gco_ = unity2matlab * gco * unity2matlab;
    //     var gco__ = gwc_.inverse * gwo_;

    //     Debug.Log("gwo:\n" + gwo_);
    //     Debug.Log("gwc:\n" + gwc_);
    //     Debug.Log("gco1:\n" + gco_);
    //     Debug.Log("gco2:\n" + gco__);

    //     // convert
    //     var pco1 = gco__.MultiplyPoint3x4(fp[0]);
    //     var pco2 = gco__.MultiplyPoint3x4(fp[1]);
    //     var pco3 = gco__.MultiplyPoint3x4(fp[2]);
    //     var pco4 = gco__.MultiplyPoint3x4(fp[3]);

    //     Debug.Log("red: " + pco1 +
    //             "\nblue: " + pco2 +
    //             "\ngreen: " + pco3 +
    //             "\nyellow: " + pco4);

    //     float[] f = {
    //         focal / pco1.y * pco1.x,
    //         focal / pco1.y * pco1.z,
    //         focal / pco2.y * pco2.x,
    //         focal / pco2.y * pco2.z,
    //         focal / pco3.y * pco3.x,
    //         focal / pco3.y * pco3.z,
    //         focal / pco4.y * pco4.x,
    //         focal / pco4.y * pco4.z
    //         };

    //     Debug.Log("Vismea:\n" +
    //               f[0] + " " + f[1] + "\n" +
    //               f[2] + " " + f[3] + "\n" +
    //               f[4] + " " + f[5] + "\n" +
    //               f[6] + " " + f[7] + "\n"
    //             );
    // }
}
