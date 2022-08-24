using System.Collections.Generic;
using System.Linq;

namespace WarOfWords
{
    /// <summary>
    /// A sequence of ordered MapLetterTiles that have confirmed to be a valid selection but may or may not constitute
    /// a valid word.  This selection will be in one of two states:
    ///
    /// 1. Verified - (IsVerified == true) The selection is "locked in" as a verified word and a valid portion of some perimeter
    /// 2. Unverified - (IsVerified == false) The selection is still in process and, though it is not yet verified as a
    ///                 word, it hasn't violated any selection constraints such as including a tile that has already been selected
    ///                 as part of the active perimeter
    /// </summary>
    public class MapBoardSelection
    {
        public List<MapLetterTile> LetterTiles { get; } = new();
        public int LetterTileCount => LetterTiles?.Count ?? 0;

        public bool IsVerified
        {
            get => _isVerified;
            set
            {
                _isVerified = value;
                MarkTilesVerified();
            } 
        }

        private bool _isVerified;

        public MapBoardSelection(MapLetterTile initialTile)
        {
            AddTile(initialTile);
        }

        public bool Contains(MapLetterTile targetLetterTile)
        {
            return LetterTiles.Contains(targetLetterTile);
        }

        /// <summary>
        /// True if the given tile is:
        /// 1. NOT already present in the selection
        /// 2. Is adjacent to the last added letter in the tile
        /// </summary>
        public bool CanBeExtendedBy(MapLetterTile letterTile)
        {
            if (LetterTileCount == 0) return true;
            if (LetterTiles.Contains(letterTile)) return false;

            GridDirection relativeAdjacentGridDirection = CoordUtils.GetRelativeAdjacentGridDirection(LetterTiles[^1].MapLetter.Coords, letterTile.MapLetter.Coords);
            return relativeAdjacentGridDirection != GridDirection.None;
        }

        public string ToCharacterSequence()
        {
            return string.Join("", LetterTiles.Select(letterTile => letterTile.MapLetter.Character).ToList());
        }

        /// <summary>
        /// Add a tile to the end of this selection.  Assumes *** UNVERIFIED ***
        /// NOTE: Connections are managed by the MapBoardSelectionPerimeter
        /// </summary>
        public void AddTile(MapLetterTile letterTile)
        {
            if (LetterTileCount == 0)
            {
                // Don't need to check for adjacency since this is the first one
                LetterTiles.Add(letterTile);
            }
            else
            {
                // Don't add if the tile isn't adjacent to the previous tile
                MapLetterTile previousLetterTile = LetterTiles[^1];
                if (!CoordUtils.AreAdjacent(previousLetterTile.MapLetter.Coords, letterTile.MapLetter.Coords))
                    return;
                
                LetterTiles.Add(letterTile);   
            }
            
            // The tile might be a TERMINAL tile in a perimeter that is already selected
            // If so, just leave it as is
            if (letterTile.IsSelected) return;
            
            letterTile.Select(TileSelectionType.UnverifiedEdge);
            if (LetterTileCount >= 3)
            {
                // Most recently added tile before this is no longer the edge but the middle
                LetterTiles[^2].SelectionType = TileSelectionType.UnverifiedMiddle;
            }
        }

        public List<MapLetterTile> GetTiles(bool isReversed = false)
        {
            if (!isReversed) return LetterTiles;
            List<MapLetterTile> clonedLetterTiles = new List<MapLetterTile>(LetterTiles);
            clonedLetterTiles.Reverse();
            return clonedLetterTiles;
        }
        
        private void MarkTilesVerified()
        {
            foreach (MapLetterTile letterTile in LetterTiles)
            {
                letterTile.IsVerifiedSelection = true;
            }
        }

        public void Deselect()
        {
            foreach (MapLetterTile letterTile in LetterTiles)
            {
                if(!letterTile.IsVerifiedSelection) letterTile.Deselect();
            }
        }

        public void UpdateVisuals()
        {
            foreach (MapLetterTile letterTile in LetterTiles)
            {
                letterTile.UpdateVisuals();
            }
        }
    }
}
