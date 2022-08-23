using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(Game))]
    public class MapBoard : MonoBehaviour
    {
        public static event Action<List<MapLetterTile>, string, bool> WordAttempted;
        
        [SerializeField] private MapLetterTile _mapLetterTilePrefab;
        
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
        public Bounds CameraConstraintBounds => (Bounds == default) ? default : VectorUtils.ContractBounds(Bounds, 10f, 6f);
        
        private bool _isSelecting;
        
        public MapBoardSelectionPerimeter Perimeter { get; set; }
        public MapBoardSelection Selection { get; set; }
        public Party CurrentPlayerParty { get; set; }

        #region Lifecycle

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
        }

        public bool IsTileNear(Vector2 position)
        {
            Collider2D collider = Physics2D.OverlapCircle(position, 4, LayerMask.GetMask("MapLetterTile"));
            return collider != null;
        }

        #region Event Handlers

            public void OnTouchStarted(Vector2 screenPosition, Vector2 worldPosition)
            {
                AttemptAddToSelection(worldPosition);
            }
        
            public void OnTouchMoved(Vector2 screenPosition, Vector2 worldPosition)
            {
             
                AttemptAddToSelection(worldPosition);
            }
        
            public void OnTouchEnded(Vector2 screenPosition, Vector2 worldPosition)
            {
                if (Selection is { Length: > 0 })
                {
                    string sequence = string.Join("", Selection.SelectedLetterTiles.Select(letterTile => letterTile.MapLetter.Character));
                    bool isWord = Map.Dictionary.IsWord(sequence);

                    if (isWord && Perimeter.CanBeExtendedBy(Selection))
                    {
                        WordAttempted?.Invoke(Selection.SelectedLetterTiles, sequence, isWord);
                        
                        Debug.Log($"Adding {sequence} to perimeter");
                        Selection.IsVerified = true;
                        Perimeter.AddCompletedSelection(Selection);

                        // Reset selection
                        Selection = null;

                        // TODO Final tile highlighting
                        // foreach (MapLetterTile selectedLetterTile in Selection.SelectedLetterTiles)
                        // {
                        //     if (selectedLetterTile.TileOwnership == null)
                        //     {
                        //         selectedLetterTile.TileOwnership = new TileOwnership(CurrentPlayerParty);
                        //     } else if (selectedLetterTile.TileOwnership.IsCurrentPlayer)
                        //     {
                        //         selectedLetterTile.TileOwnership.ClaimCount += 1;
                        //     }
                        //     
                        //     selectedLetterTile.UpdateMainTile();
                        // }
                    }
                    else
                    {
                        if(isWord)
                            Debug.Log($"{sequence} can't extend perimeter");
                        else
                        {
                            Debug.Log($"{sequence} is not a word.");
                        }
                        ClearSelection();
                    }
                }
                
                Selection?.UpdateVisuals();
                Perimeter?.UpdateVisuals();
            }

        #endregion


        #region Methods

            private void AttemptAddToSelection(Vector2 worldPosition)
            {
                Collider2D circle = Physics2D.OverlapCircle(worldPosition, 0.1f);
                if (circle != null)
                {
                    Perimeter ??= new MapBoardSelectionPerimeter();
                    MapLetterTile letterTile = circle.gameObject.GetComponentInParent<MapLetterTile>();
                    
                    // Don't allow selection of existing INTERMEDIATE perimeter tiles
                    if (Perimeter.IsAnIntermediateLetterTile(letterTile))
                    {
                        Debug.Log("Is an intermediate: " + letterTile.MapLetter.Character);
                        return;
                    }

                    bool isAlreadyAtTerminalTile = Selection != null && Selection.SelectedLetterTiles.Count > 1 &&
                                                   Perimeter.SelectionCount > 0 &&
                                                   Perimeter.TerminalLetterTiles.Contains(
                                                       Selection.SelectedLetterTiles[^1]);

                    if (!isAlreadyAtTerminalTile)
                    {
                        if (Selection == null)
                        {
                            Selection = new MapBoardSelection(letterTile);
                        }
                        else
                        {
                            Selection.AddLetterTile(letterTile);
                        }
                    }
                    else
                    {
                        Debug.Log("Selection is already at terminal tile");
                    }
                    
                    Selection?.UpdateVisuals();
                }
            }

            public void ClearSelection()
            {
                
                if (Selection != null)
                {
                    foreach (MapLetterTile selectedLetterTile in Selection.SelectedLetterTiles)
                    {
                        
                        if (!Perimeter.Contains(selectedLetterTile))
                        {
                            selectedLetterTile.Deselect();
                            selectedLetterTile.UpdateVisuals();
                        }
                    }
                    Selection = null;
                    
                }
            }
        #endregion
    }
}
