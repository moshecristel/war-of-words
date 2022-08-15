using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    private TouchControls _touchControls;

    #region Lifecycle

    private void Awake()
    {
        _touchControls = new TouchControls();
    }

    private void Start()
    {
        _touchControls.Touch.TouchPress.started += Test;
    }

    public void OnClickMe()
    {
        Debug.Log("Click me!");
    }

    private void Test(InputAction.CallbackContext context)
    {
        Vector2 val = _touchControls.Touch.TouchPosition.ReadValue<Vector2>();
        StartCoroutine(WaitAndPoll());
    }

    IEnumerator WaitAndPoll()
    {
        yield return new WaitForEndOfFrame();
        Vector2 val = _touchControls.Touch.TouchPosition.ReadValue<Vector2>();

        if (EventSystem.current.currentSelectedGameObject != null)
        {
            Debug.Log("Selecting: " + EventSystem.current.currentSelectedGameObject.name);
        }
        else
        {
            Debug.Log("Selecting Map: " + val);
        }
    }

    private void OnEnable()
    {
        // EnhancedTouchSupport.Enable();
        // TouchSimulation.Enable();
        // Touch.onFingerDown += TouchOnFingerDown;
        // Touch.onFingerUp += TouchOnFingerUp;
        // Touch.onFingerMove += TouchOnFingerMove;
        _touchControls.Enable();
    }



    private void OnDisable()
    {
        // EnhancedTouchSupport.Disable();
        // TouchSimulation.Disable();
        // Touch.onFingerDown -= TouchOnFingerDown;
        // Touch.onFingerUp -= TouchOnFingerUp;
        // Touch.onFingerMove -= TouchOnFingerMove;
        _touchControls.Disable();
        
    }
    #endregion

    #region Event Handlers
    
    // private void TouchOnFingerDown(Finger finger)
    // {
    //     TouchStarted?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    // }
    //
    // private void TouchOnFingerMove(Finger finger)
    // {
    //     TouchMoved?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    // }
    //
    // private void TouchOnFingerUp(Finger finger)
    // {
    //     TouchEnded?.Invoke(finger.currentTouch.screenPosition, ScreenToWorldPosition(finger.currentTouch.screenPosition));
    // }
    
    
    
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
