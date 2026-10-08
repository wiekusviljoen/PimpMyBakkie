using UnityEngine;
using UnityEngine.InputSystem;
namespace PimpMyBakkie.Configurator
{
    public sealed class VehicleRotator : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float rotationSpeed=.18f, zoomSpeed=.012f, minDistance=2.5f, maxDistance=8f;
        Camera cam; float distance=5f;
        void Awake(){ cam=Camera.main; if(target==null) target=transform; }
        void Update(){ Touch(); Mouse(); CameraUpdate(); }
        void Touch()
        {
            var t=Touchscreen.current; if(t==null)return;
            if(t.touches.Count>=1 && t.touches[0].press.isPressed)
                target.Rotate(Vector3.up,-t.touches[0].delta.ReadValue().x*rotationSpeed,Space.World);
            if(t.touches.Count>=2 && t.touches[0].press.isPressed && t.touches[1].press.isPressed)
            {
                var a=t.touches[0].position.ReadValue(); var b=t.touches[1].position.ReadValue();
                var pa=a-t.touches[0].delta.ReadValue(); var pb=b-t.touches[1].delta.ReadValue();
                distance-= (Vector2.Distance(a,b)-Vector2.Distance(pa,pb))*zoomSpeed;
                distance=Mathf.Clamp(distance,minDistance,maxDistance);
            }
        }
        void Mouse()
        {
            var m=Mouse.current; if(m==null)return;
            if(m.leftButton.isPressed) target.Rotate(Vector3.up,-m.delta.ReadValue().x*rotationSpeed,Space.World);
            distance-=m.scroll.ReadValue().y*.01f;
            distance=Mathf.Clamp(distance,minDistance,maxDistance);
        }
        void CameraUpdate()
        {
            if(cam==null)return;
            var focus=target.position+Vector3.up*.8f;
            cam.transform.position=focus+Quaternion.Euler(12f,target.eulerAngles.y+155f,0f)*Vector3.forward*distance;
            cam.transform.LookAt(focus);
        }
    }
}