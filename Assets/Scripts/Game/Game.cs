using System;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(MapBoard))]
    public class Game : MonoBehaviour
    {
        [SerializeField] private MapPanel _mapPanel;
        [SerializeField] private TilePanel _tilePanel;
        
        private GameView _gameView;
        public GameView GameView => _gameView;
        
        private MapBoard _mapBoard;

        private void Awake()
        {
            _mapBoard = GetComponent<MapBoard>();
            
            LoadMap(State.Washington);
            
            _mapBoard.Map.Print();
            SetGameView(GameView.Map, _mapBoard.CenterPoint);
        }

        private void OnEnable()
        {
            InputManager.TouchStarted += InputManagerOnTouchStarted;
        }

        private void InputManagerOnTouchStarted(Vector2 screenPosition, Vector2 worldPosition)
        {
            if (_gameView == GameView.Map && _mapBoard.IsTileNear(worldPosition))
            {
                SetGameView(GameView.Tile, worldPosition);
            }
        }

        private void LoadMap(State state)
        {
            _mapBoard.Map = MapReader.LoadNewMapFromData(state);
        }

        public void SetGameView(GameView gameView, Vector2 cameraPosition)
        {
            _gameView = gameView;
            switch (_gameView)
            {
                case GameView.Map:
                    _mapBoard.gameObject.SetActive(true);
                    _mapPanel.SetTitle(_mapBoard.Map.State.ToString());
                    _tilePanel.gameObject.SetActive(false);
                    CameraManager.Instance.SwitchToWideCamera(cameraPosition);
                    break;
                
                case GameView.Tile:
                    _tilePanel.gameObject.SetActive(true);
                    _mapBoard.gameObject.SetActive(false);
                    CameraManager.Instance.SwitchToNarrowCamera(cameraPosition);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnFullMapButtonClicked()
        {
            SetGameView(GameView.Map, _mapBoard.CenterPoint);
        }

        public void OnPanClicked(int gridDirectionValue)
        {
            Debug.Log("Pan clicked: " + gridDirectionValue);
            switch ((GridDirection)gridDirectionValue)
            {
                case GridDirection.N:
                    break;
                case GridDirection.E:
                    break;
                case GridDirection.S:
                    break;
                case GridDirection.W:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gridDirectionValue), gridDirectionValue, null);
            }
        }
    }
}
