using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using WarOfWords;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class InputManager : Singleton<InputManager>
{
    public static event Action<Vector2, Vector2> TouchStarted;
    public static event Action<Vector2, Vector2> TouchMoved;
    public static event Action<Vector2, Vector2> TouchEnded;

    [SerializeField]
    private Camera _narrowCamera;
    
    [SerializeField]
    private Camera _wideCamera;

    #region Lifecycle

    private void Awake()
    {
        
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
        TouchStarted?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    }
    
    private void TouchOnFingerMove(Finger finger)
    {
        TouchMoved?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    }
    
    private void TouchOnFingerUp(Finger finger)
    {
        TouchEnded?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    }
    
    #endregion

    #region Methods

    private Vector2 ScreenToWorldPosition(Vector2 screenPosition)
    {
        return _narrowCamera.isActiveAndEnabled ? 
            _narrowCamera.ScreenToWorldPoint(screenPosition) : 
            _wideCamera.ScreenToWorldPoint(screenPosition);
    }

    #endregion
}
