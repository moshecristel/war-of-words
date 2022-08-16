using UnityEngine;

namespace WarOfWords
{
    public static class BoundsExtensions
    {
        public static bool ContainBounds(this Bounds bounds, Bounds target)
        {
            return bounds.Contains(target.min) && bounds.Contains(target.max);
        }
    }
}
