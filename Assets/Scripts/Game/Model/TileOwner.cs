using UnityEngine;

namespace WarOfWords
{
    public class TileOwner
    {
        public Party Party { get; set; }
        public bool IsCurrentPlayer { get; set; }
        public int PointCount { get; set; }

        public TileOwner(Party party, bool isCurrentPlayer = true, int pointCount = 1)
        {
            Party = party;
            IsCurrentPlayer = isCurrentPlayer;
            PointCount = pointCount;
        }
    }
}
