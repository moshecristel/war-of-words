using UnityEngine;

namespace WarOfWords
{
    public static class VectorUtils
    {
        public static Vector2 ClampPointToBounds(Vector3 point, Bounds bounds)
        {
            float x = Mathf.Clamp(point.x, bounds.min.x, bounds.max.x);
            float y = Mathf.Clamp(point.y, bounds.min.y, bounds.max.y);
            return new Vector2(x, y);
        }

        public static Bounds ContractBounds(Bounds bounds, float contractDistanceX, float contractDistanceY)
        {
            return new Bounds(bounds.center, (Vector2)bounds.size + new Vector2(-contractDistanceX, -contractDistanceY));
        }
    }
}
