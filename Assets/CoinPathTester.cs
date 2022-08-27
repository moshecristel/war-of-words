using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CoinPathTester : MonoBehaviour
{
    [SerializeField] private CollectToStatusPath _coinCollectToStatusPathPrefab;
    [SerializeField] private Transform _canvasTransform;
    [SerializeField] private Transform _startTransform;
    [SerializeField] private Transform _endTransform;

    [SerializeField] private int _n = 10;
    [SerializeField] private float _tweenSeconds = 1f;
    [SerializeField] private float _scaleDelay = 0.6f;
    [SerializeField] private float _startScale = 1f;
    [SerializeField] private float _endScale = 1.5f;
    [SerializeField] private float _maxTimeVariation = 0.4f;
    [SerializeField] private float _maxMidpointVariation = 0f;

    [Button("Go")]
    private void Go()
    {
        CollectToStatusPath.MultiTween(_canvasTransform, _coinCollectToStatusPathPrefab, _startTransform, _endTransform, _n, 
            _tweenSeconds, _scaleDelay, _startScale, _endScale, 
            _maxTimeVariation, _maxMidpointVariation);
    }
}
