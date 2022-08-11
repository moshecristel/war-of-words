using System;
using TMPro;
using UnityEngine;

namespace WarOfWords
{
    public class MapLetterTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [SerializeField] private TMP_Text _letterText;
        [SerializeField] private TMP_Text _letterCountText;
        [SerializeField] private TMP_Text _wordCountText;
        
        [SerializeField] private Sprite _tanTileSprite;
        [SerializeField] private Sprite _yellowTileSprite;
        [SerializeField] private Sprite _blueTileSprite;
        [SerializeField] private Sprite _redTileSprite;
        
        private MapLetter _mapLetter;
        public MapLetter MapLetter
        {
            get => _mapLetter;
            set {
                _mapLetter = value;
                UpdateSprite();
            }
        }
        
        private Party _party;
        public Party Party
        {
            get => _party;
            set {
                _party = value;
                UpdateSprite();
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            if (IsSelected)
            {
                _spriteRenderer.sprite = _yellowTileSprite;
                return;
            }
            
            switch (Party)
            {
                case Party.None:
                    _spriteRenderer.sprite = _tanTileSprite;
                    break;
                case Party.Democrat:
                    _spriteRenderer.sprite = _blueTileSprite;
                    break;
                case Party.Republican:
                    _spriteRenderer.sprite = _redTileSprite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Party), Party, null);
            }

            if(MapLetter != null)
                _letterText.text = MapLetter.Character;
            
            _letterCountText.gameObject.SetActive(MapLetter.WordStartsTotalLetterCount != 0);
            _wordCountText.gameObject.SetActive(MapLetter.WordStarts != 0);

            _letterCountText.text = $"{MapLetter.WordStartsTotalLetterCount:n0}";
            _wordCountText.text = $"{MapLetter.WordStarts:n0}";
            
        }
    }
}
