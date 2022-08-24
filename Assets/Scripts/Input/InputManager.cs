using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using WarOfWords;

public class InputManager : Singleton<InputManager>
{
    // Only send world position
    public static event Action<Vector2> TouchStarted;
    public static event Action<Vector2> TouchMoved;
    public static event Action<Vector2> TouchEnded;

    [SerializeField]
    private Camera _narrowCamera;
    
    [SerializeField]
    private Camera _wideCamera;

    private TouchControls _touchControls;
    private bool _fingerDown;

    #region Lifecycle

    private void Awake()
    {
        _touchControls = new TouchControls();
    }

    private void Start()
    {
        _touchControls.Touch.TouchPress.started += OnTouchStartedReceived;
        _touchControls.Touch.TouchPosition.performed += OnTouchMoveReceived;
        _touchControls.Touch.TouchPress.canceled += OnTouchCanceledReceived;
    }

    private void OnEnable()
    {
        _touchControls.Enable();
    }
    
    private void OnDisable()
    {
        _touchControls.Disable();
    }
    #endregion

    #region Event Handlers
    
        private void OnTouchStartedReceived(InputAction.CallbackContext context)
        {
            StartCoroutine(WaitAndPoll());
        }

        IEnumerator WaitAndPoll()
        {
            yield return new WaitForEndOfFrame();

            // Don't register touch if we're over a UI element
            if (EventSystem.current.currentSelectedGameObject != null) yield break;
            _fingerDown = true;
                
            Vector2 touchScreenPosition = _touchControls.Touch.TouchPosition.ReadValue<Vector2>();
            TouchStarted?.Invoke(ScreenToWorldPosition(touchScreenPosition));
        }
        
        private void OnTouchMoveReceived(InputAction.CallbackContext context)
        {
            if (!_fingerDown) return;
            Vector2 touchScreenPosition = context.ReadValue<Vector2>();
            TouchMoved?.Invoke(ScreenToWorldPosition(touchScreenPosition));
        }
        
        private void OnTouchCanceledReceived(InputAction.CallbackContext context)
        {
            _fingerDown = false;
            
            Vector2 touchScreenPosition = _touchControls.Touch.TouchPosition.ReadValue<Vector2>();
            TouchEnded?.Invoke(ScreenToWorldPosition(touchScreenPosition));
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
