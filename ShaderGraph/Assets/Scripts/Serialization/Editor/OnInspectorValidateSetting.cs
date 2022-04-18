using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OnInspectorValidate))]
public class OnInspectorValidateSetting : Editor
{
    public void OnValidate()
    {
        Debug.Log("OnValidate Setting Call!!");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        var volume = serializedObject.FindProperty("volume");
        EditorGUILayout.PropertyField(volume);
        
        EditorUtility.SetDirty(this);
        serializedObject.ApplyModifiedProperties();
    }
}
