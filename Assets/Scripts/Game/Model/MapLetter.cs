using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    public class MapLetter
    {
        public string Character { get; set; }
        public Vector2Int Coords { get; set; }
        
        public int WordsStartingCount { get; set; }
        public int WordsEndingCount { get; set; }
        public int WordCount => WordsStartingCount + WordsEndingCount;
        
        public Dictionary<GridDirection, MapLetter> Directions { get; set; }

        public MapLetter(string character)
        {
            Character = character;
            Directions = new Dictionary<GridDirection, MapLetter>();
        }
    }
}
