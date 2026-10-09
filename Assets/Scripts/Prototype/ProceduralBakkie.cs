using System.Collections.Generic;
using UnityEngine;

namespace PimpMyBakkie.Prototype
{
    public sealed class ProceduralBakkie : MonoBehaviour
    {
        Material body, dark, glass, metal, lamp, red;
        readonly Dictionary<string, GameObject> upgrades = new();

        void Start()
        {
            body = Mat(new Color(.12f, .16f, .14f), .45f, .3f);
            dark = Mat(new Color(.025f, .03f, .03f), .25f, .45f);
            glass = Mat(new Color(.035f, .11f, .13f), .12f, .55f);
            metal = Mat(new Color(.32f, .35f, .33f), .7f, .3f);
            lamp = Mat(new Color(.92f, .82f, .56f), .15f, .25f, true);
            red = Mat(new Color(.55f, .035f, .025f), .25f, .3f, true);
            Build();
        }

        void Build()
        {
            Cube("Chassis", new Vector3(0, .9f, 0), new Vector3(3.8f, .75f, 7f), body);
            Cube("Bonnet", new Vector3(0, 1.35f, -2f), new Vector3(3.5f, .35f, 1.6f), body);
            Cube("Cab", new Vector3(0, 1.65f, .55f), new Vector3(3.45f, 1.25f, 3.25f), body);
            Cube("Windscreen", new Vector3(0, 2.05f, -1.12f), new Vector3(2.85f, .78f, .055f), glass);
            Cube("RearWindow", new Vector3(0, 2.04f, 2.18f), new Vector3(2.45f, .72f, .05f), glass);

            for (int side = -1; side <= 1; side += 2)
            {
                Cube("SideWindow", new Vector3(side * 1.735f, 2.04f, .5f), new Vector3(.045f, .7f, 1.28f), glass);
                Cube("RearSideWindow", new Vector3(side * 1.735f, 2.04f, 1.55f), new Vector3(.045f, .7f, .5f), glass);
                Cube("MirrorArm", new Vector3(side * 1.93f, 1.8f, -.82f), new Vector3(.22f, .08f, .12f), dark);
                Cube("Mirror", new Vector3(side * 2.02f, 1.88f, -.82f), new Vector3(.16f, .25f, .28f), dark);
            }

            Cube("LoadBedFloor", new Vector3(0, 1.3f, 2.65f), new Vector3(3.55f, .12f, 2.15f), dark);
            Cube("BedLeftRail", new Vector3(-1.72f, 1.62f, 2.65f), new Vector3(.12f, .62f, 2.15f), body);
            Cube("BedRightRail", new Vector3(1.72f, 1.62f, 2.65f), new Vector3(.12f, .62f, 2.15f), body);
            Cube("Tailgate", new Vector3(0, 1.62f, 3.68f), new Vector3(3.35f, .62f, .12f), body);
            Cube("FrontGrille", new Vector3(0, 1.22f, -3.57f), new Vector3(1.45f, .34f, .1f), dark);
            Cube("FrontBumper", new Vector3(0, .72f, -3.62f), new Vector3(3.75f, .25f, .22f), metal);
            Cube("RearBumper", new Vector3(0, .72f, 3.58f), new Vector3(3.75f, .25f, .22f), metal);

            for (int side = -1; side <= 1; side += 2)
            {
                Cube("Headlamp", new Vector3(side * 1.35f, 1.38f, -3.57f), new Vector3(.55f, .3f, .08f), lamp);
                Cube("TailLamp", new Vector3(side * 1.55f, 1.52f, 3.73f), new Vector3(.22f, .4f, .08f), red);
                Wheel(new Vector3(side * 1.9f, .75f, -2.15f));
                Wheel(new Vector3(side * 1.9f, .75f, 2.15f));
            }
        }

        public void SetUpgrade(string key, bool enabled)
        {
            if (upgrades.TryGetValue(key, out var existing))
            {
                if (existing != null) Destroy(existing);
                upgrades.Remove(key);
            }

            if (key == "Lift")
                transform.localPosition = enabled ? new Vector3(0, .25f, 0) : Vector3.zero;
            if (!enabled) return;

            var root = new GameObject("Upgrade_" + key);
            root.transform.SetParent(transform, false);
            upgrades[key] = root;

            switch (key)
            {
                case "Bullbar":
                    Part(root.transform, "BullbarTop", new Vector3(0, 1.08f, -3.83f), new Vector3(3.9f, .12f, .16f), metal);
                    Part(root.transform, "BullbarLower", new Vector3(0, .56f, -3.83f), new Vector3(3.7f, .12f, .16f), metal);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        Part(root.transform, "BullbarWing", new Vector3(s * 1.82f, .82f, -3.76f), new Vector3(.12f, .65f, .18f), metal);
                        Part(root.transform, "BullbarSupport", new Vector3(s * 1.15f, .82f, -3.72f), new Vector3(.12f, .5f, .18f), metal);
                    }
                    break;
                case "Snorkel":
                    Part(root.transform, "SnorkelTube", new Vector3(-1.72f, 2.0f, -1.2f), new Vector3(.13f, 1.7f, .13f), dark);
                    Part(root.transform, "SnorkelHead", new Vector3(-1.72f, 2.88f, -1.2f), new Vector3(.24f, .14f, .22f), dark);
                    break;
                case "Canopy":
                    Part(root.transform, "CanopyTop", new Vector3(0, 2.05f, 2.65f), new Vector3(3.5f, .18f, 2.2f), body);
                    for (int s = -1; s <= 1; s += 2)
                        Part(root.transform, "CanopySide", new Vector3(s * 1.68f, 1.72f, 2.65f), new Vector3(.12f, .65f, 2.1f), body);
                    Part(root.transform, "CanopyRearGlass", new Vector3(0, 1.72f, 3.72f), new Vector3(2.8f, .55f, .05f), glass);
                    break;
                case "Lights":
                    for (int s = -1; s <= 1; s += 2)
                        Part(root.transform, "SpotLight", new Vector3(s * .7f, 1.42f, -3.9f), new Vector3(.32f, .32f, .22f), lamp);
                    break;
            }
        }

        void Wheel(Vector3 position)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "AllTerrainTyre";
            wheel.transform.SetParent(transform, false);
            wheel.transform.localPosition = position;
            wheel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            wheel.transform.localScale = new Vector3(.88f, .35f, .88f);
            wheel.GetComponent<Renderer>().material = dark;

            var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "WheelHub";
            hub.transform.SetParent(wheel.transform, false);
            hub.transform.localPosition = new Vector3(0, 0, .52f);
            hub.transform.localRotation = Quaternion.identity;
            hub.transform.localScale = new Vector3(.48f, .28f, .48f);
            hub.GetComponent<Renderer>().material = metal;
        }

        void Cube(string name, Vector3 position, Vector3 scale, Material material)
        {
            Part(transform, name, position, scale, material);
        }

        void Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
        }

        Material Mat(Color color, float metallic, float smoothness, bool emission = false)
        {
            // This project currently uses Unity's Built-in render pipeline. Prefer Standard
            // so runtime-generated geometry does not render magenta. URP remains a fallback.
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .3f);
            }
            return material;
        }
    }
}
