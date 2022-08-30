using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(MapBoard))]
    public class Game : MonoBehaviour
    {
        [SerializeField] private MapPanel _mapPanel;
        [SerializeField] private TilePanel _tilePanel;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private SpriteRenderer _minimapBG;
        
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
                
                // White background to avoid outline around edge of render texture drawings
                _minimapBG.gameObject.SetActive(true);
                
                _tilePanel.SetPoints(0);
                _tilePanel.SetCoins(0);
                _tilePanel.SetTimeRemaining(60 * 3);
                _tilePanel.SetWordDisplay(null);
                
            }

            private void OnEnable()
            {
                InputManager.TouchStarted += InputManager_OnTouchStarted;
                InputManager.TouchMoved += InputManager_OnTouchMoved;
                InputManager.TouchEnded += InputManager_OnTouchEnded;
                
                MapBoard.WordAttempted += MapBoard_OnWordAttempted;
            }

            private void OnDisable()
            {
                InputManager.TouchStarted -= InputManager_OnTouchStarted;
                InputManager.TouchMoved -= InputManager_OnTouchMoved;
                InputManager.TouchEnded -= InputManager_OnTouchEnded;
            }

        #endregion

        #region Event Handlers

            private void InputManager_OnTouchMoved(Vector2 worldPosition)
            {
                // If selection hasn't been interrupted by game view change
                if (_isSelecting && _gameView == GameView.Tile)
                {
                    _mapBoard.OnTouchMoved(worldPosition);
                }
            }

            private void InputManager_OnTouchStarted(Vector2 worldPosition)
            {
                _isSelecting = true;
                if (_gameView == GameView.Map && MapBoard.IsTileNear(worldPosition))
                {
                    SetGameView(GameView.Tile, worldPosition);
                } else if (_gameView == GameView.Tile)
                {
                    _mapBoard.OnTouchStarted(worldPosition);
                }
            }
            
            private void InputManager_OnTouchEnded(Vector2 worldPosition)
            {
                if (_isSelecting && _gameView == GameView.Tile)
                {
                    _mapBoard.OnTouchEnded(worldPosition);
                }
            }
            
            private void MapBoard_OnWordAttempted(string sequence, bool isWordSucceeded, bool isPerimeterSucceeded, Vector2 latestTerminalPosition)
            {
                _tilePanel.SetWordDisplay(isWordSucceeded ? sequence : null);
                
                if(isWordSucceeded)
                    CameraManager.Instance.AnimateNarrowCameraToPoint(latestTerminalPosition, 1.0f);
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
                    // TODO clear selection was here
                    _mapBoard.gameObject.SetActive(true);
                    
                    _mapPanel.gameObject.SetActive(true);
                    _mapPanel.SetTitle(_mapBoard.Map.State.ToString());
                    _tilePanel.gameObject.SetActive(false);
                    CameraManager.Instance.SwitchToWideCamera(cameraPosition);
                    break;
                
                case GameView.Tile:
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
