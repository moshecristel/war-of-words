using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WarOfWords
{
    [RequireComponent(typeof(MapBoard))]
    public class Game : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _fullMapButton;

        private GameView _gameView;
        private MapBoard _mapBoard;

        private void Awake()
        {
            _mapBoard = GetComponent<MapBoard>();
            
            LoadMap(State.Washington);
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
                    _titleText.gameObject.SetActive(true);
                    _titleText.text = _mapBoard.Map.State.ToString();
                    _fullMapButton.gameObject.SetActive(false);
                    CameraManager.Instance.SwitchToWideCamera(cameraPosition);
                    break;
                case GameView.Tile:
                    _titleText.gameObject.SetActive(false);
                    _fullMapButton.gameObject.SetActive(true);
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
    }
}
