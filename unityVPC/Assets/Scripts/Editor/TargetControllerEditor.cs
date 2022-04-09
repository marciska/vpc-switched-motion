using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TargetController))]
public class TargetControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TargetController tc = (TargetController)target;
        if (GUILayout.Button("Switch Trajectory"))
            tc.SwitchTrajectory();
    }
}
