using System;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class CollectToStatusPath : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;            // TODO Animation?
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

    private Random _random;

    public static void MultiTween(Transform canvasTransform, CollectToStatusPath pathPrefab, Transform start, Transform end, int n, 
        float tweenSeconds = 1f, float scaleDelay = 0.5f, float startScale = 1f, float endScale = 1.5f,
        float maxTimeVariation = 0.2f, float maxMidpointVariation = 0.0f)
    {
        Random random = new Random((uint)DateTime.Now.Millisecond);

        float variationPerPath = (maxTimeVariation * 2f) / n;
        
        print($"variationPerPath={variationPerPath}");
        
        for (int i = 0; i < n; i++)
        {
            CollectToStatusPath path = Instantiate(pathPrefab, canvasTransform);
            
            float midpointVariation = 0f;
            if (!Mathf.Approximately(0f, maxMidpointVariation))
            {
                midpointVariation = random.NextFloat(0f, maxMidpointVariation);
            }

            float timeVariation = -maxTimeVariation + (i * variationPerPath);
            print($"{i} = timeVariation={timeVariation}");
            
            float currTweenSeconds = tweenSeconds - timeVariation;
            path.Tween(start, end, midpointVariation, currTweenSeconds, scaleDelay, startScale, endScale);
        }
    }

    public void Tween(Transform start, Transform end, float midpointVariation = 0f, float tweenSeconds = 1f, float scaleDelay = 0.5f, float startScale = 1f, float endScale = 1.5f)
    {
        _start = start.position;
        _end = end.position;

        _pathStart.position = _start;
        _pathEnd.position = _end;

        ResetControls();

        // if (!Mathf.Approximately(0f, midpointVariation))
        // {
        //     float totalX = _start.x - _end.x;
        //     float totalY = _end.y - _start.y;
        //     
        //     float x = totalX - _random.NextFloat(0f, 2 * midpointVariation);
        //     float y = totalY - _random.NextFloat(0f, 2 * midpointVariation);
        //     Vector2 variation = new Vector2(x, y);
        //     
        //     foreach (Transform t in _controlsAndMidpointsForVariation)
        //     {
        //         t.position += (Vector3)variation;
        //     }
        // }

        _tweenSeconds = tweenSeconds;
        _scaleDelay = scaleDelay;
        _startScale = startScale;
        _endScale = endScale;

        GameObject obj = Instantiate(_prefab, transform);
        obj.transform.localScale = new Vector3(_startScale, _startScale, 1);
        obj.transform.position = start.position;
        LeanTween.move(obj, _ltPath.vec3, _tweenSeconds).setEase(LeanTweenType.easeInCubic).setOnComplete(Destroy);
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
        
        _random = new Random((uint)DateTime.Now.Millisecond);
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
