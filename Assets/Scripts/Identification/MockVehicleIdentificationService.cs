using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PimpMyBakkie.Identification
{
    /// <summary>
    /// Development guard: never invent a vehicle match. Replace with a real vision provider.
    /// </summary>
    public sealed class MockVehicleIdentificationService : IVehicleIdentificationService
    {
        public Task<VehicleCandidate> IdentifyAsync(Texture2D photo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotSupportedException(
                "Vehicle recognition is not connected. Do not guess a make/model; connect a production vision service.");
        }
    }
}