using System;
using DigitalRubyShared;
using UnityEngine;
using WarOfWords;

public class InputManager : Singleton<InputManager>
{
    // 0 - Started, Moved or Ended, 1 - World Position
    public static event Action<InputState, Vector2> TapStateChanged;
    public static event Action<InputState, Vector2> PanStateChanged;
    public static event Action<InputState, Vector2> DoublePanStateChanged;
    public static event Action<InputState, Vector2> JoystickStateChanged;
    
    // 0 - Started, Moved or Ended, 1 - Scale Multiplier
    public static event Action<InputState, float> ScaleStateChanged;
    
    [SerializeField] private FingersJoystickScript _fingersJoystick;
    [SerializeField] private CircleCollider2D _joystickMask;
    [SerializeField] private GameObject _panScalePanel;

    private TapGestureRecognizer _tapGesture;
    private PanGestureRecognizer _panGesture;
    private ScaleGestureRecognizer _scaleGesture;

    private InputType _currentInputType;

    #region Lifecycle

        private void Awake()
        {
            _fingersJoystick.JoystickExecuted = FingersJoystick_JoystickExecuted;
        }
        
        private void OnEnable()
        {
            _tapGesture = new TapGestureRecognizer();
            _tapGesture.PlatformSpecificView = _panScalePanel;
            _tapGesture.StateUpdated += TapGesture_OnStateUpdated;
            FingersScript.Instance.AddGesture(_tapGesture);

            _panGesture = new PanGestureRecognizer();
            _panGesture.PlatformSpecificView = _panScalePanel;
            _panGesture.MaximumNumberOfTouchesToTrack = 2;
            _panGesture.ThresholdUnits = 0.01f;
            _panGesture.StateUpdated += PanGesture_OnStateUpdated;
            FingersScript.Instance.AddGesture(_panGesture);
            
            _scaleGesture = new ScaleGestureRecognizer();
            _scaleGesture.PlatformSpecificView = _panScalePanel;
            _scaleGesture.StateUpdated += ScaleGesture_OnStateUpdated;
            FingersScript.Instance.AddGesture(_scaleGesture);
            
            FingersScript.Instance.AddMask(_joystickMask, _fingersJoystick.PanGesture);
        }

        private void OnDisable()
        {
            if (!FingersScript.HasInstance) return;
            
            FingersScript.Instance.RemoveGesture(_tapGesture);
            FingersScript.Instance.RemoveGesture(_panGesture);
            FingersScript.Instance.RemoveGesture(_scaleGesture);
            FingersScript.Instance.RemoveMask(_joystickMask, _fingersJoystick.PanGesture);
        }
    #endregion

    #region Event Handlers
    
        private void TapGesture_OnStateUpdated(GestureRecognizer tap)
        {
            Debug.Log("Tap gesture: " + tap.State);

            // Exit if another input type is currently underway
            // No such thing as a current input type of tap since Ended is the only thing we care about
            if (_currentInputType != InputType.None) return;
            
            Vector2 tapFocusWorld = ToWorld(tap.FocusX, tap.FocusY);

            if (tap.State == GestureRecognizerState.Ended)
            {
                TapStateChanged?.Invoke(InputState.Ended, tapFocusWorld);
            }
        }
    
