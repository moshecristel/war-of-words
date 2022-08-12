using UnityEngine;

namespace WarOfWords
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private Camera _narrowCamera;
        [SerializeField] private Camera _wideCamera;

        public void SwitchToWideCamera(Vector2 position)
        {
            _wideCamera.gameObject.SetActive(true);
            _narrowCamera.gameObject.SetActive(false);

            _wideCamera.transform.position = new Vector3(position.x, position.y, -10);
        }

        public void SwitchToNarrowCamera(Vector2 position)
        {
            _narrowCamera.gameObject.SetActive(true);
            _wideCamera.gameObject.SetActive(false);

            _narrowCamera.transform.position = new Vector3(position.x, position.y, -10);
        }
    }
}
