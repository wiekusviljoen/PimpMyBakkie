using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace PimpMyBakkie.Identification
{
    public sealed class MockVehicleIdentificationService : IVehicleIdentificationService
    {
        public async Task<VehicleCandidate> IdentifyAsync(Texture2D photo, CancellationToken cancellationToken)
        {
            await Task.Delay(600, cancellationToken);
            return new VehicleCandidate("Toyota","Hilux",2022,"2.8 GD-6 4x4",0.94f);
        }
    }
}