using System;
using UnityEngine;

/// <summary>
/// Switches between cameras on keypress events
/// </summary>
[AddComponentMenu("DigiTwin/Cam Numkey Switcher")]
public class CamSwitch : MonoBehaviour
{
    [Tooltip("Assign all cameras to switch between")]
    public Camera[] cameras;

    // Update is called once per frame
    void Update()
    {
        // Check which camera has been requested
        if (Input.GetButtonDown("cam0"))
            SwitchCamera(0);
        else if (Input.GetButtonDown("cam1"))
            SwitchCamera(1);
        else if (Input.GetButtonDown("cam2"))
            SwitchCamera(2);
        else if (Input.GetButtonDown("cam3"))
            SwitchCamera(3);
        else if (Input.GetButtonDown("cam4"))
            SwitchCamera(4);
        else if (Input.GetButtonDown("cam5"))
            SwitchCamera(5);
        else if (Input.GetButtonDown("cam6"))
            SwitchCamera(6);
        else if (Input.GetButtonDown("cam7"))
            SwitchCamera(7);
        else if (Input.GetButtonDown("cam8"))
            SwitchCamera(8);
        else if (Input.GetButtonDown("cam9"))
            SwitchCamera(9);
    }

    /// <summary>
    ///     Turns off all cameras, but turns the desired camera on
    /// </summary>
    void SwitchCamera(int switchToCam)
    {
        // Do not switch if there are no more cameras
        if (switchToCam >= cameras.Length)
            return;

        // Turn off all cameras, and turn on the correct camera
        for (int cam = 0; cam < cameras.Length; ++cam)
        {
            if (switchToCam == cam)
            {
                if (cameras[cam])
                    cameras[cam].enabled = true;
            }
            else
            {
                if (cameras[cam])
                    cameras[cam].enabled = false;
            }
        }
    }
}
