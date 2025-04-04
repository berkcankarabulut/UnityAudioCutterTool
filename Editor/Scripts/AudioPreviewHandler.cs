using UnityEngine;

namespace AudioCutterTool
{
    public class AudioPreviewHandler
    {
        private GameObject tempAudioObject;
        private AudioClip previewClip;
        public bool IsPlaying { get; set; }
        private AudioProcessor audioProcessor;

        public AudioPreviewHandler(AudioProcessor audioProcessor)
        {
            this.audioProcessor = audioProcessor;
        }

        public void PlayPreview(AudioClip sourceClip, float startTime, float endTime)
        {
            StopPreview();

            if (tempAudioObject == null)
            {
                CreateTempAudioObject();
            }

            previewClip = audioProcessor.CreateCutClip(sourceClip, startTime, endTime); // Nesne üzerinden çağırıldı
            if (previewClip != null)
            {
                AudioSource audioSource = tempAudioObject.GetComponent<AudioSource>();
                audioSource.clip = previewClip;
                audioSource.Play();
                IsPlaying = true;
            }
        }

        public void StopPreview()
        {
            if (IsPlaying && tempAudioObject != null)
            {
                AudioSource audioSource = tempAudioObject.GetComponent<AudioSource>();
                if (audioSource != null) audioSource.Stop();
                IsPlaying = false;
            }
        }

        public void CleanUp()
        {
            if (tempAudioObject != null)
            {
                Object.DestroyImmediate(tempAudioObject);
            }
        }

        public bool IsAudioPlaying()
        {
            if (tempAudioObject == null) return false;
            AudioSource audioSource = tempAudioObject.GetComponent<AudioSource>();
            return audioSource != null && audioSource.isPlaying;
        }

        private void CreateTempAudioObject()
        {
            tempAudioObject = new GameObject("TempAudioProcessor");
            tempAudioObject.hideFlags = HideFlags.HideAndDontSave;
            AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
}