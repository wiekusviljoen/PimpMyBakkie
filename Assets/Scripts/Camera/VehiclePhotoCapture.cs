using UnityEngine;
namespace PimpMyBakkie.CameraCapture
{
    public sealed class VehiclePhotoCapture : MonoBehaviour
    {
        WebCamTexture webcam;
        public bool IsRunning=>webcam!=null&&webcam.isPlaying;
        public Texture CurrentPreview=>webcam;
        public void StartCamera()
        {
            if(IsRunning)return;
            var d=WebCamTexture.devices; if(d.Length==0){Debug.LogWarning("No camera device found.");return;}
            webcam=new WebCamTexture(d[0].name,1280,720,30); webcam.Play();
        }
        public Texture2D Capture()
        {
            if(!IsRunning)return null;
            var p=new Texture2D(webcam.width,webcam.height,TextureFormat.RGB24,false);
            p.SetPixels(webcam.GetPixels()); p.Apply(); return p;
        }
        void OnDestroy(){if(IsRunning)webcam.Stop();}
    }
}