using System;
using UnityEngine;

namespace WarOfWords
{
    public class LetterTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        [SerializeField] private Sprite _tanTileSprite;
        [SerializeField] private Sprite _yellowTileSprite;
        
        [SerializeField] private Sprite _blueTileSprite;
        [SerializeField] private Sprite _redTileSprite;
        [SerializeField] private Sprite _purpleTileSprite;
        [SerializeField] private Sprite _greenTileSprite;
        [SerializeField] private Sprite _brownTileSprite;
        
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
                case Party.Libertarian:
                    _spriteRenderer.sprite = _purpleTileSprite;
                    break;
                case Party.Green:
                    _spriteRenderer.sprite = _greenTileSprite;
                    break;
                case Party.Constitution:
                    _spriteRenderer.sprite = _brownTileSprite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Party), Party, null);
            }
        }
    }
}
