using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WarOfWords
{   
    /// <summary>
    /// Attached to UI for GameView.Tile and handles associated UI events (screen touch events are handled by "Game")
    /// </summary>
    public class TilePanel : MonoBehaviour
    {
        [SerializeField] private PanButton _panNButton;
        [SerializeField] private PanButton _panEButton;
        [SerializeField] private PanButton _panSButton;
        [SerializeField] private PanButton _panWButton;

        [SerializeField] private TMP_Text _timeRemainingText;
        [SerializeField] private TMP_Text _pointsText;
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private TMP_Text _wordDisplayText;

        public MapBoard MapBoard { get; set; }
        public Matrix4x4 CanvasMatrix { get; set; }

        #region Lifecycle

            public void Awake()
            {
                CameraManager.NarrowCameraTargetChanged += CameraManager_OnNarrowCameraTargetChanged;
            }

            private void OnDestroy()
            {
                CameraManager.NarrowCameraTargetChanged -= CameraManager_OnNarrowCameraTargetChanged;
            }

            private void OnDrawGizmos()
            {
                if (MapBoard != null && MapBoard.CameraConstraintBounds != default)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.matrix = CanvasMatrix;
                    Vector2 center = new Vector2(MapBoard.CameraConstraintBounds.center.x,
                        MapBoard.CameraConstraintBounds.center.y);
                    Vector2 size = new Vector2(MapBoard.CameraConstraintBounds.size.x,
                        MapBoard.CameraConstraintBounds.size.y);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        #endregion

        #region Setters

            public void SetPoints(int points)
            {
                _pointsText.text = $"{points:n0}";
            }
            
            public void SetTimeRemaining(int secondsRemaining)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(secondsRemaining);
                string secondsRemainingString = $"{timeSpan.Minutes:D1}:{timeSpan.Seconds:D2}";
                _timeRemainingText.text = secondsRemainingString;
            }

            public void SetCoins(int coins)
            {
                _coinsText.text = $"{coins:n0}";
            }

            public void SetWordDisplay(string word)
            {
                _wordDisplayText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(word));
                if (word != null)
                {
                    _wordDisplayText.text = word.ToUpper();
                }
            }
        #endregion

        #region Event Handlers

            private void CameraManager_OnNarrowCameraTargetChanged(Vector2 cameraTarget, Dictionary<GridDirection, bool> canCameraMoveInDirection)
            {
                _panNButton.SetEnabled(canCameraMoveInDirection[GridDirection.N]);
                _panEButton.SetEnabled(canCameraMoveInDirection[GridDirection.E]);
                _panSButton.SetEnabled(canCameraMoveInDirection[GridDirection.S]);
                _panWButton.SetEnabled(canCameraMoveInDirection[GridDirection.W]);
            }
            
            public void OnPanClicked(int gridDirectionValue)
            {
                CameraManager.Instance.PanNarrowCamera((GridDirection)gridDirectionValue, MapBoard.CameraConstraintBounds);
            }
        #endregion
    }
}
