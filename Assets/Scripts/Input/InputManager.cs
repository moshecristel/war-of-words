using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class InputManager : MonoBehaviour
{
    public static event Action<Vector2, Vector2> TouchStarted;
    public static event Action<Vector2, Vector2> TouchMoved;
    public static event Action<Vector2, Vector2> TouchEnded;

    private Camera _cameraMain;

    #region Lifecycle

    private void Awake()
    {
        _cameraMain = Camera.main;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        Touch.onFingerDown += TouchOnFingerDown;
        Touch.onFingerUp += TouchOnFingerUp;
        Touch.onFingerMove += TouchOnFingerMove;
    }


    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        TouchSimulation.Disable();
        Touch.onFingerDown -= TouchOnFingerDown;
        Touch.onFingerUp -= TouchOnFingerUp;
        Touch.onFingerMove -= TouchOnFingerMove;
    }
    #endregion

    #region Event Handlers
    
    private void TouchOnFingerDown(Finger finger)
    {
        Debug.Log("Finger DOWN: " + finger.currentTouch.screenPosition + "(began=" + finger.currentTouch.began + ")");
        TouchStarted?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    }
    
    private void TouchOnFingerMove(Finger finger)
    {
        Debug.Log("Finger MOVE: " + finger.currentTouch.screenPosition);
        TouchMoved?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    }
    
    private void TouchOnFingerUp(Finger finger)
    {
        Debug.Log("Finger UP: " + finger.currentTouch.screenPosition + "(ended=" + finger.currentTouch.ended + ")");
        TouchEnded?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    }
    
    #endregion

    #region Methods

    private Vector2 ScreenToWorldPosition(Vector2 screenPosition)
    {
        return _cameraMain.ScreenToWorldPoint(screenPosition);
    }

    #endregion
}
