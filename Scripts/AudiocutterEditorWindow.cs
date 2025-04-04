using UnityEngine;
using UnityEditor;
using System.IO;

public class AudioCutterEditorWindow : EditorWindow
{
    private AudioClip sourceClip;
    private float startTime = 0f;
    private float endTime = 10f;
    private Vector2 scrollPosition;
    private string savePath = "";
    private string saveFileName = "cut_audio.wav";
    private bool showWaveform = true;
    private AudioClip previewClip;
    private bool isPlaying = false;
    
    // References to other scripts
    private WaveformVisualizer waveformVisualizer;
    private AudioProcessor audioProcessor;
    
    // Supported audio formats
    private readonly string[] supportedFormats = { ".wav", ".mp3", ".ogg", ".aiff" };
    
    [MenuItem("Audio/Audio Cutter Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<AudioCutterEditorWindow>("Audio Cutter Tool");
        window.minSize = new Vector2(500, 350);
        window.Show();
    }
    
    private void OnEnable()
    {
        // Initialize helper classes
        waveformVisualizer = new WaveformVisualizer();
        audioProcessor = new AudioProcessor();
        
        // Load saved preferences
        if (EditorPrefs.HasKey("AudioCutter_SavePath"))
        {
            savePath = EditorPrefs.GetString("AudioCutter_SavePath");
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        DrawHeader();
        
        // Audio file selection
        EditorGUI.BeginChangeCheck();
        sourceClip = (AudioClip)EditorGUILayout.ObjectField("Audio File:", sourceClip, typeof(AudioClip), false);
        if (EditorGUI.EndChangeCheck() && sourceClip != null)
        {
            ResetEditorWithNewClip();
        }
        
        if (sourceClip == null)
        {
            EditorGUILayout.HelpBox("Please select an audio file.", MessageType.Info);
            DrawDragAndDropArea();
            return;
        }
        
        DrawAudioInfo();
        
        // Draw waveform and controls
        if (showWaveform)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            showWaveform = EditorGUILayout.Toggle("Show Waveform", showWaveform);
            
            if (GUILayout.Button("Refresh Waveform", GUILayout.Width(150)))
            {
                waveformVisualizer.GenerateWaveformData(sourceClip);
            }
            EditorGUILayout.EndHorizontal();
            
            // Adjust start and end times via the waveform
            bool timesChanged = waveformVisualizer.DrawWaveform(sourceClip, ref startTime, ref endTime);
            if (timesChanged)
            {
                Repaint();
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            showWaveform = EditorGUILayout.Toggle("Show Waveform", showWaveform);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(10);
        
        // Time settings
        startTime = EditorGUILayout.Slider("Start Time (seconds):", startTime, 0f, Mathf.Max(0.01f, sourceClip.length));
        endTime = EditorGUILayout.Slider("End Time (seconds):", endTime, Mathf.Min(startTime + 0.01f, sourceClip.length), sourceClip.length);
        
        // Cut duration info
        EditorGUILayout.LabelField("Cut Duration:", $"{endTime - startTime:F2} seconds");
        
        DrawExportSettings();
        DrawActionButtons();
    }
    
    private void DrawHeader()
    {
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        
        EditorGUILayout.LabelField("Unity Audio Cutter Tool", headerStyle);
        
        GUIStyle subHeaderStyle = new GUIStyle(GUI.skin.label);
        subHeaderStyle.alignment = TextAnchor.MiddleCenter;
        subHeaderStyle.fontSize = 10;
        
        EditorGUILayout.LabelField("Easily cut and save audio files", subHeaderStyle);
        EditorGUILayout.Space(10);
    }
    
    private void DrawDragAndDropArea()
    {
        EditorGUILayout.Space(10);
        
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag and drop audio file here", EditorStyles.helpBox);
        
        // Drag & drop area
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;
                
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        AudioClip clip = draggedObject as AudioClip;
                        if (clip != null)
                        {
                            sourceClip = clip;
                            ResetEditorWithNewClip();
                            break;
                        }
                    }
                }
                evt.Use();
                break;
        }
    }
    
