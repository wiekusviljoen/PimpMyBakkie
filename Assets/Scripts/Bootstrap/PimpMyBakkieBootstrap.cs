using UnityEngine;

namespace PimpMyBakkie
{
    public sealed class PimpMyBakkieBootstrap : MonoBehaviour
    {
        void Start()
        {
            Application.targetFrameRate = 60;
            // No procedural cartoon vehicle is spawned. The product starts with the user's
            // real vehicle photo and only advances to a verified exact-model asset.
            gameObject.AddComponent<PhotoFirstHUD>();
        }
    }
}