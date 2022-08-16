using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(Game))]
    public class MapBoard : MonoBehaviour
    {
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
        public MapBoardSelection Selection { get; set; }

        #region Lifecycle

        private void Awake()
        {
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
            AddToSelection(worldPosition);
        }
    
        public void OnTouchMoved(Vector2 screenPosition, Vector2 worldPosition)
        {
         
            AddToSelection(worldPosition);
        }
    
        public void OnTouchEnded(Vector2 screenPosition, Vector2 worldPosition)
        {
            ClearSelection();
        }
        
        

        #endregion
        
        private void AddToSelection(Vector2 worldPosition)
        {
            Collider2D circle = Physics2D.OverlapCircle(worldPosition, 0.05f);
            if (circle != null)
            {
                MapLetterTile letterTile = circle.gameObject.GetComponentInParent<MapLetterTile>();
                if (!letterTile.IsSelected)
                {
                    if (Selection == null)
                        Selection = new MapBoardSelection(letterTile);
                    else
                        Selection.AddLetterTile(letterTile);
                }
            }
        }

        public void ClearSelection()
        {
            if (Selection != null)
            {
                Selection.Clear();
                Selection = null;
            }
        }
    }
}
