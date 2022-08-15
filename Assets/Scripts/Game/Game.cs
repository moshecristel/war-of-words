using System;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(MapBoard))]
    public class Game : MonoBehaviour
    {
        [SerializeField] private MapPanel _mapPanel;
        [SerializeField] private TilePanel _tilePanel;
        [SerializeField] private Canvas _canvas;
        
        private GameView _gameView;
        public GameView GameView => _gameView;
        
        private MapBoard _mapBoard;

        private bool _isSelecting;

        #region Lifecycle

        private void Awake()
        {
            _mapBoard = GetComponent<MapBoard>();
            
            LoadMap(State.Washington);
            
            _mapBoard.Map.Print();
            SetGameView(GameView.Map, _mapBoard.Bounds.center);

            _tilePanel.MapBoard = _mapBoard;
            _tilePanel.CanvasMatrix = _canvas.GetCanvasMatrix();
            _mapPanel.MapBoard = _mapBoard;
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

        #endregion

        #region Event Handlers

        private void InputManagerOnTouchMoved(Vector2 screenPosition, Vector2 worldPosition)
        {
            // If selection hasn't been interrupted by game view change
            if (_isSelecting && _gameView == GameView.Tile)
            {
                _mapBoard.OnTouchMoved(screenPosition, worldPosition);
            }
        }

        private void InputManagerOnTouchStarted(Vector2 screenPosition, Vector2 worldPosition)
        {
            _isSelecting = true;
            if (_gameView == GameView.Map && _mapBoard.IsTileNear(worldPosition))
            {
                SetGameView(GameView.Tile, worldPosition);
            } else if (_gameView == GameView.Tile)
            {
                _mapBoard.OnTouchStarted(screenPosition, worldPosition);
            }
        }
        
        private void InputManagerOnTouchEnded(Vector2 screenPosition, Vector2 worldPosition)
        {
            if (_isSelecting && _gameView == GameView.Tile)
            {
                _mapBoard.OnTouchEnded(screenPosition, worldPosition);
            }
        }
        #endregion

        private void LoadMap(State state)
        {
            _mapBoard.Map = MapReader.LoadNewMapFromData(state);
        }

        public void SetGameView(GameView gameView, Vector2 cameraPosition)
        {
            _isSelecting = false;
            
            _gameView = gameView;
            switch (_gameView)
            {
                case GameView.Map:
                    Debug.Log("Changing to map");
                    _mapBoard.ClearSelection();
                    _mapBoard.gameObject.SetActive(true);
                    
                    _mapPanel.gameObject.SetActive(true);
                    _mapPanel.SetTitle(_mapBoard.Map.State.ToString());
                    _tilePanel.gameObject.SetActive(false);
                    CameraManager.Instance.SwitchToWideCamera(cameraPosition);
                    break;
                
                case GameView.Tile:
                    Debug.Log("Changing to tile");
                    _mapBoard.gameObject.SetActive(true);
                    
                    _tilePanel.gameObject.SetActive(true);
                    _mapPanel.gameObject.SetActive(false);
                    CameraManager.Instance.SwitchToNarrowCamera(cameraPosition, _mapBoard.CameraConstraintBounds);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnFullMapButtonClicked()
        {
            SetGameView(GameView.Map, _mapBoard.Bounds.center);
        }
    }
}
