using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;

namespace WarOfWords
{
    public class MapShuffler : MonoBehaviour
    {
        private Map _map;
        private Coroutine _shuffleCoroutine;

        private int _startTotalWords;
        private int _startTotalWordLetters;
        private float _startAvgWordsPerTile;
        private float _startAvgWordLettersPerTile;

        private int _shuffleCount = 0;
        private bool _isEasier;

        private bool _isShuffling;
        
        public void StartShuffle(Map map, bool isEasier)
        {
            if (_isShuffling) throw new RuntimeWrappedException("Shuffle already running.");

            _isShuffling = true;
            
            _map = map;
            _shuffleCount = 0;
            _isEasier = isEasier;
            _startTotalWords = _map.TotalWords;
            _startTotalWordLetters = _map.TotalWordLetters;
            _startAvgWordsPerTile = _map.AvgWordsPerTile;
            _startAvgWordLettersPerTile = _map.AvgWordLettersPerTile;

            StartCoroutine(ShuffleBatch());
            
            Debug.Log("Starting Shuffle...");
        }

        IEnumerator ShuffleBatch()
        {
            int previousTotalWords = _map.TotalWords;
            List<PreviousLetterState> previousLetterStates = Shuffle(25);
            
            _map.RefreshLetterStats();
            if ((_isEasier && previousTotalWords > _map.TotalWords) ||
                (!_isEasier && previousTotalWords < _map.TotalWords))
            {
                // Revert
                foreach (PreviousLetterState previousLetterState in previousLetterStates)
                {
                    _map.Letters[previousLetterState.Coords.x, previousLetterState.Coords.y].Character =
                        previousLetterState.Character;
                }
                _map.RefreshLetterStats();
            }
            
            _shuffleCount++;
            
            string report = $"Shuffle {_shuffleCount} complete.\n" +
                            $"Initial AvgWordsPerTile={_startAvgWordsPerTile:n2}, Current AvgWordsPerTile={_map.AvgWordsPerTile:n2}, Diff={(_map.AvgWordsPerTile - _startAvgWordsPerTile):n2}\n" +
                            $"Initial TotalWords={_startTotalWords:n0}, Current TotalWords={_map.TotalWords:n0}, Diff={(_map.TotalWords - _startTotalWords):n0}\n";
            Debug.Log(report);
            _map.Print();

            if (!_isShuffling) yield break;
            yield return new WaitForSeconds(1f);

            StartCoroutine(ShuffleBatch());
        }

        public List<PreviousLetterState> Shuffle(int tilesToShuffle)
        {
            List<Vector2Int> alreadyShuffled = new List<Vector2Int>();
            
            Random random = new Random();

            List<PreviousLetterState> previousLetterStates = new List<PreviousLetterState>();
            
            for (int i = 0; i < tilesToShuffle; i++)
            {
                int x = random.Next(0, _map.Cols - 1);
                int y = random.Next(0, _map.Rows - 1);
                int tryCount = 0;

                while (tryCount < 1000 && 
                       (_map.Letters[x, y] == null || alreadyShuffled.Contains(new Vector2Int(x, y))))
                {
                    x = random.Next(0, _map.Cols - 1);
                    y = random.Next(0, _map.Rows - 1);
                    tryCount++;
                }

                MapLetter mapLetter = _map.Letters[x, y];
                if (mapLetter != null)
                {
                    alreadyShuffled.Add(new Vector2Int(x, y));
                    previousLetterStates.Add(new PreviousLetterState(mapLetter.Coords, mapLetter.Character));
                    mapLetter.Character = CharacterUtils.GetRandomUppercaseAlphaCharacter(_isEasier);
                }
            }

            return previousLetterStates;
        }

        public void StopShuffle()
        {
            Debug.Log("Stopping shuffle...");
            _isShuffling = false;
            _map.Print();
        }
    }

    public class PreviousLetterState
    {
        public Vector2Int Coords { get; set; }
        public string Character { get; set; }

        public PreviousLetterState(Vector2Int coords, string character)
        {
            Coords = coords;
            Character = character;
        }
    }
}
