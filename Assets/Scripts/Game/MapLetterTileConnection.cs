using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    public class MapLetterTileConnection : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _base;
        [SerializeField] private SpriteRenderer _dropShadow;
        [SerializeField] private SpriteRenderer _outline;
        [SerializeField] private GridDirection _direction;
        [SerializeField] private MapLetterTile _sourceTile;

        public MapLetterTile DestinationTile { get; set; }

        private bool _isVerified;
        public bool IsVerified
        {
            get => _isVerified;
            set
            {
                _isVerified = value;
                UpdateVisuals();
            }
        }

        public MapLetterTile SourceTile => _sourceTile;
        public GridDirection Direction => _direction;
        public bool IsSelected => DestinationTile != null;

        public void SetSelected(MapLetterTile destinationTile, bool isVerified = false)
        {
            IsVerified = isVerified;
            DestinationTile = destinationTile;
            UpdateVisuals();
        }

        public void Deselect()
        {
            DestinationTile = null;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            _base.gameObject.SetActive(IsSelected);
            
            if(_dropShadow != null)
                _dropShadow.gameObject.SetActive(IsSelected);
            
            _outline.gameObject.SetActive(IsSelected);
            
            List<Color> colors = TileColorUtils.GetSelectionConnectionColors(_isVerified);

            _base.color = colors[0];
            
            // Some lines don't have a drop shadow
            if(_dropShadow != null)
                _dropShadow.color = colors[1];
            
            _outline.color = colors[2];
        }
    }
}