    private void DrawAudioInfo()
    {
        if (sourceClip == null) return;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Audio Name:", sourceClip.name);
        EditorGUILayout.LabelField("Duration:", $"{sourceClip.length:F2} seconds");
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Channels:", sourceClip.channels.ToString());
        EditorGUILayout.LabelField("Frequency:", $"{sourceClip.frequency} Hz");
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
    
    private void DrawExportSettings()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        saveFileName = EditorGUILayout.TextField("File Name:", saveFileName);
        
        // Check file extension
        bool hasValidExtension = false;
        foreach (string format in supportedFormats)
        {
            if (saveFileName.EndsWith(format, System.StringComparison.OrdinalIgnoreCase))
            {
                hasValidExtension = true;
                break;
            }
        }
        
        if (!hasValidExtension)
        {
            saveFileName += ".wav";
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Path:", savePath.Length > 0 ? savePath : "Default");
        
        if (GUILayout.Button("Change Path", GUILayout.Width(100)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Save Folder", savePath, "");
            if (!string.IsNullOrEmpty(path))
            {
                savePath = path;
                EditorPrefs.SetString("AudioCutter_SavePath", savePath);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (!string.IsNullOrEmpty(savePath) && !Directory.Exists(savePath))
        {
            EditorGUILayout.HelpBox("The specified save path does not exist!", MessageType.Warning);
        }
    }
    
    private void DrawActionButtons()
    {
        EditorGUILayout.Space(15);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = sourceClip != null;
        
        if (GUILayout.Button(isPlaying ? "Stop" : "Preview", GUILayout.Height(30)))
        {
            if (isPlaying)
            {
                StopPreview();
            }
            else
            {
                PlayPreview();
            }
        }
        
        if (GUILayout.Button("Cut and Save", GUILayout.Height(30)))
        {
            CutAndSaveAudio();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void ResetEditorWithNewClip()
    {
        if (sourceClip == null) return;
        
        // Reset values
        startTime = 0f;
        endTime = sourceClip.length;
        
        // Suggest file name
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(sourceClip.name);
        saveFileName = $"cut_{nameWithoutExtension}.wav";
        
        // Generate waveform
        waveformVisualizer.GenerateWaveformData(sourceClip);
    }
    
    private void PlayPreview()
    {
        if (sourceClip == null) return;
        
        StopPreview();
        
        // Create preview clip
        previewClip = audioProcessor.CreateCutClip(sourceClip, startTime, endTime);
        if (previewClip != null)
        {
            // Play audio
            AudioUtility.PlayClip(previewClip);
            isPlaying = true;
            
            // Schedule auto-stop
            EditorApplication.delayCall += () => {
                // Use a coroutine-like pattern to wait for clip duration
                float delay = previewClip.length + 0.1f;
                double stopTime = EditorApplication.timeSinceStartup + delay;
                
                EditorApplication.update += WaitForStopTime;
                
                void WaitForStopTime()
                {
                    if (EditorApplication.timeSinceStartup >= stopTime)
                    {
                        StopPreview();
                        EditorApplication.update -= WaitForStopTime;
                    }
                }
            };
        }
    }
    
    private void StopPreview()
    {
        if (isPlaying)
        {
            AudioUtility.StopClip(previewClip);
            isPlaying = false;
            Repaint();
        }
    }
    
    private void CutAndSaveAudio()
    {
        if (sourceClip == null) return;
        
        // Stop playback first
        StopPreview();
        
        // Cut process
        AudioClip cutClip = audioProcessor.CreateCutClip(sourceClip, startTime, endTime);
        if (cutClip == null)
        {
            EditorUtility.DisplayDialog("Error", "Please specify a valid cutting range.", "OK");
            return;
        }
        
        // Determine save path
        string finalPath;
        if (string.IsNullOrEmpty(savePath))
        {
            // Default save path: Assets/Audio
            savePath = Path.Combine(Application.dataPath, "Audio");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }
        
        finalPath = Path.Combine(savePath, saveFileName);
        
        // Save as WAV
        if (audioProcessor.SaveWav(finalPath, cutClip))
        {
            Debug.Log($"Audio successfully saved: {finalPath}");
            
            // Refresh asset database
            AssetDatabase.Refresh();
            
            // Show success message
            EditorUtility.DisplayDialog("Success", $"Audio file saved to:\n{finalPath}", "OK");
            
            // Show file in Explorer/Finder (optional)
            EditorUtility.RevealInFinder(finalPath);
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to save audio file.", "OK");
        }
    }
    
    private void OnDestroy()
    {
        StopPreview();
    }
}