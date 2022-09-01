using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WarOfWords
{
    [RequireComponent(typeof(Game))]
    public class MapBoard : MonoBehaviour
    {
        // 0 - Character sequence,
        // 1 - Is word successful
        // 2 - Is perimeter successful
        // 3 - New terminal tile position
        public static event Action<string, bool, bool, Vector2> WordAttempted;
        
        // 0 - New terminal tile position
        public static event Action<Vector2> WordReverted;

        // 0 - New position to pan to
        public static event Action<Vector2> ZoomTerminalTile;

        [SerializeField] private MapLetterTile _mapLetterTilePrefab;
        [SerializeField] private PolygonCollider2D _tileSelectionCollider;

        private Map _map;
        public Map Map
        {
            get => _map;
            set
            {
                _map = value;
                PopulateBoard(_map);
            }
        }
        
        public MapLetterTile[,] Board { get; set; }
        public Bounds Bounds { get; set; }
        public Bounds CameraConstraintBounds => (Bounds == default) ? default : VectorUtils.ContractBounds(Bounds, 5f, 3f);

        public MapBoardSelectionPerimeter Perimeter { get; set; }
        public Vector2 _lastTerminalTilePositionReportedInEvent;

        #region Lifecycle

        private void Awake()
        {
            TilePanel.ResetPerimeterPressed += TilePanel_OnResetPerimeterPressed;
            TilePanel.RevertLastWordPressed += TilePanel_OnRevertLastWordPressed;
            TilePanel.ToggleZoomTerminalTilePressed += TilePanel_OnToggleZoomTerminalTilePressed;
        }

        private void OnDestroy()
        {
            TilePanel.SolveWordPressed -= TilePanel_OnResetPerimeterPressed;
            TilePanel.RevertLastWordPressed -= TilePanel_OnRevertLastWordPressed;
            TilePanel.ToggleZoomTerminalTilePressed -= TilePanel_OnToggleZoomTerminalTilePressed;
        }

        #endregion

        private void PopulateBoard(Map map)
        {
            if (Board != null)
            {
                for (int i = 0; i < Board.GetLength(0); i++)
                {
                    for (int j = 0; j < Board.GetLength(1); j++)
                    {
                        if (Board[i, j] != null)
                        {
                            Destroy(Board[i, j]);
                        }
                    }
                }
            }
            
            Board = new MapLetterTile[map.Cols, map.Rows];
            
            for (int row = 0; row < map.Rows; row++)
            {
                for (int col = 0; col < map.Cols; col++)
                {
                    if (Map.Letters[col, row] != null)
                    {
                        Board[col, row] = Instantiate(_mapLetterTilePrefab, new Vector2(col + 0.5f, row + 0.5f),
                            Quaternion.identity);

                        Board[col, row].MapLetter = Map.Letters[col, row];
                        Board[col, row].gameObject.transform.parent = transform;
                        Board[col, row].UpdateVisuals();
                    }
                }
            }

            Vector2 center = new Vector2(map.Cols / 2.0f, map.Rows / 2.0f);
            Vector2 size = new Vector2(map.Cols, map.Rows);
            Bounds = new Bounds(center, size);
            
            AssignTileBonuses();
        }

        public static bool IsTileNear(Vector2 position)
        {
            Collider2D collider = Physics2D.OverlapCircle(position, 4, LayerMask.GetMask("MapLetterTile"));
            return collider != null;
        }

        #region Event Handlers

            public void OnTouchStarted(Vector2 worldPosition)
            {
                if (Perimeter is not { IsComplete: true }) CheckForLetterTile(worldPosition, 0.3f);
            }
        
            public void OnTouchMoved(Vector2 worldPosition)
            {
                if (Perimeter is not { IsComplete: true }) CheckForLetterTile(worldPosition);
            }
        
            public void OnTouchEnded(Vector2 worldPosition)
            {
                if (Perimeter != null && Perimeter.IsComplete) return;
                if (Perimeter.CurrentSelection is not { LetterTileCount: > 0 }) return;
                
                string sequence = Perimeter.CurrentSelection.ToCharacterSequence();
                bool isWord = sequence.Length >= 3 && Map.Dictionary.IsWord(sequence);

                bool deselect = !isWord;
                if (isWord)
                {
                    bool successfullyMerged = Perimeter.MergeCurrent();
                    deselect = !successfullyMerged;
                }
                
                if(deselect) 
                {
                    // TODO Visual fail
                    Perimeter.DeselectCurrent();
                } 
                
                Perimeter.UpdateVisuals();
                Perimeter.Print();

                if (Perimeter.IsComplete)
                {
                    StartCoroutine(PauseThenSelectPerimeter());
                }

                _lastTerminalTilePositionReportedInEvent = Perimeter.MostRecentTerminalVerifiedTile != null
                    ? Perimeter.MostRecentTerminalVerifiedTile.transform.position
                    : default;
                WordAttempted?.Invoke(sequence, isWord && !deselect, Perimeter.IsComplete, _lastTerminalTilePositionReportedInEvent);
            }
            
            private void TilePanel_OnResetPerimeterPressed()
            {
                ResetPerimeter();
            }
            
            private void TilePanel_OnRevertLastWordPressed()
            {
                RevertLastWord();
            }
            
            private void TilePanel_OnToggleZoomTerminalTilePressed()
            {
                ToggleZoomTerminalTile();
            }
        #endregion


        #region Methods
        
            private IEnumerator PauseThenSelectPerimeter()
            {
                yield return new WaitForSeconds(2f);

                var orderedVerifiedTiles = Perimeter.GetOrderedVerifiedTiles();
                List<Vector2> selectionPath = orderedVerifiedTiles
                    .Select(tile => (Vector2)tile.gameObject.transform.position).ToList();
                _tileSelectionCollider.SetPath(0, selectionPath);
        
                ContactFilter2D contactFilter = new ContactFilter2D();
                contactFilter.SetLayerMask(LayerMask.GetMask("MapLetterTile"));
                List<Collider2D> results = new();
                _tileSelectionCollider.OverlapCollider(contactFilter, results);
        
                float averageVerifiedWordLength = Perimeter.GetAverageVerifiedWordLength();

                // TODO don't recount at beginning
                Dictionary<BonusType, int> counts = new();
                foreach (var tile in orderedVerifiedTiles.Where(tile => tile.BonusType != BonusType.None))
                {
                    if (counts.ContainsKey(tile.BonusType))
                    {
                        counts[tile.BonusType]++;
                    }
                    else
                    {
                        counts[tile.BonusType] = 1;
                    }
                }

                foreach (BonusType bonusType in counts.Keys)
                {
                    Debug.Log($"{bonusType}: {counts[bonusType]:n0}");
                }

                List<MapLetterTile> selectedTiles = results.Select(collider => collider.gameObject.GetComponentInParent<MapLetterTile>()).ToList();
                foreach (var selectedTile in selectedTiles)
                {
                    selectedTile.Points += averageVerifiedWordLength;
                    selectedTile.SetColor(TileColor.Highlighted);
                    selectedTile.UpdateVisuals();
                }
                
                // Debug.Log("Total points this round: " + selectedTiles.Count * averageVerifiedWordLength);

                ResetPerimeter();
            } 

            private void CheckForLetterTile(Vector2 worldPosition, float radius = 0.1f)
            {
                Collider2D circle = Physics2D.OverlapCircle(worldPosition, radius, LayerMask.GetMask("MapLetterTile"));
                if (circle == null) return;
                
                MapLetterTile letterTile = circle.gameObject.GetComponentInParent<MapLetterTile>();
                if (letterTile == null)
                {
                    throw new Exception("Letter tile does not exist at world position: " + worldPosition);
                }
                
                Perimeter ??= new MapBoardSelectionPerimeter();
                bool letterAdded = Perimeter.AddLetterTileToCurrentSelection(letterTile);
                if (letterAdded)
                {
                    Perimeter.UpdateVisuals();
                }

                string sequence = Perimeter.CurrentSelection == null
                    ? "<none>"
                    : Perimeter.CurrentSelection.ToCharacterSequence();
                
                // Debug.Log(letterAdded
                //     ? $"ADDED letter tile {letterTile.MapLetter.Character} to current selection: {sequence}"
                //     : $"COULD NOT ADD letter tile {letterTile.MapLetter.Character} to current selection: {sequence}");
            }

            private void ResetPerimeter()
            {
                Perimeter.DeselectAll();
                Perimeter.UpdateVisuals();
                Perimeter = new MapBoardSelectionPerimeter();
            }

            private void RevertLastWord()
            {
                if (Perimeter.RevertLastVerifiedSelection())
                {
                    _lastTerminalTilePositionReportedInEvent = Perimeter.MostRecentTerminalVerifiedTile != null
                        ? Perimeter.MostRecentTerminalVerifiedTile.transform.position
                        : default;
                    WordReverted?.Invoke(_lastTerminalTilePositionReportedInEvent);
                }
            }

            private void ToggleZoomTerminalTile()
            {
                if (Perimeter == null || Perimeter.TerminalVerifiedStartTile == null) return;
                _lastTerminalTilePositionReportedInEvent = (Vector2)
                    Perimeter.TerminalVerifiedStartTile.transform.position == _lastTerminalTilePositionReportedInEvent ? 
                    Perimeter.TerminalVerifiedEndTile.transform.position : 
                    Perimeter.TerminalVerifiedStartTile.transform.position;
                ZoomTerminalTile?.Invoke(_lastTerminalTilePositionReportedInEvent);
            }

            private List<MapLetterTile> GetAllLetterTiles()
            {
                return Board.Cast<MapLetterTile>().Where(tile => tile != null).ToList();
            }

            private void AssignTileBonuses()
            {
                Dictionary<BonusType, int> bonusTypeToPer1000 = new Dictionary<BonusType, int>()
                {
                    { BonusType.Points1, 20 },
                    { BonusType.Points2, 7 },
                    { BonusType.Points3, 2 },
                    { BonusType.Coins1, 30 },
                    { BonusType.Coins2, 10 },
                    { BonusType.Coins3, 5 }
                };

                List<BonusType> orderedBonusTypes = new List<BonusType>(bonusTypeToPer1000.Keys);
                List<MapLetterTile> letterTiles = GetAllLetterTiles();

                foreach (MapLetterTile letterTile in letterTiles)
                {

                    float threshold = 0;
                    int random = Random.Range(1, 1000);
                    
                    foreach (BonusType bonusType in orderedBonusTypes)
                    {
                        int per1000 = bonusTypeToPer1000[bonusType];
                        threshold += per1000;

                        if (random <= threshold)
                        {
                            letterTile.SetBonus(bonusType);
                            letterTile.UpdateVisuals();
                            break;
                        }
                    }
                }
            }
        #endregion
    }
}
