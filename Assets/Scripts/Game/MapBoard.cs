using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(Game))]
    public class MapBoard : MonoBehaviour
    {
        // 0 - Character sequence, 1 - Is word successful
        public static event Action<string, bool> WordAttempted;
        
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
        public Bounds CameraConstraintBounds => (Bounds == default) ? default : VectorUtils.ContractBounds(Bounds, 10f, 6f);

        public MapBoardSelectionPerimeter Perimeter { get; set; }

        #region Lifecycle

        private void Awake()
        {
            TilePanel.ResetPerimeter += TilePanel_OnResetPerimeter;
        }

        private void OnDestroy()
        {
            TilePanel.ResetPerimeter -= TilePanel_OnResetPerimeter;
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
        }

        public static bool IsTileNear(Vector2 position)
        {
            Collider2D collider = Physics2D.OverlapCircle(position, 4, LayerMask.GetMask("MapLetterTile"));
            return collider != null;
        }

        #region Event Handlers

            public void OnTouchStarted(Vector2 worldPosition)
            {
                if (Perimeter is not { IsComplete: true }) CheckForLetterTile(worldPosition);
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
                WordAttempted?.Invoke(sequence, isWord);

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
            }

            private IEnumerator PauseThenSelectPerimeter()
            {
                yield return new WaitForSeconds(2f);
                
                List<Vector2> selectionPath = Perimeter.GetOrderedVerifiedTiles()
                    .Select(tile => (Vector2)tile.gameObject.transform.position).ToList();
                _tileSelectionCollider.SetPath(0, selectionPath);
        
                ContactFilter2D contactFilter = new ContactFilter2D();
                contactFilter.SetLayerMask(LayerMask.GetMask("MapLetterTile"));
                List<Collider2D> results = new();
                _tileSelectionCollider.OverlapCollider(contactFilter, results);
        
                float averageVerifiedWordLength = Perimeter.GetAverageVerifiedWordLength();
                Debug.Log($"averageVerifiedWordLength={averageVerifiedWordLength}");
                
                List<MapLetterTile> selectedTiles = results.Select(collider => collider.gameObject.GetComponentInParent<MapLetterTile>()).ToList();
                foreach (var selectedTile in selectedTiles)
                {
                    selectedTile.Points += averageVerifiedWordLength;
                    selectedTile.SetColor(TileColor.Highlighted);
                    selectedTile.UpdateVisuals();
                }

                ResetPerimeter();
            } 
            
            private void TilePanel_OnResetPerimeter()
            {
                ResetPerimeter();
            }
            
        #endregion


        #region Methods

            private void CheckForLetterTile(Vector2 worldPosition)
            {
                Collider2D circle = Physics2D.OverlapCircle(worldPosition, 0.1f, LayerMask.GetMask("MapLetterTile"));
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
                
                Debug.Log(letterAdded
                    ? $"ADDED letter tile {letterTile.MapLetter.Character} to current selection: {sequence}"
                    : $"COULD NOT ADD letter tile {letterTile.MapLetter.Character} to current selection: {sequence}");
            }

            private void ResetPerimeter()
            {
                Perimeter.DeselectAll();
                Perimeter.UpdateVisuals();
                Perimeter = new MapBoardSelectionPerimeter();
            }
        #endregion
    }
}
