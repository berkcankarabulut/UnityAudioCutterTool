using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AudioCutterTool
{
    public class WaveformVisualizer
    {
        private List<float> waveformData = new List<float>();

        // Waveform drawing settings
        private float waveformHeight = 100f;
        private Color waveformColor = new Color(0.3f, 0.65f, 1f);
        private Color selectedAreaColor = new Color(0.3f, 0.85f, 0.3f, 0.3f);

        private GUIStyle timelineLabelStyle = new()
        {
            normal =
            {
                textColor = Color.white
            },
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };

        public void GenerateWaveformData(AudioClip clip)
        {
            if (clip == null) return;

            waveformData.Clear();

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            // Create waveform (simplified representation of audio amplitude)
            int resolution = 1000; // Number of points to draw
            int packSize = Mathf.Max(1, samples.Length / resolution);

            for (int i = 0; i < samples.Length; i += packSize)
            {
                float maxAmp = 0f;
                for (int j = 0; j < packSize && i + j < samples.Length; j++)
                {
                    float amp = Mathf.Abs(samples[i + j]);
                    if (amp > maxAmp) maxAmp = amp;
                }

                waveformData.Add(maxAmp);
            }
        }

        // Draw waveform and handle mouse interaction
        // Returns true if start or end times were changed
        public bool DrawWaveform(AudioClip clip, ref float startTime, ref float endTime)
        {
            if (clip == null || waveformData == null || waveformData.Count == 0)
            {
                if (clip != null) GenerateWaveformData(clip);
                return false;
            }

            bool timeChanged = false;

            // Waveform visualization area
            Rect waveformRect = GUILayoutUtility.GetRect(0, waveformHeight, GUILayout.ExpandWidth(true));
            waveformRect = EditorGUI.IndentedRect(waveformRect);

            // Background
            EditorGUI.DrawRect(waveformRect, new Color(0.1f, 0.1f, 0.1f, 0.8f));

            // Draw selected area
            float startPos = waveformRect.x + (waveformRect.width * (startTime / clip.length));
            float endPos = waveformRect.x + (waveformRect.width * (endTime / clip.length));
            Rect selectedRect = new Rect(startPos, waveformRect.y, endPos - startPos, waveformRect.height);
            EditorGUI.DrawRect(selectedRect, selectedAreaColor);

            // Draw waveform
            int sampleSize = Mathf.Min(waveformData.Count, (int)waveformRect.width * 2);
            int step = Mathf.Max(1, waveformData.Count / sampleSize);

            for (int i = 0; i < sampleSize - 1; i += 2)
            {
                int dataIndex = (i * step) % waveformData.Count;
                float amplitude = waveformData[dataIndex];

                float xPos = waveformRect.x + (i * waveformRect.width / sampleSize);
                float yPos = waveformRect.y + (waveformRect.height / 2);
                float height = amplitude * (waveformRect.height / 2);

                Rect lineRect = new Rect(xPos, yPos - height / 2, 1, height);
                EditorGUI.DrawRect(lineRect, waveformColor);
            }

            // Time markers
            int markers = 10;
            for (int i = 0; i <= markers; i++)
            {
                float time = i * clip.length / markers;
                float xPos = waveformRect.x + (i * waveformRect.width / markers);

                // Vertical line
                Rect markerRect = new Rect(xPos, waveformRect.y, 1, waveformRect.height);
                EditorGUI.DrawRect(markerRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));

                // Time label
                Rect labelRect = new Rect(xPos - 15, waveformRect.y + waveformRect.height - 12, 30, 12);
                GUI.Label(labelRect, $"{time:F1}s", timelineLabelStyle);
            }

            // Start and end markers (red lines)
            Rect startMarker = new Rect(startPos, waveformRect.y, 2, waveformRect.height);
            EditorGUI.DrawRect(startMarker, Color.red);

            Rect endMarker = new Rect(endPos, waveformRect.y, 2, waveformRect.height);
            EditorGUI.DrawRect(endMarker, Color.red);

            // Mouse interaction - adjust start and end points
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown || evt.type == EventType.MouseDrag)
            {
                if (waveformRect.Contains(evt.mousePosition))
                {
                    float time = ((evt.mousePosition.x - waveformRect.x) / waveformRect.width) * clip.length;
                    time = Mathf.Clamp(time, 0, clip.length);

                    // Hold Shift to adjust end point, otherwise adjust start
                    if (evt.shift)
                    {
                        if (endTime != time && time > startTime + 0.01f)
                        {
                            endTime = time;
                            timeChanged = true;
                        }
                    }
                    else
                    {
                        if (startTime != time && time < endTime - 0.01f)
                        {
                            startTime = time;
                            timeChanged = true;
                        }
                    }

                    evt.Use();
                }
            }

            return timeChanged;
        }

        public bool DrawWaveformControls(AudioClip clip, ref float startTime, ref float endTime)
        {
            return DrawWaveform(clip, ref startTime, ref endTime);
        }
    }
}