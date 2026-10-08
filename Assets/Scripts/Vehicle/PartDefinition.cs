using UnityEngine;
namespace PimpMyBakkie.Vehicle
{
    public enum PartCategory { Wheels, Tyres, Suspension, Bullbar, Lights, Exhaust, Snorkel, Canopy, Colour, Engine }

    [CreateAssetMenu(menuName = "PimpMyBakkie/Part Definition")]
    public sealed class PartDefinition : ScriptableObject
    {
        public string manufacturer;
        public string partName;
        public PartCategory category;
        public float price;
        public GameObject modelPrefab;
        public float powerDeltaKw;
        public float torqueDeltaNm;
        public float massDeltaKg;
        public float clearanceDeltaMm;
        public float fuelConsumptionDeltaPercent;
        public float accelerationDeltaPercent;
    }
}