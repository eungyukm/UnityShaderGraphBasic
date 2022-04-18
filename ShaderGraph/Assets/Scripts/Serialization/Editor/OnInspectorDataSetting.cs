using UnityEditor;

[CustomEditor(typeof(OnInspectorData))]
public class OnInspectorDataSetting : Editor
{
    public override void OnInspectorGUI()
    {
        var valueProperty = serializedObject.FindProperty("value");
        EditorGUILayout.PropertyField(valueProperty);
        
        SerializedProperty serializedPropertyTR = serializedObject.FindProperty("tr");
        EditorGUILayout.PropertyField(serializedPropertyTR);

        // ApplyModifiedProperties는 내부 캐쉬에 변경점을 적용합니다.
        serializedObject.ApplyModifiedProperties();
    }
}
