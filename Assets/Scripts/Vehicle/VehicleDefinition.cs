using UnityEngine;
namespace PimpMyBakkie.Vehicle
{
    [CreateAssetMenu(menuName = "PimpMyBakkie/Vehicle Definition")]
    public sealed class VehicleDefinition : ScriptableObject
    {
        public string make = "Toyota";
        public string model = "Hilux";
        public int year = 2022;
        public string variant = "2.8 GD-6 4x4";
        public float powerKw = 150f;
        public float torqueNm = 500f;
        public float massKg = 2100f;
        public float groundClearanceMm = 286f;
        public string DisplayName => $"{year} {make} {model} {variant}";
    }
}