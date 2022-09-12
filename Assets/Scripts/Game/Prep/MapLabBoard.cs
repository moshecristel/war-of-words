using UnityEngine;

namespace WarOfWords
{
    public class MapLabBoard : MonoBehaviour
    {
        [SerializeField] private MapLabLetterTile _mapLabLetterTilePrefab;
        [SerializeField] private Camera _camera;
        
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
        
        public MapLabLetterTile[,] Board { get; set; }
        public Bounds Bounds { get; set; }
        
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
            
            Board = new MapLabLetterTile[map.Cols, map.Rows];
            
            for (int row = 0; row < map.Rows; row++)
            {
                for (int col = 0; col < map.Cols; col++)
                {
                    if (Map.Letters[col, row] != null)
                    {
                        Board[col, row] = Instantiate(_mapLabLetterTilePrefab, new Vector2(col + 0.5f, row + 0.5f),
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

            _camera.transform.position = new Vector3(center.x, center.y, -10);
            _camera.orthographicSize = (map.Rows / 2f) * 1.2f;
        }
    }
}
