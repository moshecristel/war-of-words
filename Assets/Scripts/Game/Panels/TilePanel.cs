using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarOfWords
{   
    /// <summary>
    /// Attached to UI for GameView.Tile and handles associated UI events (screen touch events are handled by "Game")
    /// </summary>
    public class TilePanel : MonoBehaviour
    {
        [SerializeField] private Button _panNButton;
        [SerializeField] private Button _panEButton;
        [SerializeField] private Button _panSButton;
        [SerializeField] private Button _panWButton;
        
        public MapBoard MapBoard { get; set; }
        private Bounds _lastContractedBounds; 

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
            if (_lastContractedBounds != default)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(_lastContractedBounds.center, _lastContractedBounds.size);
            }
        }

        #endregion

        private void CameraManager_OnNarrowCameraTargetChanged(Vector2 cameraTarget, Dictionary<GridDirection, bool> canCameraMoveInDirection)
        {
            _panNButton.interactable = canCameraMoveInDirection[GridDirection.N];
            _panEButton.interactable = canCameraMoveInDirection[GridDirection.E];
            _panSButton.interactable = canCameraMoveInDirection[GridDirection.S];
            _panWButton.interactable = canCameraMoveInDirection[GridDirection.W];
        }
        
        public void OnPanClicked(int gridDirectionValue)
        {
            _lastContractedBounds = VectorUtils.ContractBounds(MapBoard.Bounds, 10f, 6f);
            CameraManager.Instance.PanNarrowCamera((GridDirection)gridDirectionValue, _lastContractedBounds);
        }
    }
}
