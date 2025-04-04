#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

public class AudioCutterEditorWindow : EditorWindow
{
    private AudioClip sourceClip;
    private float startTime = 0f;
    private float endTime = 10f;
    private string savePath = "";
    private string saveFileName = "cut_audio.wav";
    
    private AudioPreviewHandler previewHandler;
    private WaveformVisualizer waveformVisualizer;
    private AudioExporter audioExporter;
    private UIHandler uiHandler;
    private AudioProcessor audioProcessor;
    
    [MenuItem("Audio/Audio Cutter Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<AudioCutterEditorWindow>("Audio Cutter Tool");
        window.minSize = new Vector2(500, 350);
        window.Show();
    }

    private void OnEnable()
    {
        audioProcessor = new AudioProcessor();
        previewHandler = new AudioPreviewHandler(audioProcessor);
        waveformVisualizer = new WaveformVisualizer();
        audioExporter = new AudioExporter(audioProcessor);
        uiHandler = new UIHandler();
        
        if (EditorPrefs.HasKey("AudioCutter_SavePath"))
        {
            savePath = EditorPrefs.GetString("AudioCutter_SavePath");
        }
    }

    private void OnGUI()
    {
        uiHandler.DrawHeader();
    
        sourceClip = uiHandler.DrawAudioClipField(sourceClip, ResetEditorWithNewClip);
        waveformVisualizer.GenerateWaveformData(sourceClip);
    
        if (sourceClip == null)
        {
            uiHandler.DrawDragAndDropArea(ref sourceClip, ResetEditorWithNewClip);
            return;
        }

        uiHandler.DrawAudioInfo(sourceClip);
    
        if (waveformVisualizer.DrawWaveformControls(sourceClip, ref startTime, ref endTime))
        {
            Repaint();
        }

        uiHandler.DrawTimeControls(sourceClip, ref startTime, ref endTime);
        uiHandler.DrawExportSettings(ref savePath, ref saveFileName);
    
        // Check if audio finished playing
        if (previewHandler.IsPlaying && !previewHandler.IsAudioPlaying())
        {
            previewHandler.IsPlaying = false;
        }
    
        if (uiHandler.DrawActionButtons(sourceClip != null, isPlaying: previewHandler.IsPlaying))
        {
            if (previewHandler.IsPlaying)
            {
                previewHandler.StopPreview();
            }
            else
            {
                previewHandler.PlayPreview(sourceClip, startTime, endTime);
            }
        }

        if (uiHandler.ShouldCutAndSave())
        {
            Debug.Log("Audio Cutter Tool SavePath: " + savePath);
            audioExporter.CutAndSave(sourceClip, startTime, endTime, savePath, saveFileName);
        }
    }

    private void ResetEditorWithNewClip()
    { 
        startTime = 0f;
        endTime = sourceClip.length;
        saveFileName = $"cut_{Path.GetFileNameWithoutExtension(sourceClip.name)}.wav";
        waveformVisualizer.GenerateWaveformData(sourceClip);
    }

    private void OnDestroy()
    {
        previewHandler.CleanUp();
    }
}
#endif