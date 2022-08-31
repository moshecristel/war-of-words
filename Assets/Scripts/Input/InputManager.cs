using System;
using DigitalRubyShared;
using UnityEngine;
using WarOfWords;

public class InputManager : Singleton<InputManager>
{
    // Only send world position
    public static event Action<Vector2> TouchStarted;
    public static event Action<Vector2> TouchMoved;
    public static event Action<Vector2> TouchEnded;
    
    [SerializeField] private FingersJoystickScript _fingersJoystick;
    [SerializeField] private CircleCollider2D _joystickMask;

    [SerializeField] private GameObject _panPanel;
    
    private PanGestureRecognizer _panGesture;
    private ScaleGestureRecognizer _scaleGesture;

    private bool _isPanning;
    private bool _isDoublePan;
    private bool _isJoysticking;

    #region Lifecycle

    private void Awake()
    {
        // _touchControls = new TouchControls();
        _fingersJoystick.JoystickExecuted = JoystickExecuted;
    }

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        _panGesture = new PanGestureRecognizer();
        _panGesture.PlatformSpecificView = _panPanel;
        _panGesture.MaximumNumberOfTouchesToTrack = 2;
        _panGesture.StateUpdated += PanGesture_OnStateUpdated;
        FingersScript.Instance.AddGesture(_panGesture);

        _scaleGesture = new ScaleGestureRecognizer();
        _scaleGesture.PlatformSpecificView = _panPanel;
        _scaleGesture.StateUpdated += ScaleGesture_OnStateUpdated;
        FingersScript.Instance.AddGesture(_scaleGesture);
        
        FingersScript.Instance.AddMask(_joystickMask, _fingersJoystick.PanGesture);
    }

    private void OnDisable()
    {
        if (FingersScript.HasInstance)
        {
            FingersScript.Instance.RemoveGesture(_panGesture);
            FingersScript.Instance.RemoveMask(_joystickMask, _fingersJoystick.PanGesture);
        }
    }

    private void PanGesture_OnStateUpdated(GestureRecognizer pan)
    {
        if (_isJoysticking) return;

        _isPanning = pan.State == GestureRecognizerState.Began || (_isPanning && pan.State == GestureRecognizerState.Executing);
        if (!_isPanning)
        {
            _isDoublePan = false;
            return;
        }

        _isDoublePan = pan.CurrentTrackedTouches.Count > 1;
        
        Debug.Log($"PAN: {pan.State}, focus = {pan.FocusX}, {pan.FocusY}, tracked touches=" + pan.CurrentTrackedTouches.Count + " touches: " + pan.TrackedTouchCountIsWithinRange);
    }
    
    private void ScaleGesture_OnStateUpdated(GestureRecognizer scale)
    {
        if (scale.State == GestureRecognizerState.Began)
        {
            Debug.Log("BEGAN Scaling: " + scale.State + " " + _scaleGesture.ScaleMultiplier);

        } else if (scale.State == GestureRecognizerState.Executing)
        {
            Debug.Log("EXECUTING Scaling: " + scale.State + " " + _scaleGesture.ScaleMultiplier);
        }
        
    }
    
    

    public void HiButtonClicked()
    {
        Debug.Log("Hi!");
    }
    #endregion

    #region Event Handlers
    
    private void JoystickExecuted(FingersJoystickScript script, Vector2 amount)
    {
        if (_isPanning) return; 
        
        _isJoysticking = script.Executing;
        if (script.Executing)
        {
            print("Amt=" + amount + " script executing=" + script.Executing);
        }
        else
        {
            print("Joystick complete");
        }
    }
    #endregion
}
