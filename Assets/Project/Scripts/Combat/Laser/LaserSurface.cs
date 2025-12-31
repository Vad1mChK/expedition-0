using NaughtyAttributes;
using UnityEngine;

namespace Expedition0.Combat.Laser
{
    public enum LaserSurfaceType
    {
        // Fully absorb the beam; produce no further bounces. Objects with no LaserSurface component behave the same.
        Absorb,
        // Reflect the beam off the surface with the given normal.
        Reflect,
        // The surface refracts the beam with given ior; total internal reflection is possible sometimes.
        Refract,
        // Analogous to refract with ior = 1; the beam does not change its direction but the intensity may decay.
        Passthrough,
        // The surface lets the beam through and doesn't affect it
        Ignore
    }
    
    public class LaserSurface: MonoBehaviour
    {
        [Tooltip("The way this surface interacts with the laser")]
        [SerializeField] public LaserSurfaceType surfaceType = LaserSurfaceType.Reflect;
        [Tooltip("Intensity of the beam after the bounce. 1 fully preserves the beam and 0 fully consumes it")]
        [Range(0f, 1f)] [SerializeField] public float bounceIntensity = 0.75f;
        [Tooltip("Index of Refraction for refractive surfaces")]
        [ShowIf(nameof(surfaceType), LaserSurfaceType.Refract)]
        [Range(1f, 2f)] [SerializeField] public float ior = 1.5f;
    }
}