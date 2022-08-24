using System;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(Game))]
    public class MapBoard : MonoBehaviour
    {
        // 0 - Character sequence, 1 - Is word successful
        public static event Action<string, bool> WordAttempted;
        
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

        public MapBoardSelectionPerimeter Perimeter { get; set; }

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

        public static bool IsTileNear(Vector2 position)
        {
            Collider2D collider = Physics2D.OverlapCircle(position, 4, LayerMask.GetMask("MapLetterTile"));
            return collider != null;
        }

        #region Event Handlers

            public void OnTouchStarted(Vector2 worldPosition)
            {
                CheckForLetterTile(worldPosition);
            }
        
            public void OnTouchMoved(Vector2 worldPosition)
            {
             
                CheckForLetterTile(worldPosition);
            }
        
            public void OnTouchEnded(Vector2 worldPosition)
            {
                if (Perimeter.CurrentSelection is not { LetterTileCount: > 0 }) return;
                
                string sequence = Perimeter.CurrentSelection.ToCharacterSequence();
                bool isWord = Map.Dictionary.IsWord(sequence);
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
                    Debug.Log("Deselecting current");
                    Perimeter.DeselectCurrent();
                }
                
                Perimeter.UpdateVisuals();

                Perimeter.Print();
            }

        #endregion


        #region Methods

            private void CheckForLetterTile(Vector2 worldPosition)
            {
                Collider2D circle = Physics2D.OverlapCircle(worldPosition, 0.1f);
                if (circle == null) return;
                
                MapLetterTile letterTile = circle.gameObject.GetComponentInParent<MapLetterTile>();
                if (letterTile == null)
                {
                    Debug.Log("null letter tile!");
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
        #endregion
    }
}
