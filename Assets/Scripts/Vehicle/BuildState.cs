using System.Collections.Generic;
using UnityEngine;
namespace PimpMyBakkie.Vehicle
{
    [System.Serializable]
    public struct BuildStats
    {
        public float powerKw, torqueNm, massKg, groundClearanceMm;
        public float fuelConsumptionPercent, accelerationPercent, totalCost;
    }

    public sealed class BuildState
    {
        public VehicleDefinition Vehicle { get; }
        public List<PartDefinition> Parts { get; } = new();
        public BuildState(VehicleDefinition vehicle) => Vehicle = vehicle;
        public void AddPart(PartDefinition part) { if (part != null && !Parts.Contains(part)) Parts.Add(part); }
        public void RemovePart(PartDefinition part) => Parts.Remove(part);

        public BuildStats CalculateStats()
        {
            var s = new BuildStats {
                powerKw = Vehicle.powerKw, torqueNm = Vehicle.torqueNm,
                massKg = Vehicle.massKg, groundClearanceMm = Vehicle.groundClearanceMm
            };
            foreach (var p in Parts) {
                s.powerKw += p.powerDeltaKw; s.torqueNm += p.torqueDeltaNm;
                s.massKg += p.massDeltaKg; s.groundClearanceMm += p.clearanceDeltaMm;
                s.fuelConsumptionPercent += p.fuelConsumptionDeltaPercent;
                s.accelerationPercent += p.accelerationDeltaPercent; s.totalCost += p.price;
            }
            return s;
        }
    }
}