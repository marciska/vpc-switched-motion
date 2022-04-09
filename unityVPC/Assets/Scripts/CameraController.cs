using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Points the camera at the center point of all interesting gameobjects
/// </summary>
public class CameraController : MonoBehaviour
{
    // configs
    [Tooltip("The offset from the central point the camera follows")]
    public Vector3 offset = new Vector3(2, 4, -3);
    [Tooltip("Smoothes the pose of the receiving messages over time")]
    public float smoothing = 5f;
    [Tooltip("The objects whose center we want to look at")]
    public GameObject[] gos;

    // states
    Vector3 center = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        // remove disabled go's
        int active_gos = 0;
        foreach (var go in gos)
            if (go.activeSelf)
                active_gos++;
        GameObject[] gos_ = new GameObject[active_gos];
        int go_number = 0;
        for (int i = 0; i < gos.Length; i++)
        {
            if (gos[i].activeSelf)
            {
                gos_[go_number] = gos[i];
                go_number++;
            }
        }
        gos = gos_;

        // start by calculating center
        if (gos.Length > 0)
            CalculateCenter();
        else
            Destroy(this);
    }

    void FixedUpdate()
    {
        CalculateCenter();
        MoveTurn();
    }

    void MoveTurn()
    {
        Vector3 targetPosition = center + offset;
        Vector3 direction = center - transform.position;
        Quaternion toRotation = Quaternion.LookRotation(direction);

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, smoothing * Time.deltaTime);
    }

    void CalculateCenter()
    {
        // calculate center point
        center.Set(0, 0, 0);
        foreach (var go in gos)
            center += go.transform.position;
        center /= gos.Length;
    }
}
