using UnityEngine;
using System.IO;
using System;

namespace AudioCutterTool
{
    public class AudioProcessor
    { 
        public AudioClip CreateCutClip(AudioClip sourceClip, float startTime, float endTime)
        {
            if (sourceClip == null) return null;
 
            int startSample = Mathf.FloorToInt(startTime * sourceClip.frequency) * sourceClip.channels;
            int endSample = Mathf.FloorToInt(endTime * sourceClip.frequency) * sourceClip.channels;
            int clipLength = endSample - startSample;

            if (clipLength <= 0) return null;
 
            float[] sourceData = new float[sourceClip.samples * sourceClip.channels];
            sourceClip.GetData(sourceData, 0);
 
            float[] newData = new float[clipLength];
            for (int i = 0; i < clipLength; i++)
            {
                if (startSample + i < sourceData.Length)
                {
                    newData[i] = sourceData[startSample + i];
                }
            } 
            
            AudioClip cutClip = AudioClip.Create(
                "Cut_" + sourceClip.name,
                clipLength / sourceClip.channels,
                sourceClip.channels,
                sourceClip.frequency,
                false
            );

            cutClip.SetData(newData, 0);
            return cutClip;
        }
 
        public bool SaveWav(string filepath, AudioClip clip)
        {
            try
            { 
                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    int samples = clip.samples;
                    int channels = clip.channels;
                    int hz = clip.frequency;
 
                    float[] soundData = new float[samples * channels];
                    clip.GetData(soundData, 0);
 
                    short[] intData = new short[samples * channels];
                    for (int i = 0; i < soundData.Length; i++)
                    {
                        intData[i] = (short)(soundData[i] * 32767f);
                    }
 
                    WriteWavHeader(fileStream, clip);
                    WriteWavData(fileStream, intData);
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"WAV save error: {e.Message}");
                return false;
            }
        }

        private void WriteWavHeader(FileStream stream, AudioClip clip)
        {
            var hz = clip.frequency;
            var channels = clip.channels;
            var samples = clip.samples;

            byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            stream.Write(riff, 0, 4);

            byte[] chunkSize = BitConverter.GetBytes(36 + samples * channels * 2);
            stream.Write(chunkSize, 0, 4);

            byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            stream.Write(wave, 0, 4);

            byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            stream.Write(fmt, 0, 4);

            byte[] subChunk1 = BitConverter.GetBytes(16);
            stream.Write(subChunk1, 0, 4);

            ushort audioFormat = 1; 
            stream.Write(BitConverter.GetBytes(audioFormat), 0, 2);

            ushort numChannels = (ushort)channels;
            stream.Write(BitConverter.GetBytes(numChannels), 0, 2);

            stream.Write(BitConverter.GetBytes(hz), 0, 4);

            stream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);

            ushort blockAlign = (ushort)(channels * 2);
            stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            ushort bitsPerSample = 16;
            stream.Write(BitConverter.GetBytes(bitsPerSample), 0, 2);

            byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
            stream.Write(dataString, 0, 4);

            byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
            stream.Write(subChunk2, 0, 4);
        }

        private void WriteWavData(FileStream stream, short[] data)
        {
            byte[] buffer = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                byte[] sample = BitConverter.GetBytes(data[i]);
                buffer[i * 2] = sample[0];
                buffer[i * 2 + 1] = sample[1];
            }

            stream.Write(buffer, 0, buffer.Length);
        }
    }
}