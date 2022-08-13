using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WarOfWords
{
    public class CoordUtils
    {
        public static bool AreAdjacent(Vector2Int coords1, Vector2Int coords2)
        {
            return coords1 != coords2 &&
                   Math.Abs(coords1.x - coords2.x) <= 1 &&
                   Math.Abs(coords1.y - coords2.y) <= 1;
        }

        public static GridDirection GetRelativeAdjacentGridDirection(Vector2Int fromCoords, Vector2Int toCoords)
        {
            Debug.Log("Getting relative adj dir for from=" + fromCoords + ", to=" + toCoords);
            
            if (!AreAdjacent(fromCoords, toCoords)) return GridDirection.None;
            int relX = toCoords.x - fromCoords.x;
            int relY = toCoords.y - fromCoords.y;

            if (relX == 0)
            {
                // No Horizontal
                if (relY > 0) return GridDirection.N;
                return GridDirection.S;
            }

            if (relX == 1)
            {
                // E
                if (relY == 0) return GridDirection.E;
                if (relY == 1) return GridDirection.NE;
                return GridDirection.SE;
            }

            if (relX == -1)
            {
                // W
                if (relY == 0) return GridDirection.W;
                if (relY == 1) return GridDirection.NW;
                return GridDirection.SW;
            }

            throw new RuntimeWrappedException($"No relative adjacent grid direction found for {fromCoords} and {toCoords}");
        }

        public static GridDirection GetOpposingDirection(GridDirection direction)
        {
            return direction switch
            {
                GridDirection.None => GridDirection.None,
                GridDirection.N => GridDirection.S,
                GridDirection.NE => GridDirection.SW,
                GridDirection.E => GridDirection.W,
                GridDirection.SE => GridDirection.NW,
                GridDirection.S => GridDirection.N,
                GridDirection.SW => GridDirection.NE,
                GridDirection.W => GridDirection.E,
                GridDirection.NW => GridDirection.SE,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}
