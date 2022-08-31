using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRubyShared;
using Sirenix.OdinInspector;
using UnityEngine;
using WarOfWords;

public class Launcher : MonoBehaviour
{
    [SerializeField] private AreaClaimedPopup _areaClaimedPopup;
    
    [Button("Go")]
    private void Go()
    {
        // CollectToStatusPath.MultiTween(_tweenType, _canvasTransform, _coinCollectToStatusPathPrefab, _startTransform, _endTransform, _n, 
        //     _tweenSeconds, _scaleDelay, _startScale, _endScale, 
        //     _maxTimeVariation, _maxMidpointVariation);
        PerimeterStats stats = new PerimeterStats
        {
            AverageWordLength = 10.3f,
            BonusCoins = 37,
            BonusPoints = 234,
            ClaimedTiles = 989,
            Points = 2_344,
            Seconds = 0,
            SecondsPerTile = 0,
            Words = 15
        };
        
        _areaClaimedPopup.DisplayWith(stats);
    }

    public void OnHiClicked()
    {
        Debug.Log("Hi!");
    }
}
