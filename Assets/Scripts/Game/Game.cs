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
                
                MapBoard.WordAttempted += MapBoard_OnWordAttempted;
                MapBoard.WordReverted += MapBoard_OnWordReverted;
                MapBoard.ZoomTerminalTile += MapBoard_OnZoomTerminalTile;
                
                InputManager.TapStateChanged += InputManager_OnTapStateChanged;
                InputManager.PanStateChanged += InputManager_OnPanStateChanged;
            }

            private void OnDestroy()
            {
                MapBoard.WordAttempted -= MapBoard_OnWordAttempted;
                MapBoard.WordReverted -= MapBoard_OnWordReverted;
                MapBoard.ZoomTerminalTile -= MapBoard_OnZoomTerminalTile;
                
                InputManager.TapStateChanged -= InputManager_OnTapStateChanged;
                InputManager.PanStateChanged -= InputManager_OnPanStateChanged;
            }
        #endregion

        #region Event Handlers

            private void InputManager_OnTapStateChanged(InputState inputState, Vector2 worldPosition)
            {
                if (inputState == InputState.Ended && _gameView == GameView.Map && MapBoard.IsTileNear(worldPosition))
                {
                    SetGameView(GameView.Tile, worldPosition);
                }
            }
            
            private void InputManager_OnPanStateChanged(InputState state, Vector2 worldPosition)
            {
                if (_gameView != GameView.Tile) return;

                    switch (state)
                {
                    case InputState.Started:
                        _isSelecting = true;  // Resets on view change
                        _mapBoard.OnTouchStarted(worldPosition);
                        break;
                    case InputState.Moved:
                        
                        // If selection hasn't been interrupted by game view change
                        if (_isSelecting)
                            _mapBoard.OnTouchMoved(worldPosition);
                        break;
                    case InputState.Ended:
                        if (_isSelecting)
                        {
                            _mapBoard.OnTouchEnded(worldPosition);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }   
            }

            private void MapBoard_OnWordAttempted(string sequence, bool isWordSucceeded, bool isPerimeterSucceeded, Vector2 latestTerminalPosition)
            {
                _tilePanel.SetWordDisplay(isWordSucceeded ? sequence : null);
                
                if(isWordSucceeded)
                    CameraManager.Instance.AnimateNarrowCameraToPoint(latestTerminalPosition, _mapBoard.CameraConstraintBounds, 0.5f);
            }
            
            private void MapBoard_OnWordReverted(Vector2 latestTerminalPosition)
            {
                CameraManager.Instance.AnimateNarrowCameraToPoint(latestTerminalPosition, _mapBoard.CameraConstraintBounds, 0.5f);
            }
            
            private void MapBoard_OnZoomTerminalTile(Vector2 zoomPosition)
            {
                CameraManager.Instance.AnimateNarrowCameraToPoint(zoomPosition, _mapBoard.CameraConstraintBounds, 0.5f);
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
