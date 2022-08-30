using System.Collections.Generic;
using UnityEngine;
using WarOfWords;

public class CollectToStatusPath : MonoBehaviour
{
    [SerializeField] private LeanTweenPath _ltPath;

    [SerializeField] private Transform _pathStart;
    [SerializeField] private Transform _pathEnd;
    [SerializeField] private Transform[] _controlsAndMidpoints;
    [SerializeField] private Transform[] _controlsAndMidpointsForVariation;

    // The relative percent delta in both directions relative to start (lower right start) and endpoint (upper left end)
    private List<Vector2> _controlsAndMidpointsPercentOffsets = new();
    
    private float _tweenSeconds;
    private float _scaleDelay;
    private float _startScale;
    private float _endScale;

    private Vector2 _start;
    private Vector2 _end;

    public static void MultiTween(LeanTweenType tweenType, Transform canvasTransform, GameObject collectPrefab, CollectToStatusPath pathPrefab, Transform start, Transform end, int n, 
        float tweenSeconds = 1f, float scaleDelay = 0.5f, float startScale = 1f, float endScale = 1.5f,
        float maxTimeVariation = 0.2f, float maxMidpointPercentVariation = 0.0f)
    {
        float timeVariationPerPath = (maxTimeVariation * 2f) / n;
        List<Vector2> midpointVariations = GetMidpointVariations(n, start.position, end.position, maxMidpointPercentVariation);
        
        for (int i = 0; i < n; i++)
        {
            CollectToStatusPath path = Instantiate(pathPrefab, canvasTransform);
            
            float timeVariation = -maxTimeVariation + (i * timeVariationPerPath);
            float currTweenSeconds = tweenSeconds - timeVariation;
            path.Tween(collectPrefab, start, end, tweenType, midpointVariations[i], currTweenSeconds, scaleDelay, startScale, endScale);
        }
    }

    private static List<Vector2> GetMidpointVariations(int n, Vector2 start, Vector2 end, float maxMidpointPercentVariation)
    {
        float totalX = start.x - end.x;
        float totalY = end.y - start.y;
        float midpointPercentVariationPerPath = (maxMidpointPercentVariation * 2f) / n;

        List<Vector2> midpointVariations = new();
        for (int i = 0; i < n; i++)
        {
            float midpointPercentVariation = -maxMidpointPercentVariation + (midpointPercentVariationPerPath * i);
            Vector2 midpointVariation = new Vector2(midpointPercentVariation * totalX, midpointPercentVariation * totalY);
            midpointVariations.Add(midpointVariation);
        }
        
        midpointVariations.Shuffle();
        return midpointVariations;
    }

    public void Tween(GameObject collectPrefab, Transform start, Transform end, LeanTweenType tweenType = LeanTweenType.easeInCubic, Vector2 midpointVariation = default, float tweenSeconds = 1f, float scaleDelay = 0.5f, float startScale = 1f, float endScale = 1.5f)
    {
        _start = start.position;
        _end = end.position;

        _pathStart.position = _start;
        _pathEnd.position = _end;

        ResetControls();

        if (midpointVariation != default)
        {
            foreach (Transform t in _controlsAndMidpointsForVariation)
            {
                t.position += (Vector3)midpointVariation;
            }
        }

        _tweenSeconds = tweenSeconds;
        _scaleDelay = scaleDelay;
        _startScale = startScale;
        _endScale = endScale;

        GameObject obj = Instantiate(collectPrefab, transform);
        obj.transform.localScale = new Vector3(_startScale, _startScale, 1);
        obj.transform.position = start.position;
        LeanTween.move(obj, _ltPath.vec3, _tweenSeconds).setEase(tweenType).setOnComplete(Destroy);
        if(_tweenSeconds > _scaleDelay)
            LeanTween.scale(obj, Vector2.one * _endScale, _tweenSeconds - _scaleDelay).setDelay(_scaleDelay);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }

    private void ResetControls()
    {
        float totalX = _start.x - _end.x;
        float totalY = _end.y - _start.y;
        
        float leftMostX = _end.x;
        float downMostY = _start.y;

        for (int i = 0; i < _controlsAndMidpoints.Length; i++)
        {
            Vector2 percentOffsets = _controlsAndMidpointsPercentOffsets[i];
            _controlsAndMidpoints[i].position = new Vector2(leftMostX + (totalX * percentOffsets.x),
                downMostY + (totalY * percentOffsets.y));
        }
    }

    private void Awake()
    {
        // Record relative positions of controls and points
        RecordOffsets();
    }

    private void RecordOffsets()
    {
        float totalX = _pathStart.position.x - _pathEnd.position.x;
        float totalY = _pathEnd.position.y - _pathStart.position.y;
        
        // POSITIVE percentage is ABOVE (Y) end point and RIGHT (X) of start point
        // NEGATIVE percentage is BELOW (Y) start point and to LEFT (X) of end point
        
        //              1+ Y
        //      (0, 1)         (1, 1)
        //       END-------------
        //        |             |
        //  -X    |             |     1+ X
        //        |             |
        //        |             |  
        //       (0, 0)--------START (1, 0)
        // 
        //              -Y
        //

        float leftMostX = _pathEnd.position.x;
        float downMostY = _pathStart.position.y;

        foreach (Transform t in _controlsAndMidpoints)
        {
            float xOffsetPercent = (t.position.x - leftMostX) / totalX;
            float yOffsetPercent = (t.position.y - downMostY) / totalY;
            
            _controlsAndMidpointsPercentOffsets.Add(new Vector2(xOffsetPercent, yOffsetPercent));
        }
    }
}
