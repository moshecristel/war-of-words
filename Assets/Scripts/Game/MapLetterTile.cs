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

        [SerializeField] private SpriteRenderer _miniMapFocusedTile;
        [SerializeField] private SpriteRenderer _miniMapUnfocusedTile;

        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private GameObject[] _bonusLabels;
        
        // Correspond to enum values (CW from N): N = 0, NE = 1...
        [SerializeField] private GameObject[] _selectionLines;

        
        public MapLetter MapLetter { get; set; }
        public TileOwnership TileOwnership { get; set; }
        
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (!_isSelected) IsVerifiedSelection = false;
            } 
        }
        public bool IsVerifiedSelection { get; set; }
        public TileSelectionType SelectionType { get; set; } = TileSelectionType.None;

        public List<GridDirection> OutgoingConnections { get; set; } = new();

        public BonusType BonusType { get; set; } = BonusType.None;
        public float Points { get; set; }
        
        #region Lifecycle

            private void Awake()
            {
                UpdateVisuals();
            }

            private void LateUpdate()
            {
                // Check "focus" of minimap tile sprite based on whether it is in narrow camera bounds
                UpdateMinimapVisuals(CameraManager.Instance.IsWithinNarrowCameraBounds(_miniMapFocusedTile.bounds));
            }

        #endregion

        #region Selection        
            public void Select(TileSelectionType selectionType)
            {
                _isSelected = true;
                SelectionType = selectionType;
                
                
            }

            public void Deselect()
            {
                _isSelected = false;
                IsVerifiedSelection = false;
                SelectionType = TileSelectionType.None;
                OutgoingConnections = new List<GridDirection>();
            }
        #endregion

        #region Setters

            public void SetBonus(BonusType bonusType)
            {
                BonusType = bonusType;
            }

            public void SetColor(TileColor tileColor)
            {
                List<Color> tileColors = TileColorUtils.GetTileColors(tileColor);
                _baseSpriteRenderer.color = tileColors[0];
                _softShadowSpriteRenderer.color = tileColors[1];
                _dropShadowSpriteRenderer.color = tileColors[2];
            }
        #endregion


        #region Update Visuals
        
            /// <summary>
            /// Driven by the LateUpdate cycle of the MapBoard and its perimeter
            /// </summary>
            public void UpdateVisuals()
            {
                UpdateMainVisuals();
                UpdateSelectionVisuals();
            }
        
            private void UpdateSelectionTypeVisuals()
            {
                _selectionParent.gameObject.SetActive(SelectionType != TileSelectionType.None);
                if (SelectionType == TileSelectionType.None) return;

                List<Color> selectionColors = TileColorUtils.GetSelectionTileColors(SelectionType, IsVerifiedSelection);
                _selectionInnerSpriteRenderer.color = selectionColors[0];
                _selectionBaseSpriteRenderer.color = selectionColors[1];
                _selectionDropshadowSpriteRenderer.color = selectionColors[2];
                _selectionOutlineSpriteRenderer.color = selectionColors[3];
            }
            
            private void UpdateSelectionVisuals()
            {
                // Unhighlight all
                foreach (GameObject highlightLine in _selectionLines)
                {
                    highlightLine.SetActive(false);
                }

                UpdateSelectionTypeVisuals();
                if (!_isSelected)
                {
                    return;
                }

                // Each tile is responsible only for its outgoing connection
                foreach (GameObject line in _selectionLines)
                {
                    line.SetActive(false);
                }

                foreach (GridDirection outgoingDirection in OutgoingConnections)
                {
                    _selectionLines[(int)outgoingDirection].SetActive(true);
                }
            }
            
            private void UpdateMainVisuals()
            {
                if(MapLetter != null)
                    _letterText.text = MapLetter.Character;


                foreach (GameObject bonusLabel in _bonusLabels)
                {
                    bonusLabel.SetActive(false);
                }
                
                if (BonusType != BonusType.None)
                {
                    _bonusLabels[((int)BonusType)-1].SetActive(true);    
                }
                
                pointsText.transform.parent.gameObject.SetActive(Points != 0);
                pointsText.gameObject.SetActive(Points != 0);
                pointsText.text = $"{Points:n1}";
                
                // if (TileOwnership == null)
                // {
                //     SetColor(TileColor.Standard);
                //     return;
                // }
                //
                // if (TileOwnership.IsCurrentPlayer)
                // {
                //     SetColor(TileColor.Highlighted);
                //     _claimCountText.transform.parent.gameObject.SetActive(TileOwnership.ClaimCount > 1);
                //     _claimCountText.text = $"{TileOwnership.ClaimCount:n0}";
                // }
            }
            
            /// <summary>
            /// Driven by LateUpdate cycle (affected by camera position due to masking shortcomings)
            /// </summary>
            private void UpdateMinimapVisuals(bool isFocused)
            {
                _miniMapFocusedTile.gameObject.SetActive(isFocused);
                _miniMapUnfocusedTile.gameObject.SetActive(!isFocused);
            }

        #endregion
    }
}
