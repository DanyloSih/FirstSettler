using UnityEngine;
using UnityEditor;
using Utilities.Math.Tests;

[CustomEditor(typeof(OctreeTest))]
public class OctreeTestEditor : Editor
{
    private SerializedProperty _pointerPositionProp;
    private SerializedProperty _pointerRankProp;
    private SerializedProperty _pointerValueProp;
    private SerializedProperty _scaleProp;

    private void OnEnable()
    {
        _pointerPositionProp = serializedObject.FindProperty("_pointerPosition");
        _pointerRankProp = serializedObject.FindProperty("_pointerRank");
        _pointerValueProp = serializedObject.FindProperty("_pointerValue");
        _scaleProp = serializedObject.FindProperty("_scale");
    }

    private void OnSceneGUI()
    {
        serializedObject.Update();

        OctreeTest octreeTest = (OctreeTest)target;

        Vector3Int pointerPosition = _pointerPositionProp.vector3IntValue;
        Vector3 currentPos = octreeTest.transform.position + (Vector3)pointerPosition;

        EditorGUI.BeginChangeCheck();
        Vector3 newPos = Handles.PositionHandle(currentPos, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Vector3Int roundedPos = Vector3Int.FloorToInt(newPos - octreeTest.transform.position);
            serializedObject.Update();
            _pointerPositionProp.vector3IntValue = roundedPos;
            serializedObject.ApplyModifiedProperties();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        if (GUILayout.Button("Set Data"))
        {
            ((OctreeTest)target).SetData();
        }

        if (GUILayout.Button("Get Data"))
        {
            ((OctreeTest)target).GetData();
        }

        if (GUILayout.Button("Apply Array"))
        {
            ((OctreeTest)target).ApplyArray();
        }

        serializedObject.ApplyModifiedProperties();
    }
}