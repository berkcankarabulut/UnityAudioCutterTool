using UnityEngine;
using UnityEditor;
using System.IO;

public class AudioExporter
{
    private AudioProcessor audioProcessor;

    public AudioExporter(AudioProcessor audioProcessor)
    {
        this.audioProcessor = audioProcessor;
    }
    
    public void CutAndSave(AudioClip sourceClip, float startTime, float endTime, string savePath, string saveFileName)
    {
        if (sourceClip == null) return;

        AudioClip cutClip = audioProcessor.CreateCutClip(sourceClip, startTime, endTime);
        if (cutClip == null)
        {
            EditorUtility.DisplayDialog("Error", "Please specify a valid cutting range.", "OK");
            return;
        }

        string finalPath = GetFinalPath(savePath, saveFileName);
        
        if (audioProcessor.SaveWav(finalPath, cutClip))
        {
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Audio file saved to:\n{finalPath}", "OK");
            EditorUtility.RevealInFinder(finalPath);
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to save audio file.", "OK");
        }
    }

    private string GetFinalPath(string savePath, string saveFileName)
    {
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Path.Combine(Application.dataPath, "Audio");
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        }
        return Path.Combine(savePath, saveFileName);
    }
}