        private void PanGesture_OnStateUpdated(GestureRecognizer pan)
        {
            bool isCurrentlyPanning = _currentInputType is InputType.Pan or InputType.DoublePan;
            
            // Exit if another input type is currently underway
            if (_currentInputType != InputType.None && !isCurrentlyPanning) return;

            Vector2 panFocusScreen = new Vector2(pan.FocusX, pan.FocusY);

            bool isOnButton = UIRaycastUtils.PointerIsOverUIWithTag(panFocusScreen, "PanIgnore");
            Vector2 panFocusWorld = ToWorld(panFocusScreen);
            
            bool isInsideJoystickMask = _joystickMask.OverlapPoint(panFocusWorld);
            
            if( (!isInsideJoystickMask && pan.State == GestureRecognizerState.Began) || 
               (isCurrentlyPanning && pan.State == GestureRecognizerState.Executing))
            {
                InputState inputState =
                    pan.State == GestureRecognizerState.Began ? InputState.Started : InputState.Moved;
                
                if (pan.CurrentTrackedTouches.Count > 1)
                {
                    // Double pan, quit single pan if it is underway
                    if (_currentInputType == InputType.Pan)
                    {
                        // End current pan... not sure if this can happen without ending
                        PanStateChanged?.Invoke(InputState.Ended, panFocusWorld);
                    }
                    
                    _currentInputType = InputType.DoublePan;
                    DoublePanStateChanged?.Invoke(inputState, panFocusWorld);
                }
                else
                {
                    // Single pan, quit double pan if it is underway
                    if (_currentInputType == InputType.DoublePan)
                    {
                        DoublePanStateChanged?.Invoke(InputState.Ended, panFocusWorld);
                    }

                    _currentInputType = InputType.Pan;
                    PanStateChanged?.Invoke(inputState, panFocusWorld);
                }             
            } 
            else
            {
                // End current pan... not sure if this can happen without ending
                if (_currentInputType == InputType.Pan)
                {
                    PanStateChanged?.Invoke(InputState.Ended, panFocusWorld);
                }
                else if(_currentInputType == InputType.DoublePan)
                {
                    DoublePanStateChanged?.Invoke(InputState.Ended, panFocusWorld);
                }

                _currentInputType = InputType.None;
            }
            
            // Debug.Log($"PAN: {pan.State}, focus = {pan.FocusX}, {pan.FocusY}, tracked touches=" + pan.CurrentTrackedTouches.Count + " touches: " + pan.TrackedTouchCountIsWithinRange);
        }
        
        private void ScaleGesture_OnStateUpdated(GestureRecognizer scale)
        {
            bool isCurrentlyScaling = _currentInputType == InputType.Scale;
            
            // Exit if another input type is currently underway
            if (_currentInputType != InputType.None && !isCurrentlyScaling) return;

            if (scale.State is GestureRecognizerState.Began or GestureRecognizerState.Executing)
            {
                InputState inputState = _currentInputType == InputType.None ? InputState.Started : InputState.Moved;
                ScaleStateChanged?.Invoke(inputState, _scaleGesture.ScaleMultiplier);
            }
            else
            {
                if (isCurrentlyScaling)
                {
                    ScaleStateChanged?.Invoke(InputState.Ended, _scaleGesture.ScaleMultiplier);
                }

                _currentInputType = InputType.None;
            }
        }
        
        private void FingersJoystick_JoystickExecuted(FingersJoystickScript fingersJoystick, Vector2 moveAmount)
        {
            bool isCurrentlyJoysticking = _currentInputType == InputType.Joystick;
            
            // Exit if another input type is currently underway
            if (_currentInputType != InputType.None && !isCurrentlyJoysticking) return;
            
            if (fingersJoystick.Executing)
            {
                InputState inputState = _currentInputType == InputType.None ? InputState.Started : InputState.Moved;
                _currentInputType = InputType.Joystick;
                JoystickStateChanged?.Invoke(inputState, moveAmount);
            }
            else
            {
                if (isCurrentlyJoysticking)
                {
                    JoystickStateChanged?.Invoke(InputState.Ended, moveAmount);
                }

                _currentInputType = InputType.None;
            }
        }


        private Vector2 ToWorld(float screenPositionX, float screenPositionY)
        {
            return ToWorld(new Vector2(screenPositionX, screenPositionY));
        }
        
        private Vector2 ToWorld(Vector2 screenPosition)
        {
            return CameraManager.Instance.ScreenToWorldPosition(screenPosition);
        }
    #endregion
}
