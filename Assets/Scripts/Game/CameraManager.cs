using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    public class CameraManager : Singleton<CameraManager>
    {
        // Position, GridDirection => PanAllowed
        public static event Action<Vector2, Dictionary<GridDirection, bool>> NarrowCameraTargetChanged;
        
        [SerializeField] private Camera _narrowCamera;
        [SerializeField] private SpriteRenderer _narrowCameraMinimapAreaGraphic;
        
        [SerializeField] private Camera _wideCamera;
        [SerializeField] private Camera _minimapCamera;

        public Camera WideCamera => _wideCamera;
        public Camera NarrowCamera => _narrowCamera;

        private bool _isAnimatingCamera;

        private void Awake()
        {
            // Camera Minimap Area (Shape designating the bounds of the narrow camera within the larger map)
            float xSize = _narrowCamera.aspect * _narrowCamera.orthographicSize * 2f;
            float ySize = _narrowCamera.orthographicSize * 2f;
            
            _narrowCameraMinimapAreaGraphic.size = new Vector2(xSize, ySize);
        }

        private void Update()
        {
            _minimapCamera.transform.position = _wideCamera.transform.position;
        }
        
        public Vector2 ScreenToWorldPosition(Vector2 screenPosition)
        {
            return _narrowCamera.isActiveAndEnabled ? 
                _narrowCamera.ScreenToWorldPoint(screenPosition) : 
                _wideCamera.ScreenToWorldPoint(screenPosition);
        }

        public void SwitchToWideCamera(Vector2 position)
        {
            _wideCamera.gameObject.SetActive(true);
            _narrowCamera.gameObject.SetActive(false);

            _wideCamera.transform.position = new Vector3(position.x, position.y, -10);
        }

        public void SwitchToNarrowCamera(Vector2 position, Bounds cameraMovementConstraint)
        {
            _narrowCamera.gameObject.SetActive(true);
            _wideCamera.gameObject.SetActive(false);

            Vector3 cameraTo = new Vector3(position.x, position.y, -10f);
            FireNarrowCameraTargetChanged(cameraTo, cameraMovementConstraint);
            
            _narrowCamera.transform.position = new Vector3(position.x, position.y, -10);
        }

        public void AnimateNarrowCameraToPoint(Vector2 to, Bounds cameraMovementConstraint, float time)
        {
            MoveNarrowCamera(to, cameraMovementConstraint, time, LeanTweenType.easeInOutSine);
        }

        public void PanNarrowCamera(GridDirection panDirection, Bounds cameraMovementConstraint, float panPercentage = 0.5f) 
        {
            if (LeanTween.isTweening())
            {
                LeanTween.cancelAll();
                _isAnimatingCamera = false;
            }
            
            Vector2 currentCameraPosition = _narrowCamera.transform.position;
            float cameraWidth = (_narrowCamera.orthographicSize * _narrowCamera.aspect) * 2f;
            float cameraHeight = (_narrowCamera.orthographicSize * 2);

            Vector2 newCameraPositionUnconstrained;
            
            switch (panDirection)
            {
                case GridDirection.N:
                    newCameraPositionUnconstrained =
                        currentCameraPosition + new Vector2(0f, cameraHeight * panPercentage);
                    break;
                case GridDirection.E:
                    newCameraPositionUnconstrained =
                        currentCameraPosition + new Vector2(cameraWidth * panPercentage, 0f);
                    break;
                case GridDirection.S:
                    newCameraPositionUnconstrained =
                        currentCameraPosition - new Vector2(0f, cameraHeight * panPercentage);
                    break;
                case GridDirection.W:
                    newCameraPositionUnconstrained =
                        currentCameraPosition - new Vector2(cameraWidth * panPercentage, 0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(panDirection), panDirection, null);
            }

            MoveNarrowCamera(newCameraPositionUnconstrained, cameraMovementConstraint, 1f, LeanTweenType.easeOutQuint);
            
        }

        private void MoveNarrowCamera(Vector2 newCameraPositionUnconstrained, Bounds cameraMovementConstraint, float time, LeanTweenType easeType)
        {
            Vector2 newCameraPositionConstrained =
                VectorUtils.ClampPointToBounds(newCameraPositionUnconstrained, cameraMovementConstraint);
            
            Vector3 cameraTo = new Vector3(newCameraPositionConstrained.x, newCameraPositionConstrained.y, -10f);

            if (cameraTo != _narrowCamera.transform.position)
            {
                FireNarrowCameraTargetChanged(cameraTo, cameraMovementConstraint);
                _isAnimatingCamera = true;
                LeanTween.move(_narrowCamera.gameObject, cameraTo, time).setEase(easeType).setOnComplete(() =>
                {
                    _isAnimatingCamera = false;
                });
            }
        }

        private void FireNarrowCameraTargetChanged(Vector2 cameraTo, Bounds contractedBounds)
        {
            Dictionary<GridDirection, bool> canCameraMoveInDirection = new Dictionary<GridDirection, bool>
            {
                { GridDirection.N, cameraTo.y < contractedBounds.center.y + contractedBounds.extents.y },
                { GridDirection.S, cameraTo.y > contractedBounds.center.y - contractedBounds.extents.y },
                { GridDirection.E, cameraTo.x < contractedBounds.center.x + contractedBounds.extents.x },
                { GridDirection.W, cameraTo.x > contractedBounds.center.x - contractedBounds.extents.x }
            };
            
            NarrowCameraTargetChanged?.Invoke(cameraTo, canCameraMoveInDirection);
        }

        public bool IsWithinNarrowCameraBounds(Bounds tileBounds)
        {
            Bounds narrowCameraBounds = _narrowCameraMinimapAreaGraphic.bounds;
            return narrowCameraBounds.Contains(tileBounds.min) && narrowCameraBounds.Contains(tileBounds.max);
        }
    }
}
