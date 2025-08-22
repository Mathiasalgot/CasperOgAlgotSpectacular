using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MA.Events;

#if UNITY_EDITOR
[CustomEditor(typeof(StringEvent), true)]
public class StringEventObjectEditor : Editor
{
    private string debugString = "";
    public override void OnInspectorGUI()
    {
        var myTarget = target as StringEvent;

        base.OnInspectorGUI();

        //EditorGUILayout.TextArea("HELLO");

        StringListener[] listeners = GameObject.FindObjectsOfType<StringListener>();

        EditorGUILayout.Space(10);
        
        for (int i = 0; i < listeners.Length; i++)
        {
                
                
                SerializedObject listenerObject = new SerializedObject(listeners[i]);
                SerializedProperty unityEvent = listenerObject.FindProperty("unityEventResponse");
                SerializedProperty targetEvent = listenerObject.FindProperty("gameEvent");

            //Debug.Log(targetEvent.objectReferenceValue + " " + serializedObject.targetObject);
            if (targetEvent.objectReferenceValue == serializedObject.targetObject)
            {
                EditorGUILayout.LabelField(listeners[i].gameObject.name);
                EditorGUILayout.PropertyField(unityEvent);

                EditorGUILayout.Space(10);
                listenerObject.ApplyModifiedProperties();
            }

           
            //EditorGUILayout.ObjectField(listeners[i].gameObject,typeof(GameObject));
        }

        EditorGUILayout.LabelField("Debug information");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Last Sent Value:");
        EditorGUILayout.LabelField(myTarget.GetLastValue().ToString());
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(10);
        debugString = EditorGUILayout.TextField(debugString);
        if(GUILayout.Button("Raise " + debugString))
        {
            myTarget.Raise(debugString);
        }
        
    }
}
#endif