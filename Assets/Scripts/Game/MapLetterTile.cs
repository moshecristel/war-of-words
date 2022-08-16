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

        [SerializeField] private SpriteRenderer _miniMapFocusedTile;
        [SerializeField] private SpriteRenderer _miniMapUnfocusedTile;
        
        [SerializeField] private Sprite _tanTileSprite;
        [SerializeField] private Sprite _yellowTileSprite;
        [SerializeField] private Sprite _blueTileSprite;
        [SerializeField] private Sprite _redTileSprite;

        // Correspond to enum values (CW from N): N = 0, NE = 1...
        [SerializeField] private GameObject[] _highlightLines;
        [SerializeField] private GameObject _highlightCircle;
        
        private MapLetter _mapLetter;
        public MapLetter MapLetter
        {
            get => _mapLetter;
            set {
                _mapLetter = value;
                UpdateMainTile();
            }
        }
        
        private Party _party;
        public Party Party
        {
            get => _party;
            set {
                _party = value;
                UpdateMainTile();
            }
        }

        private bool _isSelected;
        public bool IsSelected => _isSelected;

        private GridDirection _incomingHighlightDirection;
        public GridDirection IncomingHighlightDirection => _incomingHighlightDirection;
        
        private GridDirection _outgoingHighlightDirection;
        public GridDirection OutgoingHighlightDirection => _outgoingHighlightDirection;

        private void LateUpdate()
        {
            // Check "focus" of minimap tile sprite based on whether it is in narrow camera bounds
            SetMinimapState(CameraManager.Instance.IsWithinNarrowCameraBounds(_miniMapFocusedTile.bounds));
        }

        public void Select(GridDirection incomingDirection = GridDirection.None)
        {
            _isSelected = true;
            _incomingHighlightDirection = incomingDirection;
            _outgoingHighlightDirection = GridDirection.None;
            UpdateHighlight();
        }

        public void SelectOutgoing(GridDirection outgoingDirection)
        {
            _isSelected = true;
            _outgoingHighlightDirection = outgoingDirection;
            UpdateHighlight();
        }

        public void Deselect()
        {
            _isSelected = false;
            _incomingHighlightDirection = GridDirection.None;
            UpdateHighlight();
        }

        private void UpdateMainTile()
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

        private void UpdateHighlight()
        {
            // Unhighlight all
            _highlightCircle.SetActive(false);
            foreach (GameObject highlightLine in _highlightLines)
            {
                highlightLine.SetActive(false);
            }

            if (!_isSelected) return;
            
            _highlightCircle.SetActive(true);
            if (_incomingHighlightDirection != GridDirection.None)
            {
                _highlightLines[(int)_incomingHighlightDirection].SetActive(true);
            }

            if (_outgoingHighlightDirection != GridDirection.None)
            {
                _highlightLines[(int)_outgoingHighlightDirection].SetActive(true);
            }
        }

        private void SetMinimapState(bool isFocused)
        {
            _miniMapFocusedTile.gameObject.SetActive(isFocused);
            _miniMapUnfocusedTile.gameObject.SetActive(!isFocused);
        }
    }
}
