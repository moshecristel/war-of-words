using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace WarOfWords
{
    public class AreaClaimedPopup : MonoBehaviour
    {
        public static event Action Closed;

        [SerializeField] private GameObject _popup;
        
        [BoxGroup("Perimeter Stats Components")]
        [SerializeField] private TMP_Text _claimedTilesText;
        [BoxGroup("Perimeter Stats Components")]
        [SerializeField] private TMP_Text _wordsText;
        [BoxGroup("Perimeter Stats Components")]
        [SerializeField] private TMP_Text _averageWordLengthText;
        [BoxGroup("Perimeter Stats Components")]
        [SerializeField] private TMP_Text _perimeterTilesText;
        [BoxGroup("Perimeter Stats Components")]
        [SerializeField] private TMP_Text _pointsText;
        [BoxGroup("Perimeter Stats Components")]
        [SerializeField] private TMP_Text _bonusPointsText;
        [BoxGroup("Perimeter Stats Components")]
        [SerializeField] private TMP_Text _bonusCoinsText;

        [BoxGroup("General Animation")]
        [SerializeField] private CollectToStatusPath _coinCollectToStatusPathPrefab;
        [BoxGroup("General Animation")]
        [SerializeField] private Transform _canvasTransform;
        [BoxGroup("General Animation")]
        [SerializeField] private int _n = 30;
        [BoxGroup("General Animation")]
        [SerializeField] private float _tweenSeconds = 1.25f;
        [BoxGroup("General Animation")]
        [SerializeField] private float _scaleDelay = 0.5f;
        [BoxGroup("General Animation")]
        [SerializeField] private float _startScale = 1f;
        [BoxGroup("General Animation")]
        [SerializeField] private float _endScale = 1.5f;
        [BoxGroup("General Animation")]
        [SerializeField] private float _maxTimeVariation = 0.3f;
        [BoxGroup("General Animation")]
        [SerializeField] private float _maxMidpointVariation = 0.3f;
        [BoxGroup("General Animation")]
        [SerializeField] private LeanTweenType _tweenType = LeanTweenType.easeInCubic;


        [BoxGroup("Points Animation")]
        [SerializeField] private GameObject _pointsPrefab;
        [BoxGroup("Points Animation")]
        [SerializeField] private Transform _pointsTweenStartTransform;
        [BoxGroup("Points Animation")]
        [SerializeField] private Transform _bonusPointsTweenStartTransform;
        [BoxGroup("Points Animation")]
        [SerializeField] private Transform _pointsTweenEndTransform;
        
        [BoxGroup("Coins Animation")]
        [SerializeField] private GameObject _coinsPrefab;
        [BoxGroup("Coins Animation")]
        [SerializeField] private Transform _bonusCoinsTweenStartTransform;
        [BoxGroup("Coins Animation")]
        [SerializeField] private Transform _bonusCoinsTweenEndTransform;


        public void DisplayWith(PerimeterStats perimeterStats)
        {
            gameObject.SetActive(true);
            
            SetStats(perimeterStats);
            PlayDisplayTween();
        }
        
        private void SetStats(PerimeterStats perimeterStats)
        {
            _claimedTilesText.text = $"{perimeterStats.ClaimedTiles:n0}";
            _wordsText.text = $"{perimeterStats.Words:n0}";
            _averageWordLengthText.text = $"{perimeterStats.AverageWordLength:f1}";
            _perimeterTilesText.text = $"{perimeterStats.Tiles:n0}";
            _pointsText.text = $"{perimeterStats.Points:n0}";
            _bonusPointsText.text = $"{perimeterStats.BonusPoints:n0}";
            _bonusCoinsText.text = $"{perimeterStats.BonusCoins:n0}";
        }

        private void PlayDisplayTween()
        {
            // _popup.transform.localScale = Vector2.one * 0.8f;
            // LeanTween.scale(_popup, Vector2.one, 1f)
            //     .setEaseOutElastic().setOnComplete(OnDisplayTweenComplete);
            OnDisplayTweenComplete();
        }

        private void OnDisplayTweenComplete()
        {
            StartCoroutine(PlayTweens(0, 0.4f));
        }

        IEnumerator PlayTweens(float delay1, float delay2)
        {
            yield return new WaitForSeconds(delay1);
            CollectToStatusPath.MultiTween(_tweenType, _canvasTransform, _pointsPrefab, _coinCollectToStatusPathPrefab, _pointsTweenStartTransform, _pointsTweenEndTransform, 
                _n, _tweenSeconds, _scaleDelay, _startScale, _endScale, _maxTimeVariation, _maxMidpointVariation);
            CollectToStatusPath.MultiTween(_tweenType, _canvasTransform, _pointsPrefab, _coinCollectToStatusPathPrefab, _bonusPointsTweenStartTransform, _pointsTweenEndTransform, 
                _n, _tweenSeconds, _scaleDelay, _startScale, _endScale, _maxTimeVariation, _maxMidpointVariation);
            
            yield return new WaitForSeconds(delay2);
            CollectToStatusPath.MultiTween(_tweenType, _canvasTransform, _coinsPrefab, _coinCollectToStatusPathPrefab, _bonusCoinsTweenStartTransform, _bonusCoinsTweenEndTransform, 
                _n, _tweenSeconds, _scaleDelay, _startScale, _endScale, _maxTimeVariation, _maxMidpointVariation);

        }
        
        
        

        // Run close animation and then fire
        public void Close()
        {
            
        }
    }
}
