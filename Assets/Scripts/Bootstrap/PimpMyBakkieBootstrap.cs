using UnityEngine;
using PimpMyBakkie.Configurator;
using PimpMyBakkie.Prototype;

namespace PimpMyBakkie
{
    public sealed class PimpMyBakkieBootstrap : MonoBehaviour
    {
        void Start()
        {
            Application.targetFrameRate = 60;
            var vehicle = new GameObject("Bakkie_Prototype");
            vehicle.AddComponent<ProceduralBakkie>();
            vehicle.AddComponent<VehicleRotator>();

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "DisplayGround";
            ground.transform.localScale = Vector3.one * 8f;
            var groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMaterial.color = new Color(.18f, .2f, .17f);
            ground.GetComponent<Renderer>().material = groundMaterial;

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 42f;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 200f;
            cameraObject.AddComponent<AudioListener>();

            var keyLightObject = new GameObject("Key Light");
            var keyLight = keyLightObject.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.2f;
            keyLight.transform.rotation = Quaternion.Euler(35f, -35f, 0f);

            var fillLightObject = new GameObject("Fill Light");
            var fillLight = fillLightObject.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = .35f;
            fillLight.transform.rotation = Quaternion.Euler(60f, 145f, 0f);

            gameObject.AddComponent<ConfiguratorHUD>();
        }
    }
}
