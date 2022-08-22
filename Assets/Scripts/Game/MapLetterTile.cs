using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WarOfWords
{
    public class MapLetterTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _baseSpriteRenderer;
        [SerializeField] private SpriteRenderer _softShadowSpriteRenderer;
        [SerializeField] private SpriteRenderer _dropShadowSpriteRenderer;

        [SerializeField] private GameObject _selectionParent;
        [SerializeField] private SpriteRenderer _selectionInnerSpriteRenderer;
        [SerializeField] private SpriteRenderer _selectionBaseSpriteRenderer;
        [SerializeField] private SpriteRenderer _selectionDropshadowSpriteRenderer;
        [SerializeField] private SpriteRenderer _selectionOutlineSpriteRenderer;

        [SerializeField] private TMP_Text _letterText;
        [SerializeField] private TMP_Text _claimCountText;

        [SerializeField] private SpriteRenderer _miniMapFocusedTile;
        [SerializeField] private SpriteRenderer _miniMapUnfocusedTile;

        // Correspond to enum values (CW from N): N = 0, NE = 1...
        [SerializeField] private GameObject[] _selectionLines;
        
        private MapLetter _mapLetter;
        public MapLetter MapLetter
        {
            get => _mapLetter;
            set {
                _mapLetter = value;
                UpdateMainTile();
            }
        }
        
        private TileOwnership _tileOwnership;
        public TileOwnership TileOwnership
        {
            get => _tileOwnership;
            set {
                _tileOwnership = value;
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
            UpdateSelection();
        }

        public void SelectOutgoing(GridDirection outgoingDirection)
        {
            _isSelected = true;
            _outgoingHighlightDirection = outgoingDirection;
            UpdateSelection();
        }

        public void Deselect()
        {
            _isSelected = false;
            _incomingHighlightDirection = GridDirection.None;
            UpdateSelection();
        }

        public void UpdateMainTile()
        {
            if(MapLetter != null)
                _letterText.text = MapLetter.Character;
            
            if (_tileOwnership == null)
            {
                SetColor(TileColor.Standard);
                return;
            }
            
            if (_tileOwnership.IsCurrentPlayer)
            {
                SetColor(TileColor.Highlighted);
                _claimCountText.transform.parent.gameObject.SetActive(_tileOwnership.ClaimCount > 1);
                _claimCountText.text = $"{_tileOwnership.ClaimCount:n0}";
            }
        }

        private void SetColor(TileColor tileColor)
        {
            List<Color> tileColors = TileColorUtils.GetTileColors(tileColor);
            _baseSpriteRenderer.color = tileColors[0];
            _softShadowSpriteRenderer.color = tileColors[1];
            _dropShadowSpriteRenderer.color = tileColors[2];
        }

        private void SetSelectionType(TileSelectionType tileSelectionType)
        {
            _selectionParent.gameObject.SetActive(tileSelectionType != TileSelectionType.None);
            if (tileSelectionType == TileSelectionType.None) return;

            List<Color> selectionColors = TileColorUtils.GetSelectionTileColors(tileSelectionType);
            _selectionInnerSpriteRenderer.color = selectionColors[0];
            _selectionBaseSpriteRenderer.color = selectionColors[1];
            _selectionDropshadowSpriteRenderer.color = selectionColors[2];
            _selectionOutlineSpriteRenderer.color = selectionColors[3];
        }

        private void UpdateSelection()
        {
            // Unhighlight all
            foreach (GameObject highlightLine in _selectionLines)
            {
                highlightLine.SetActive(false);
            }

            if (!_isSelected)
            {
                SetSelectionType(TileSelectionType.None);
                return;
            }

            TileSelectionType tileSelectionType = TileSelectionType.WordMiddle;
            if (_incomingHighlightDirection == GridDirection.None)
            {
                tileSelectionType = TileSelectionType.PerimeterEdge;
            } else if (_outgoingHighlightDirection == GridDirection.None)
            {
                tileSelectionType = TileSelectionType.WordEdge;
            }
            SetSelectionType(tileSelectionType);
            
            if (_outgoingHighlightDirection != GridDirection.None)
            {
                _selectionLines[(int)_outgoingHighlightDirection].SetActive(true);
            }
        }

        private void SetMinimapState(bool isFocused)
        {
            _miniMapFocusedTile.gameObject.SetActive(isFocused);
            _miniMapUnfocusedTile.gameObject.SetActive(!isFocused);
        }
    }
}
