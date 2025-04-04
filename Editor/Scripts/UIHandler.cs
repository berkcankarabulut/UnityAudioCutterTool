using UnityEngine;
using UnityEditor;
using System.IO;

public class UIHandler
{
    private readonly string[] supportedFormats = { ".wav", ".mp3", ".ogg", ".aiff" };

    public void DrawHeader()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Audio Cutter Tool", EditorStyles.boldLabel);
        GUILayout.Space(5);
        EditorGUILayout.HelpBox("Select an audio clip or drag & drop one to begin editing", MessageType.Info);
        GUILayout.Space(10);
    }

    public AudioClip DrawAudioClipField(AudioClip clip, System.Action onChanged)
    {
        EditorGUI.BeginChangeCheck();
        var newClip = (AudioClip)EditorGUILayout.ObjectField("Audio File:", clip, typeof(AudioClip), false);
        if (EditorGUI.EndChangeCheck() && clip != null)
        {
            onChanged?.Invoke();
        }
        return newClip;
    }

    public void DrawDragAndDropArea(ref AudioClip clip, System.Action onChanged)
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Audio File Here", EditorStyles.helpBox);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is AudioClip draggedClip)
                        {
                            clip = draggedClip;
                            onChanged?.Invoke();
                            break;
                        }
                    }
                }
                break;
        }
    }

    public void DrawAudioInfo(AudioClip clip)
    {
        if (clip == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Clip Name: {clip.name}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Duration: {clip.length:F2} seconds", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Sample Rate: {clip.frequency} Hz", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Channels: {clip.channels}", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        GUILayout.Space(10);
    }
 
    public void DrawTimeControls(AudioClip clip, ref float startTime, ref float endTime)
    {
        if (clip == null) return;

        EditorGUILayout.BeginHorizontal();
        startTime = EditorGUILayout.FloatField("Start Time (s)", startTime);
        endTime = EditorGUILayout.FloatField("End Time (s)", endTime);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.MinMaxSlider(ref startTime, ref endTime, 0f, clip.length);
    }

    public void DrawExportSettings(ref string savePath, ref string saveFileName)
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string newPath = EditorUtility.SaveFolderPanel("Select Save Directory", savePath, "");
            if (!string.IsNullOrEmpty(newPath))
            {
                savePath = newPath;
                EditorPrefs.SetString("AudioCutter_SavePath", savePath);
            }
        }
        EditorGUILayout.EndHorizontal();

        saveFileName = EditorGUILayout.TextField("File Name", saveFileName);
        GUILayout.Space(10);
    }

    public bool DrawActionButtons(bool enabled, bool isPlaying)
    {
        GUI.enabled = enabled;
        
        EditorGUILayout.BeginHorizontal();
        bool previewPressed = GUILayout.Button(isPlaying ? "Stop Preview" : "Preview", GUILayout.Height(30));
        EditorGUILayout.EndHorizontal();
        
        GUI.enabled = true;
        
        return previewPressed;
    }

    public bool ShouldCutAndSave()
    { 
        return GUILayout.Button("Cut & Save", GUILayout.Height(30));
    }
}