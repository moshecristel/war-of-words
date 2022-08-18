using UnityEngine;

namespace WarOfWords
{
    public class TileOwnership
    {
        public Party Party { get; set; }
        public bool IsCurrentPlayer { get; set; }
        public int ClaimCount { get; set; }

        public TileOwnership(Party party, bool isCurrentPlayer = true, int claimCount = 1)
        {
            Party = party;
            IsCurrentPlayer = isCurrentPlayer;
            ClaimCount = claimCount;
        }
    }
}
