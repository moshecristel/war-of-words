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
        public Dictionary<Vector2Int, string> CoordToCityName;

        public DictionaryTrie Dictionary { get; }
        private Dictionary<Vector2Int, List<MapLetterSequence>> _coordToForwardWords = new();
        private Dictionary<Vector2Int, List<MapLetterSequence>> _coordToBackwardWords = new();


        // Create map from shape
        public Map(State state, short[,] shape, Dictionary<Vector2Int, string> coordToCityName, bool initWeighted = true)
        {
            State = state;
            Dictionary = new DictionaryTrie();
            CoordToCityName = coordToCityName;
            GenerateLetters(shape, initWeighted);
            MarkLetterAdjacency();
            RefreshMappingsAndStats();
        }

        public Map(State state, List<string> mapData)
        {
            State = state;
            Dictionary = new DictionaryTrie();
            GenerateLettersFromData(mapData);
            MarkLetterAdjacency();
            RefreshMappingsAndStats();
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

        private void GenerateLetters(short[,] shape, bool initWeighted)
        {
            int cols = shape.GetLength(0);
            int rows = shape.GetLength(1);
            
            Letters = new MapLetter[cols, rows];
            for (int y = 0; y < shape.GetLength(1); y++)
            {
                for (int x = 0; x < shape.GetLength(0); x++)
                {
                    if (shape[x, y] == 0)
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

        public void RefreshMappingsAndStats()
        {
            _coordToForwardWords.Clear();
            _coordToBackwardWords.Clear();
            
            // Fill in stats
            int maxY = Letters.GetLength(1) - 1;
            int maxX = Letters.GetLength(0) - 1;

            // Zero-out letter fields
            
            TotalWords = 0;
            TotalWordLetters = 0;
            
            for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    MapLetter letter = Letters[x, y];
                    if (letter == null) continue;
                    letter.WordsStartingCount = 0;
                    letter.WordsEndingCount = 0;
                }
            }

            for (int y = 0; y <= maxY ; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    MapLetter letter = Letters[x, y];
                    if (letter == null) continue;
                    
                    List<MapLetterSequence> sequences = GetWordLetterSequencesStartingWithLetter(letter).Where(sequence => sequence.Length >= 3).ToList();

                    // Forward
                    _coordToForwardWords[letter.Coords] = sequences;
                    letter.WordsStartingCount += sequences.Count;

                    TotalWords += sequences.Count;
                    TotalWordLetters += sequences.Sum(sequence => sequence.Length);
                        
                    // Backwards
                    foreach (MapLetterSequence sequence in sequences)
                    {
                        Vector2Int endLetterCoords = sequence.EndLetter.Coords;
                        if (!_coordToBackwardWords.ContainsKey(endLetterCoords))
                        {
                            _coordToBackwardWords[endLetterCoords] = new List<MapLetterSequence>();
                        }
                        _coordToBackwardWords[endLetterCoords].Add(sequence);
                        Letters[endLetterCoords.x, endLetterCoords.y].WordsEndingCount++;
                    }
                }
            }
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
        
        #region Get Words

            public List<string> GetWordsStartingWithLetter(MapLetter mapLetter)
            {
                string sequenceSoFar = mapLetter.Character;
                DictionaryNode dictionaryNode = Dictionary.Roots[mapLetter.Character];
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
            
            public List<MapOrderedLetterSequence> GetConnectedWordLetterSequenceBetween(MapLetter startLetter, MapLetter endLetter, List<MapLetter> allIntraversableLetters, bool allowPartial = false, int maxDepth = 2, float directionDeviationDifferencePerSequence = 0f)
            {
                // Limit letters (?)
                int maxLetters = 7;
                    
                List<MapLetter> middleIntraversableLetters = allIntraversableLetters
                    .Where(letter => letter != startLetter && letter != endLetter).ToList();

                float distanceBetweenStartAndEnd = Vector2Int.Distance(startLetter.Coords, endLetter.Coords);
                   
                // FORWARD Finish Attempt 
                List<MapLetterSequence> forwardSequencesAtStart = _coordToForwardWords.ContainsKey(startLetter.Coords) ? _coordToForwardWords[startLetter.Coords].Where(sequence => !sequence.ContainsAny(middleIntraversableLetters) && sequence.Length <= maxLetters).ToList() : new();
                
                // We want to allow the start and end letters but NOT in the non-terminal position
                forwardSequencesAtStart = forwardSequencesAtStart.Where(sequence => !ContainsLetterNonStart(sequence, startLetter) && !ContainsLetterNonEnd(sequence, endLetter)).ToList();
                
                forwardSequencesAtStart = forwardSequencesAtStart.Where(sequence =>
                    Vector2Int.Distance(sequence.EndLetter.Coords, endLetter.Coords) <
                    distanceBetweenStartAndEnd + directionDeviationDifferencePerSequence).ToList();

                // Check to see if any joins to the end as is
                MapLetterSequence finishingForwardSequence = forwardSequencesAtStart
                    .OrderByDescending(sequence => sequence.Length)
                    .FirstOrDefault(sequence => sequence.EndLetter.Coords == endLetter.Coords);
                    
                if (finishingForwardSequence != default(MapLetterSequence))
                {
                    // Able to complete connection (with 1 forward word)
                    return new List<MapOrderedLetterSequence>
                        { new(finishingForwardSequence, false) };
                }
                    
                // BACKWARD Finish Attempt
                List<MapLetterSequence> backwardSequencesAtStart = _coordToBackwardWords.ContainsKey(startLetter.Coords) ? _coordToBackwardWords[startLetter.Coords].Where(sequence => !sequence.ContainsAny(middleIntraversableLetters) && sequence.Length <= maxLetters).ToList() : new();
                
                // We want to allow the start and end letters but NOT in the non-terminal position
                backwardSequencesAtStart = backwardSequencesAtStart.Where(sequence => !ContainsLetterNonStart(sequence, endLetter) && !ContainsLetterNonEnd(sequence, startLetter)).ToList();
                
                backwardSequencesAtStart = backwardSequencesAtStart.Where(sequence =>
                    Vector2Int.Distance(sequence.StartLetter.Coords, endLetter.Coords) <
                    distanceBetweenStartAndEnd + directionDeviationDifferencePerSequence).ToList();

                // Check to see if any joins to the end as is
                MapLetterSequence finishingBackwardSequence = backwardSequencesAtStart
                    .OrderByDescending(sequence => sequence.Length)
                    .FirstOrDefault(sequence => sequence.StartLetter.Coords == endLetter.Coords);
                    
                if (finishingBackwardSequence != default(MapLetterSequence))
                {
                    // Able to complete connection (with 1 forward word)
                    return new List<MapOrderedLetterSequence>
                        { new(finishingBackwardSequence, true) };
                }

                // If max depth is reached, return default OR the closest sequence if allowPartial == true
                if (maxDepth <= 0)
                {
                    if (!allowPartial) return default;
                        
                    // returnPartial is true so return the best that we have 
                    MapLetterSequence nonFinishingForwardSequence = forwardSequencesAtStart
                        .OrderByDescending(sequence => sequence.Length).FirstOrDefault();
                        
                    MapLetterSequence nonFinishingBackwardSequence = backwardSequencesAtStart
                        .OrderByDescending(sequence => sequence.Length).FirstOrDefault();

                    // Don't want to return a list with a default sequence encapsulated (ie. list needs to be the default reference)
                    if (nonFinishingForwardSequence == default && nonFinishingBackwardSequence == default)
                        return default;

                    List<MapOrderedLetterSequence> bestSequenceAsList = new();
                    if (nonFinishingForwardSequence != default && nonFinishingBackwardSequence == default)
                    {
                        bestSequenceAsList.Add(new MapOrderedLetterSequence(nonFinishingForwardSequence, false));
                    }
                    else if (nonFinishingForwardSequence == default && nonFinishingBackwardSequence != default)
                    {
                        bestSequenceAsList.Add(new MapOrderedLetterSequence(nonFinishingBackwardSequence, true));
                    }
                    else
                    {
                        float forwardSequenceDistanceToEnd = Vector2Int.Distance(nonFinishingForwardSequence.EndLetter.Coords, endLetter.Coords);
                        float backwardSequenceDistanceToEnd = Vector2Int.Distance(nonFinishingBackwardSequence.StartLetter.Coords, endLetter.Coords);

                        MapLetterSequence sequence = forwardSequenceDistanceToEnd <= backwardSequenceDistanceToEnd
                            ? nonFinishingForwardSequence
                            : nonFinishingBackwardSequence;
                        bool isReversed = forwardSequenceDistanceToEnd > backwardSequenceDistanceToEnd;
                        bestSequenceAsList.Add(new MapOrderedLetterSequence(sequence, isReversed));
                    }

                    return bestSequenceAsList;
                }

                List<MapOrderedLetterSequence> bestForward = FindBestDownstreamSequence(forwardSequencesAtStart, false,
                    allIntraversableLetters, endLetter, allowPartial, maxDepth);

                List<MapOrderedLetterSequence> bestBackward = FindBestDownstreamSequence(backwardSequencesAtStart, true,
                    allIntraversableLetters, endLetter, allowPartial, maxDepth);

                if (bestForward != default && bestBackward != default)
                {
                    bool forwardFinishesSequence = bestForward[^1].IsReversed
                        ? bestForward[^1].Sequence.StartLetter == endLetter
                        : bestForward[^1].Sequence.EndLetter == endLetter;
                    
                    bool backwardFinishesSequence = bestBackward[^1].IsReversed
                        ? bestBackward[^1].Sequence.StartLetter == endLetter
                        : bestBackward[^1].Sequence.EndLetter == endLetter;

                    if (forwardFinishesSequence && !backwardFinishesSequence) return bestForward;
                    if (!forwardFinishesSequence && backwardFinishesSequence) return bestBackward;

                    if (bestForward.Count < bestBackward.Count) return bestForward;
                    if (bestForward.Count > bestBackward.Count) return bestBackward;
                    
                    // Prefer more tiles from same number of words
                    if (bestBackward.Sum(sequence => sequence.Sequence.Length) >
                        bestForward.Sum(sequence => sequence.Sequence.Length))
                        return bestBackward;

                    return bestForward;
                }

                return bestForward ?? bestBackward;
            }

            private bool ContainsLetterNonStart(MapLetterSequence sequence, MapLetter letter)
            {
                for (int i = 1; i < sequence.Letters.Count; i++)
                {
                    if (sequence.Letters[i] == letter) return true;
                }

                return false;
            }

            private bool ContainsLetterNonEnd(MapLetterSequence sequence, MapLetter letter)
            {
                for (int i = 0; i < sequence.Letters.Count - 1; i++)
                {
                    if (sequence.Letters[i] == letter) return true;
                }

                return false;
            }
            
            
        #endregion


        #region Get Word Letter Sequence

            public List<MapOrderedLetterSequence> FindBestDownstreamSequence(
                List<MapLetterSequence> forwardSequencesAtStart, 
                bool isReversed, 
                List<MapLetter> allIntraversableLetters, 
                MapLetter endLetter, 
                bool allowPartial,
                int maxDepth,
                int pathAttemptsAfterValidSequence = 2)
            {
                List<MapOrderedLetterSequence> best = default;
                int fewestWords = -1;
                int mostLetters = -1;
                
                foreach (MapLetterSequence sequence in forwardSequencesAtStart)
                {
                    MapOrderedLetterSequence orderedSequence = new MapOrderedLetterSequence(sequence, false);
                    List<MapLetter> allIntraversableLettersForSequence = new List<MapLetter>(allIntraversableLetters);
                    allIntraversableLettersForSequence.AddRange(sequence.Letters);     // Start or end letter is already in all intraversable letters, depending on direction
                    
                    List<MapOrderedLetterSequence> downstreamSequences =
                        GetConnectedWordLetterSequenceBetween(isReversed ? sequence.StartLetter : sequence.EndLetter, endLetter,
                            allIntraversableLettersForSequence, allowPartial, maxDepth - 1);

                    if (downstreamSequences == default)
                        continue;
                    
                    downstreamSequences.Insert(0, orderedSequence);

                    if (best == default ||
                        (downstreamSequences.Count < fewestWords || 
                        (downstreamSequences.Count == fewestWords && downstreamSequences.Sum(sequence => sequence.Sequence.Length) > mostLetters)))
                    {
                        best = downstreamSequences;
                        fewestWords = downstreamSequences.Count;
                        mostLetters = downstreamSequences.Sum(sequence => sequence.Sequence.Length);
                    }

                    if (best != default) pathAttemptsAfterValidSequence--;
                    if (pathAttemptsAfterValidSequence == 0) break;
                }

                return best;
            }

            public List<MapLetterSequence> GetWordLetterSequencesStartingWithLetter(MapLetter mapLetter)
            {
                MapLetterSequence sequenceSoFar = new MapLetterSequence(mapLetter);
                DictionaryNode dictionaryNode = Dictionary.Roots[mapLetter.Character];
                List<Vector2Int> visited = new List<Vector2Int> { mapLetter.Coords };
                List<Vector2Int> allAdjacentCoords = GetAllAdjacentCoords(mapLetter.Coords);

                HashSet<MapLetterSequence> allWords = new HashSet<MapLetterSequence>();
                
                foreach (Vector2Int coords in allAdjacentCoords)
                {
                    List<MapLetterSequence> words = GetWordLetterSequencesFrom(coords, visited, dictionaryNode, sequenceSoFar);
                    allWords.UnionWith(words);
                }

                return allWords.ToList();
            }
            
            private List<MapLetterSequence> GetWordLetterSequencesFrom(Vector2Int adjacentCoords, List<Vector2Int> visited, DictionaryNode parentDictionaryNode, MapLetterSequence sequenceSoFar)
            {
                if (sequenceSoFar.Length >= 15 || Letters[adjacentCoords.x, adjacentCoords.y] == null) return new List<MapLetterSequence>();
                
                HashSet<MapLetterSequence> allWords = new HashSet<MapLetterSequence>();
                
                MapLetter currCharacter = Letters[adjacentCoords.x, adjacentCoords.y];
                sequenceSoFar += currCharacter;
                
                foreach (KeyValuePair<string,DictionaryNode> keyValuePair in parentDictionaryNode.Children)
                {
                    // Only one of the children of the parent dictionary node will match the letter of the adjacent coordinate
                    DictionaryNode childDictionaryNode = keyValuePair.Value;
                    if (childDictionaryNode.Character == currCharacter.Character)
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
                            List<MapLetterSequence> words = GetWordLetterSequencesFrom(coords, visitedWithCurrent, childDictionaryNode, sequenceSoFar);
                            allWords.UnionWith(words);
                        }
                    }
                }

                return allWords.ToList();
            }

        #endregion

        #region Print

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
        #endregion
    }
}
