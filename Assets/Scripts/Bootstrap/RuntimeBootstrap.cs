using UnityEngine;
using PimpMyBakkie.Configurator;
using PimpMyBakkie.Prototype;

namespace PimpMyBakkie
{
    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            if (Object.FindFirstObjectByType<PimpMyBakkieBootstrap>() != null) return;
            var root=new GameObject("PimpMyBakkie_Runtime");
            root.AddComponent<PimpMyBakkieBootstrap>();
        }
    }
}