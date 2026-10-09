using UnityEngine;

namespace PimpMyBakkie.CameraCapture
{
    public sealed class VehiclePhotoCapture : MonoBehaviour
    {
        WebCamTexture webcam;
        public bool IsRunning => webcam != null && webcam.isPlaying;
        public Texture CurrentPreview => webcam;

        public void StartCamera()
        {
            if (IsRunning) return;
            var devices = WebCamTexture.devices;
            if (devices == null || devices.Length == 0)
            {
                Debug.LogWarning("No camera device found.");
                return;
            }

            // Prefer the rear-facing camera on phones so the actual vehicle is photographed.
            var selected = devices[0];
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    selected = devices[i];
                    break;
                }
            }

            webcam = new WebCamTexture(selected.name, 1920, 1080, 30);
            webcam.Play();
        }

        public Texture2D Capture()
        {
            if (!IsRunning || webcam.width <= 16 || webcam.height <= 16 || !webcam.didUpdateThisFrame)
                return null;

            var photo = new Texture2D(webcam.width, webcam.height, TextureFormat.RGB24, false);
            photo.SetPixels(webcam.GetPixels());
            photo.Apply(false, false);
            return photo;
        }

        public void StopCamera()
        {
            if (webcam != null && webcam.isPlaying) webcam.Stop();
        }

        void OnDisable() => StopCamera();
        void OnDestroy() => StopCamera();
    }
}