using System;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(Game))]
    public class MapBoard : MonoBehaviour
    {
        [SerializeField]
        private MapLetterTile _mapLetterTilePrefab;
        
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
        
        public Vector2 CenterPoint { get; set; }
        
        private Game _game;

        private void Awake()
        {
            _game = GetComponent<Game>();
        }

        private void OnEnable()
        {
            InputManager.TouchStarted += InputManagerOnTouchStarted;
            InputManager.TouchMoved += InputManagerOnTouchMoved;
            InputManager.TouchEnded += InputManagerOnTouchEnded;
        }

        private void OnDisable()
        {
            InputManager.TouchStarted -= InputManagerOnTouchStarted;
            InputManager.TouchMoved -= InputManagerOnTouchMoved;
            InputManager.TouchEnded -= InputManagerOnTouchEnded;
        }

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
                        Board[col, row] = Instantiate(_mapLetterTilePrefab, new Vector2(col + 0.5f, map.Rows - row - 0.5f),
                            Quaternion.identity);

                        Board[col, row].MapLetter = Map.Letters[col, row];
                        Board[col, row].gameObject.transform.parent = transform;
                    }
                }
            }

            CenterPoint = new Vector2(map.Cols / 2.0f, map.Rows / 2.0f);
        }

        public bool IsTileNear(Vector2 position)
        {
            Collider2D collider = Physics2D.OverlapCircle(position, 4, LayerMask.GetMask("MapLetterTile"));
            return collider != null;
        }

        private void InputManagerOnTouchStarted(Vector2 screenPosition, Vector2 worldPosition)
        {
            
        }
    
        private void InputManagerOnTouchMoved(Vector2 screenPosition, Vector2 worldPosition)
        {
            
        }
    
        private void InputManagerOnTouchEnded(Vector2 screenPosition, Vector2 worldPosition)
        {
               
        }
    }
}
