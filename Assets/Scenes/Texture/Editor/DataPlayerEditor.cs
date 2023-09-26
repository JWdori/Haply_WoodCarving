using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DataPlayer))]
public class DataPlayerEditor : Editor
{
    private SerializedProperty targetObjectProperty;
    private SerializedProperty positionProperty;
    private SerializedObject hapticControllerSerializedObject;
    private SerializedProperty forceXProperty;
    private SerializedProperty forceYProperty;
    private SerializedProperty forceZProperty;
    private SerializedProperty csvFileProperty;
    private SerializedProperty testtargetObjectProperty;
    private SerializedProperty testpositionProperty;


    private void OnEnable()
    {
        targetObjectProperty = serializedObject.FindProperty("targetObject");
        positionProperty = serializedObject.FindProperty("position");
        testtargetObjectProperty = serializedObject.FindProperty("testtargetObject");
        testpositionProperty = serializedObject.FindProperty("testposition");
        DataPlayer dataPlayer = (DataPlayer)target;
        hapticControllerSerializedObject = new SerializedObject(dataPlayer.hapticController);
        forceXProperty = hapticControllerSerializedObject.FindProperty("forceX");
        forceYProperty = hapticControllerSerializedObject.FindProperty("forceY");
        forceZProperty = hapticControllerSerializedObject.FindProperty("forceZ");
        csvFileProperty = serializedObject.FindProperty("csvFile"); // 추가된 부분
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(targetObjectProperty);
        EditorGUILayout.PropertyField(positionProperty);
        EditorGUILayout.PropertyField(testtargetObjectProperty);
        EditorGUILayout.PropertyField(testpositionProperty);

        hapticControllerSerializedObject.Update();
        EditorGUILayout.PropertyField(forceXProperty);
        EditorGUILayout.PropertyField(forceYProperty);
        EditorGUILayout.PropertyField(forceZProperty);
        hapticControllerSerializedObject.ApplyModifiedProperties();

        // csvFileProperty가 null이 아닌지 확인 후 PropertyField로 표시합니다.
        if (csvFileProperty != null)
        {
            EditorGUILayout.PropertyField(csvFileProperty);
        }
        else
        {
            Debug.LogError("csvFileProperty is null.");
        }

        serializedObject.ApplyModifiedProperties();

        DataPlayer dataPlayer = (DataPlayer)target;

        if (GUILayout.Button("Play"))
        {
            dataPlayer.Play();

        }
        if (GUILayout.Button("Stop"))
        {
            dataPlayer.Stop();
        }
    }
}
