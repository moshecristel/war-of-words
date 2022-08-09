using UnityEngine;
using WarOfWords;

public class MapBoard : MonoBehaviour
{
    private LetterTile _lastLetterTile;
    
    private void Awake()
    {
        InputManager.TouchStarted += InputManagerOnTouchStarted;
        InputManager.TouchMoved += InputManagerOnTouchMoved;
        InputManager.TouchEnded += InputManagerOnTouchEnded;
    }

    private void InputManagerOnTouchStarted(Vector2 screenPosition, Vector2 worldPosition)
    {
        CheckForTile(worldPosition);
    }
    
    private void InputManagerOnTouchMoved(Vector2 screenPosition, Vector2 worldPosition)
    {
        CheckForTile(worldPosition);
    }
    
    private void InputManagerOnTouchEnded(Vector2 screenPosition, Vector2 worldPosition)
    {
        CheckForTile(worldPosition);   
    }

    private void CheckForTile(Vector2 worldPosition)
    {
        Collider2D circle = Physics2D.OverlapCircle(worldPosition, 0.05f);
        if (circle != null)
        {
            _lastLetterTile = circle.gameObject.GetComponentInParent<LetterTile>();
            _lastLetterTile.IsSelected = true;
        }
        else
        {
            if (_lastLetterTile != null)
            {
                _lastLetterTile.IsSelected = false;
                _lastLetterTile = null;
            }
        }
    } 
}
