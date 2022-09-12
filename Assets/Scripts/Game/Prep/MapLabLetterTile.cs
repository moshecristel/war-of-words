using TMPro;
using UnityEngine;

namespace WarOfWords
{
    public class MapLabLetterTile : MonoBehaviour
    {
        public MapLetter MapLetter { get; set; }
        
        [SerializeField] private TMP_Text _letterText;
        [SerializeField] private TMP_Text _wordCountText;
        [SerializeField] private SpriteRenderer _tileBaseSpriteRenderer;
        [SerializeField] private Gradient _wordDensityGradient;

        private float _maxTileWords = 100f;

        public void UpdateVisuals()
        {
            if (MapLetter != null)
            {
                _letterText.text = MapLetter.Character;

                float time = Mathf.Min((float)MapLetter.WordCount / _maxTileWords, 1f);
                _tileBaseSpriteRenderer.color = _wordDensityGradient.Evaluate(time);
                _wordCountText.text = $"{MapLetter.WordCount:n0}";
            }
            else
            {
                _wordCountText.text = "0";
                _tileBaseSpriteRenderer.color = Color.white;
            }
        }
    }
}
