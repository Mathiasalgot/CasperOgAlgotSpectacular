using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MA.Events;

#if UNITY_EDITOR
[CustomEditor(typeof(VoidEvent), true)]
public class VoidEventObjectEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        var myTarget = target as VoidEvent;

        base.OnInspectorGUI();

        //EditorGUILayout.TextArea("HELLO");

        VoidListener[] listeners = GameObject.FindObjectsOfType<VoidListener>();

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

       


        EditorGUILayout.Space(10);
        
        if(GUILayout.Button("Raise Event"))
        {
            myTarget.Raise();
        }
        
    }
}
#endif