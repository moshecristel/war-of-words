using System;
using System.Collections.Generic;

namespace WarOfWords
{
    public static class CharacterUtils
    {
        public const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        
        // Weight characters according to their frequency in the English language
        // https://www3.nd.edu/~busiforc/handouts/cryptography/letterfrequencies.html
        private const int TOTAL_CHARACTER_WEIGHTS = 50968;
        private static readonly List<WeightedCharacter> _weightedCharacters = new List<WeightedCharacter>
        {
            new("E", 5688),
            new("A", 4331),
            new("R", 3864),
            new("I", 3845),
            new("O", 3651),
            new("T", 3543),
            new("N", 3392),
            new("S", 2923),
            new("L", 2798),
            new("C", 2313),
            new("U", 1851),
            new("D", 1725),
            new("P", 1614),
            new("M", 1536),
            new("H", 1531),
            new("G", 1259),
            new("B", 1056),
            new("F", 924),
            new("Y", 906),
            new("W", 657),
            new("K", 561),
            new("V", 513),
            new("X", 148),
            new("Z", 139),
            new("J", 100),
            new("Q", 100)
        };
        
        public static string GetRandomUppercaseAlphaCharacter(bool weightCharactersByFrequency)
        {
            Random random = new Random();

            // If we want the puzzle to be harder, we may just select from letters at random,
            // not based on natural frequency
            if (!weightCharactersByFrequency)
            {
                int alphaIndex = random.Next(0, ALPHABET.Length - 1);
                return ALPHABET.Substring(alphaIndex, 1);
            }
            
            int r = random.Next(0, TOTAL_CHARACTER_WEIGHTS);
            int currThreshold = 0;
            foreach (WeightedCharacter weightedCharacter in _weightedCharacters)
            {
                if (r <= currThreshold + weightedCharacter.Weight)
                {
                    return weightedCharacter.Character;
                }

                currThreshold += weightedCharacter.Weight;
            }

            // Shouldn't get here
            return "E";
        }
    }

    public class WeightedCharacter
    {
        public string Character { get; set; }
        public int Weight { get; set; }

        public WeightedCharacter(string character, int weight)
        {
            Character = character;
            Weight = weight;
        }
    }
}
