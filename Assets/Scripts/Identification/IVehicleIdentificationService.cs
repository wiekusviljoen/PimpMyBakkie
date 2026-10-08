using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace PimpMyBakkie.Identification
{
    public interface IVehicleIdentificationService
    {
        Task<VehicleCandidate> IdentifyAsync(Texture2D photo, CancellationToken cancellationToken);
    }
}