using System;
using Unity.VisualScripting;
using UnityEngine;

namespace WarOfWords
{
    [RequireComponent(typeof(MapBoard))]
    public class Game : MonoBehaviour
    {
        [SerializeField] private MapPanel _mapPanel;
        [SerializeField] private TilePanel _tilePanel;
        [SerializeField] private GameObject _joystickPanel;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private SpriteRenderer _minimapBG;
        
        private GameView _gameView;
        public GameView GameView => _gameView;
        
        private MapBoard _mapBoard;
        
        private InputType _inputType;

        // Double-finger pan
        private Vector2 _lastDoublePanScreenPosition;
        private Vector2 _screenToWorldMultiplier;
        
        // Joystick
        private bool _isJoysticking;
        private Vector2 _joystickMoveAmount;
        private float _joystickPanSpeed = 2.5f;

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
                InputManager.DoublePanStateChanged += InputManager_OnDoublePanStateChanged;
                InputManager.ScaleStateChanged += InputManager_OnScaleStateChanged;
                InputManager.JoystickStateChanged += InputManager_OnJoystickStateChanged;
                
                TilePanel.ZoomInPressed += TilePanel_OnZoomInPressed;
                TilePanel.ZoomOutPressed += TilePanel_OnZoomOutPressed;
            }

            private void FixedUpdate()
            {
                if (_isJoysticking)
                {
                    Vector2 joystickMoveWorldOffset = _joystickMoveAmount * _joystickPanSpeed * Time.fixedDeltaTime;
                    CameraManager.Instance.ManualPanNarrowCameraByWorldOffset(joystickMoveWorldOffset);
                }
            }

            private void OnDestroy()
            {
                MapBoard.WordAttempted -= MapBoard_OnWordAttempted;
                MapBoard.WordReverted -= MapBoard_OnWordReverted;
                MapBoard.ZoomTerminalTile -= MapBoard_OnZoomTerminalTile;
                
                InputManager.TapStateChanged -= InputManager_OnTapStateChanged;
                InputManager.PanStateChanged -= InputManager_OnPanStateChanged;
                InputManager.DoublePanStateChanged -= InputManager_OnDoublePanStateChanged;
                InputManager.ScaleStateChanged -= InputManager_OnScaleStateChanged;
                InputManager.JoystickStateChanged -= InputManager_OnJoystickStateChanged;
                
                TilePanel.ZoomInPressed -= TilePanel_OnZoomInPressed;
                TilePanel.ZoomOutPressed -= TilePanel_OnZoomOutPressed;
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
            
            private void InputManager_OnPanStateChanged(InputState inputState, Vector2 worldPosition)
            {
                if (_gameView != GameView.Tile) return;

                switch (inputState)
                {
                    case InputState.Started:
                        _inputType = InputType.Pan;     // Resets on view change
                        _mapBoard.OnTouchStarted(worldPosition);
                        break;
                    case InputState.Moved:
                        
                        // If selection hasn't been interrupted by game view change
                        if (_inputType == InputType.Pan)
                            _mapBoard.OnTouchMoved(worldPosition);
                        break;
                    case InputState.Ended:
                        if (_inputType == InputType.Pan)
                        {
                            _mapBoard.OnTouchEnded(worldPosition);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inputState), inputState, null);
                }   
            }
            
            private void InputManager_OnDoublePanStateChanged(InputState inputState, Vector2 screenPosition)
            {
                if (_gameView != GameView.Tile) return;
                
                // Don't do anything on Started, just record the point
                // We assume that Ended is just going to give the same point as the last Moved
                if (inputState == InputState.Started)
                {
                    _screenToWorldMultiplier = CameraManager.Instance.GetScreenToWorldMultiplier();
                } 
                else if (inputState == InputState.Moved)
                {
                    Vector2 screenOffset = _lastDoublePanScreenPosition - screenPosition;
                    Vector2 worldOffset = screenOffset * _screenToWorldMultiplier;
                    CameraManager.Instance.ManualPanNarrowCameraByWorldOffset(worldOffset);
                }

                _lastDoublePanScreenPosition = screenPosition;
                
            }
            
            private void InputManager_OnScaleStateChanged(InputState inputState, float scaleMultiplier)
            {
                if (_gameView != GameView.Tile) return;
                CameraManager.Instance.ManualDollyNarrowCameraByMultipleOfCurrentSize(scaleMultiplier);
            }
            
            private void InputManager_OnJoystickStateChanged(InputState inputState, Vector2 moveAmount)
            {
                if (_gameView != GameView.Tile) return;

                _isJoysticking = inputState != InputState.Ended;
                _joystickMoveAmount = moveAmount;
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
            
            private void TilePanel_OnZoomOutPressed()
            {
                if (_gameView != GameView.Tile) return;

                float multiplier = (CameraManager.Instance.LatestScalePercent + 0.25f);
                print("zoom out: " + multiplier);
                CameraManager.Instance.ManualDollyNarrowCameraByMultipleOfOriginalSize(multiplier);
            }

            private void TilePanel_OnZoomInPressed()
            {
                if (_gameView != GameView.Tile) return;
                
                float multiplier = (CameraManager.Instance.LatestScalePercent - 0.25f);
                print("zoom in: " + multiplier);
                CameraManager.Instance.ManualDollyNarrowCameraByMultipleOfOriginalSize(multiplier);
            }
        #endregion

        private void LoadMap(State state)
        {
            _mapBoard.Map = MapBakedReader.LoadNewMapFromData(state);
        }

        public void SetGameView(GameView gameView, Vector2 cameraPosition)
        {
            // Reset input type on view change
            _inputType = InputType.None;
            
            _mapBoard.gameObject.SetActive(true);
            
            _joystickPanel.SetActive(gameView == GameView.Tile);
            _tilePanel.gameObject.SetActive(gameView == GameView.Tile);
            _mapPanel.gameObject.SetActive(gameView == GameView.Map);
            
            _gameView = gameView;
            
            switch (_gameView)
            {
                case GameView.Map:
                    _mapPanel.SetTitle(_mapBoard.Map.State.ToString());
                    CameraManager.Instance.SwitchToWideCamera(cameraPosition);
                    break;
                
                case GameView.Tile:
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
