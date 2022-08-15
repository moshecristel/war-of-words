using System.Collections.Generic;
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
    }
}
