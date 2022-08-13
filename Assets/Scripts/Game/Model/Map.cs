using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarOfWords
{
    public class Map
    {
        public State State { get; set; }
        
        public int Cols => Letters.GetLength(0);
        public int Rows => Letters.GetLength(1);
        public int TileCount => Cols * Rows;
        
        public int TotalWords { get; set; }
        public int TotalWordLetters { get; set; }
        public float AvgWordsPerTile => (float)TotalWords / TileCount;
        public float AvgWordLettersPerTile => (float)TotalWordLetters / TileCount;

        // Coordinates begin in LOWER LEFT of map
        public MapLetter[,] Letters { get; set; }
        private DictionaryTrie _dictionary;
        
        // Create map from shape
        public Map(State state, bool[,] shape, bool initWeighted = true)
        {
            State = state;
            _dictionary = new DictionaryTrie();
            GenerateLetters(shape, initWeighted);
            MarkLetterAdjacency();
            RefreshLetterStats();
        }

        public Map(State state, List<string> mapData)
        {
            State = state;
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
                        Letters[x, rows - 1 - y] = new MapLetter(rowChars[x].ToString());
                    else
                        Letters[x, rows - 1 - y] = null;
                }
            }
        }

        private void GenerateLetters(bool[,] shape, bool initWeighted)
        {
            int cols = shape.GetLength(0);
            int rows = shape.GetLength(1);
            
            Letters = new MapLetter[cols, rows];
            for (int y = 0; y < shape.GetLength(1); y++)
            {
                for (int x = 0; x < shape.GetLength(0); x++)
                {
                    if (shape[x, y])
                    {
                        Letters[x, rows - 1 - y] = new MapLetter(CharacterUtils.GetRandomUppercaseAlphaCharacter(initWeighted));
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
                    
                    bool canMoveN = y < Letters.GetLength(1) - 1;
                    bool canMoveS = y > 0;
                    bool canMoveE = x < Letters.GetLength(0) - 1;
                    bool canMoveW = x > 0;
                    bool canMoveNE = canMoveN && canMoveE;
                    bool canMoveNW = canMoveN && canMoveW;
                    bool canMoveSE = canMoveS && canMoveE;
                    bool canMoveSW = canMoveS && canMoveW;
                    
                    if (canMoveN) mapLetter.Directions[GridDirection.N] = Letters[x, y + 1];
                    if (canMoveS) mapLetter.Directions[GridDirection.S] = Letters[x, y - 1];
                    if (canMoveW) mapLetter.Directions[GridDirection.W] = Letters[x - 1, y];
                    if (canMoveE) mapLetter.Directions[GridDirection.E] = Letters[x + 1, y];
                    if (canMoveNE) mapLetter.Directions[GridDirection.NE] = Letters[x + 1, y + 1];
                    if (canMoveNW) mapLetter.Directions[GridDirection.NW] = Letters[x - 1, y + 1];
                    if (canMoveSE) mapLetter.Directions[GridDirection.NW] = Letters[x + 1, y - 1];
                    if (canMoveSW) mapLetter.Directions[GridDirection.NW] = Letters[x - 1, y - 1];
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

            bool canMoveN = coords.y < Letters.GetLength(1) - 1;
            bool canMoveS = coords.y > 0;
            bool canMoveE = coords.x < Letters.GetLength(0) - 1;
            bool canMoveW = coords.x > 0;
            bool canMoveNE = canMoveN && canMoveE;
            bool canMoveNW = canMoveN && canMoveW;
            bool canMoveSE = canMoveS && canMoveE;
            bool canMoveSW = canMoveS && canMoveW;
            
            if(canMoveN) adjacentCoords.Add(new Vector2Int(coords.x, coords.y + 1));
            if(canMoveS) adjacentCoords.Add(new Vector2Int(coords.x, coords.y - 1));
            if(canMoveE) adjacentCoords.Add(new Vector2Int(coords.x + 1, coords.y));
            if(canMoveW) adjacentCoords.Add(new Vector2Int(coords.x - 1, coords.y));
            if(canMoveNE) adjacentCoords.Add(new Vector2Int(coords.x + 1, coords.y + 1));
            if(canMoveNW) adjacentCoords.Add(new Vector2Int(coords.x - 1, coords.y + 1));
            if(canMoveSE) adjacentCoords.Add(new Vector2Int(coords.x + 1, coords.y - 1));
            if(canMoveSW) adjacentCoords.Add(new Vector2Int(coords.x - 1, coords.y - 1));

            return adjacentCoords;
        }

        public void Print()
        {
            string mapString = "";
            for (int y = Letters.GetLength(1) - 1; y >= 0; y--)
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