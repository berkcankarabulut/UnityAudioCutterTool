using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class AudioUtility
{
    public static void PlayClip(AudioClip clip)
    {
        // Use reflection to access Unity's internal audio preview methods
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        
        if (audioUtilClass != null)
        {
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );
            
            if (method != null)
            {
                method.Invoke(null, new object[] { clip, 0, false });
            }
            else
            {
                Debug.LogError("Could not find PlayPreviewClip method");
            }
        }
        else
        {
            Debug.LogError("Could not find AudioUtil class");
        }
    }
    
    public static void StopClip(AudioClip clip)
    {
        // Use reflection to access Unity's internal audio preview methods
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        
        if (audioUtilClass != null)
        {
            MethodInfo method = audioUtilClass.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public
            );
            
            if (method != null)
            {
                method.Invoke(null, null);
            }
            else
            {
                Debug.LogError("Could not find StopAllPreviewClips method");
            }
        }
        else
        {
            Debug.LogError("Could not find AudioUtil class");
        }
    }
}