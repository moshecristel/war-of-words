namespace WarOfWords
{
    public class PerimeterStats
    {
        public int Tiles { get; set; }
        public int ClaimedTiles { get; set; }
        public int Words { get; set; }
        public float AverageWordLength { get; set; }
        public int Points { get; set; }
        public int BonusPoints { get; set; }
        public int BonusCoins { get; set; }
        
        // TODO Capture these
        public int Seconds { get; set; }
        public float SecondsPerTile { get; set; }

        public PerimeterStats() {}

        public override string ToString()
        {
            return $"{nameof(Tiles)}: {Tiles}, {nameof(ClaimedTiles)}: {ClaimedTiles}, {nameof(Words)}: {Words}, {nameof(AverageWordLength)}: {AverageWordLength}, {nameof(Points)}: {Points}, {nameof(BonusPoints)}: {BonusPoints}, {nameof(BonusCoins)}: {BonusCoins}, {nameof(Seconds)}: {Seconds}, {nameof(SecondsPerTile)}: {SecondsPerTile}";
        }
    }
}
