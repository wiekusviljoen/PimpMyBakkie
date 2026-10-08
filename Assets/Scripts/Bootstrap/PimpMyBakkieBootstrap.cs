using UnityEngine;
using PimpMyBakkie.Configurator;
using PimpMyBakkie.Prototype;
namespace PimpMyBakkie
{
    public sealed class PimpMyBakkieBootstrap : MonoBehaviour
    {
        void Start()
        {
            Application.targetFrameRate=60;
            var vehicle=new GameObject("Bakkie_Prototype");
            vehicle.AddComponent<ProceduralBakkie>();
            vehicle.AddComponent<VehicleRotator>();
            var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name="DisplayGround"; ground.transform.localScale=Vector3.one*8f;
            var co=new GameObject("Main Camera"); co.tag="MainCamera";
            var cam=co.AddComponent<Camera>(); cam.fieldOfView=42f; co.AddComponent<AudioListener>();
            var lo=new GameObject("Key Light"); var l=lo.AddComponent<Light>();
            l.type=LightType.Directional; l.intensity=1.2f; l.transform.rotation=Quaternion.Euler(35,-35,0);
            var fo=new GameObject("Fill Light"); var f=fo.AddComponent<Light>();
            f.type=LightType.Directional; f.intensity=.35f; f.transform.rotation=Quaternion.Euler(60,145,0);
        }
    }
}