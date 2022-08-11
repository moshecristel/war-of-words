using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarOfWords
{
    public class Map
    {
        public int Cols => Letters.GetLength(0);
        public int Rows => Letters.GetLength(1);
        public int TileCount => Cols * Rows;
        
        public int TotalWords { get; set; }
        public int TotalWordLetters { get; set; }
        public float AvgWordsPerTile => (float)TotalWords / TileCount;
        public float AvgWordLettersPerTile => (float)TotalWordLetters / TileCount;

        public MapLetter[,] Letters { get; set; }
        private DictionaryTrie _dictionary;
        
        // Create map from shape
        public Map(bool[,] shape, bool initWeighted = true)
        {
            _dictionary = new DictionaryTrie();
            GenerateLetters(shape, initWeighted);
            MarkLetterAdjacency();
            RefreshLetterStats();
        }

        public Map(List<string> mapData)
        {
            _dictionary = new DictionaryTrie();
            GenerateLettersFromData(mapData);
            MarkLetterAdjacency();
            RefreshLetterStats();
        }

        public void GenerateLettersFromData(List<string> mapData)
        {
            int cols = mapData[0].Length;
            int rows = mapData.Count;

            Letters = new MapLetter[cols, rows];
            for (int y = 0; y < rows; y++)
            {
                char[] rowChars = mapData[y].ToCharArray();
                for (int x = 0; x < cols; x++)
                {
                    if(char.IsLetter(rowChars[x]))
                        Letters[x, y] = new MapLetter(rowChars[x].ToString());
                    else
                        Letters[x, y] = null;
                }
            }
        }

        private void GenerateLetters(bool[,] shape, bool initWeighted)
        {
            Letters = new MapLetter[shape.GetLength(0), shape.GetLength(1)];
            for (int y = 0; y < shape.GetLength(1); y++)
            {
                for (int x = 0; x < shape.GetLength(0); x++)
                {
                    if (shape[x, y])
                    {
                        Letters[x, y] = new MapLetter(CharacterUtils.GetRandomUppercaseAlphaCharacter(initWeighted));
                    }   
                }
            }
        }

        private void MarkLetterAdjacency()
        {
            // Register adjacent letters with each letter
            for (int y = 0; y < Letters.GetLength(1); y++)
            {
                for (int x = 0; x < Letters.GetLength(0); x++)
                {
                    MapLetter mapLetter = Letters[x, y];

                    if (mapLetter == null) continue;
                    
                    mapLetter.Coords = new Vector2Int(x, y);
                    
                    // Can move W
                    if (x > 0) mapLetter.Directions[GridDirection.W] = Letters[x - 1, y];
                    
                    // Can move E
                    if (x < Letters.GetLength(0) - 1) mapLetter.Directions[GridDirection.E] = Letters[x + 1, y];
                    
                    // Can move N
                    if (y > 0) mapLetter.Directions[GridDirection.N] = Letters[x, y - 1];
                    
                    // Can move S
                    if (y < Letters.GetLength(1) - 1) mapLetter.Directions[GridDirection.S] = Letters[x, y + 1];
                    
                    // Can move NW
                    if (y > 0 && x > 0) mapLetter.Directions[GridDirection.NW] = Letters[x - 1, y - 1];
                    
                    // Can move NE
                    if (y > 0 && x < Letters.GetLength(0) - 1) mapLetter.Directions[GridDirection.NE] = Letters[x + 1, y - 1];
                    
                    // Can move SW
                    if (y < Letters.GetLength(1) - 1 && x > 0) mapLetter.Directions[GridDirection.SW] = Letters[x - 1, y + 1];
                    
                    // Can move SE
                    if (y < Letters.GetLength(1) - 1 && x < Letters.GetLength(0) - 1) mapLetter.Directions[GridDirection.SE] = Letters[x + 1, y + 1];

                }
            }
        }

        public void RefreshLetterStats()
        {
            TotalWords = 0;
            TotalWordLetters = 0;
            
            // Fill in stats
            int maxY = Letters.GetLength(1) - 1;
            int maxX = Letters.GetLength(0) - 1;
            
            for (int y = 0; y <= maxY ; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    MapLetter letter = Letters[x, y];
                    if (letter != null)
                    {
                        List<string> words = GetWordsStartingWithLetter(letter);
                        letter.WordStarts = words.Count;
                        letter.WordStartsTotalLetterCount = words.Select(word => word.Length).Sum();

                        TotalWords += letter.WordStarts;
                        TotalWordLetters += letter.WordStartsTotalLetterCount;
                    }
                }
            }
        }

        public List<string> GetWordsStartingWithLetter(MapLetter mapLetter)
        {
            string sequenceSoFar = mapLetter.Character;
            DictionaryNode dictionaryNode = _dictionary.Roots[mapLetter.Character];
            List<Vector2Int> visited = new List<Vector2Int> { mapLetter.Coords };
            List<Vector2Int> allAdjacentCoords = GetAllAdjacentCoords(mapLetter.Coords);

            HashSet<string> allWords = new HashSet<string>();
            
            foreach (Vector2Int coords in allAdjacentCoords)
            {
                List<string> words = GetWordsFrom(coords, visited, dictionaryNode, sequenceSoFar);
                allWords.UnionWith(words);
            }

            return allWords.ToList();
        }
        
        public List<string> GetWordsFrom(Vector2Int adjacentCoords, List<Vector2Int> visited, DictionaryNode parentDictionaryNode, string sequenceSoFar)
        {
            if (sequenceSoFar.Length >= 15 || Letters[adjacentCoords.x, adjacentCoords.y] == null) return new List<string>();
            
            HashSet<string> allWords = new HashSet<string>();
            
            string currCharacter = Letters[adjacentCoords.x, adjacentCoords.y].Character;
            sequenceSoFar += currCharacter;
            
            foreach (KeyValuePair<string,DictionaryNode> keyValuePair in parentDictionaryNode.Children)
            {
                // Only one of the children of the parent dictionary node will match the letter of the adjacent coordinate
                DictionaryNode childDictionaryNode = keyValuePair.Value;
                if (childDictionaryNode.Character == currCharacter)
                {
                    // Sequence so far could be a word
                    if (childDictionaryNode.IsWordEnd)
                    {
                        allWords.Add(sequenceSoFar);
                    }
                    
                    // Either way, recurse to make sure that we're not in the middle of finding a word
                    List<Vector2Int> visitedWithCurrent = new List<Vector2Int>(visited);
                    visitedWithCurrent.Add(adjacentCoords);

                    List<Vector2Int> allAdjacentCoords = GetAllAdjacentCoords(adjacentCoords);
                    foreach (Vector2Int visitedCoords in visitedWithCurrent)
                    {
                        allAdjacentCoords.Remove(visitedCoords);
                    }

                    foreach (Vector2Int coords in allAdjacentCoords)
                    {
                        List<string> words = GetWordsFrom(coords, visitedWithCurrent, childDictionaryNode,
                            sequenceSoFar);
                        allWords.UnionWith(words);
                    }
                }
            }

            return allWords.ToList();
        }

        public List<Vector2Int> GetAllAdjacentCoords(Vector2Int coords)
        {
            List<Vector2Int> adjacentCoords = new List<Vector2Int>();
            
            // W
            if (coords.x > 0) adjacentCoords.Add(new Vector2Int(coords.x - 1, coords.y));
                    
            // E
            if (coords.x < Letters.GetLength(0) - 1) adjacentCoords.Add(new Vector2Int(coords.x + 1, coords.y));
                    
            // Can move N
            if (coords.y > 0) adjacentCoords.Add(new Vector2Int(coords.x, coords.y - 1));
                    
            // Can move S
            if (coords.y < Letters.GetLength(1) - 1) adjacentCoords.Add(new Vector2Int(coords.x, coords.y + 1));
                    
            // Can move NW
            if (coords.y > 0 && coords.x > 0) adjacentCoords.Add(new Vector2Int(coords.x - 1, coords.y - 1));
                    
            // Can move NE
            if (coords.y > 0 && coords.x < Letters.GetLength(0) - 1) adjacentCoords.Add(new Vector2Int(coords.x + 1, coords.y - 1));
                    
            // Can move SW
            if (coords.y < Letters.GetLength(1) - 1 && coords.x > 0) adjacentCoords.Add(new Vector2Int(coords.x - 1, coords.y + 1));
                    
            // Can move SE
            if (coords.y < Letters.GetLength(1) - 1 && coords.x < Letters.GetLength(0) - 1) adjacentCoords.Add(new Vector2Int(coords.x + 1, coords.y + 1));

            return adjacentCoords;
        }

        public void Print()
        {
            string mapString = "";
            for (int y = 0; y < Letters.GetLength(1); y++)
            {
                for (int x = 0; x < Letters.GetLength(0); x++)
                {
                    mapString += Letters[x, y] == null ? "+" : Letters[x, y].Character;
                }

                mapString += "\n";
            }
            
            Debug.Log(mapString);
        }

        public void PrintStats()
        {
            Debug.Log($"TileCount: {TileCount:n0}, TotalWords: {TotalWords:n0}, TotalWordLetters: {TotalWordLetters:n0}, AvgWordsPerTile: {AvgWordsPerTile:n2}, AvgWordLettersPerTile: {AvgWordLettersPerTile:n2}");
        }
    }
}
