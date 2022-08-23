using System.Collections.Generic;

namespace WarOfWords
{
    /// <summary>
    /// A string of ordered letter tiles that constitute an attempted word.  Initially it is "unverified" (we don't know
    /// if it represents a word or not) and can be marked as "verified" once it is complete.
    /// </summary>
    public class MapBoardSelection
    {
        public List<MapLetterTile> SelectedLetterTiles { get; set; }

        public int Length => SelectedLetterTiles.Count;

        public bool IsVerified
        {
            get => _isVerified;
            set
            {
                _isVerified = value;
                MarkVerifiedVisuals();
            }
        }

        private bool _isVerified;

        public MapLetterTile TerminalTile
        {
            get
            {
                if (SelectedLetterTiles == null || SelectedLetterTiles.Count == 0) return null;
                if (SelectedLetterTiles[0].SelectionType == TileSelectionType.PerimeterEdge)
                    return SelectedLetterTiles[0];
                if (SelectedLetterTiles[^1].SelectionType == TileSelectionType.PerimeterEdge)
                    return SelectedLetterTiles[^1];

                return null;
            }
        }

        public MapBoardSelection(MapLetterTile initialTile)
        {
            initialTile.Select();
            SelectedLetterTiles = new List<MapLetterTile> { initialTile };
        }

        // If you're adding to a selection we can assume that its first and last tile are of selection type PerimeterEdge
        // Since if it is the first word it is an unverified PerimeterEdge and if it is an extension it is the last verified PerimeterEdge
        public void AddLetterTile(MapLetterTile letterTileToAdd)
        {
            if (SelectedLetterTiles.Contains(letterTileToAdd)) return;
            
            MapLetterTile previousLetterTile = SelectedLetterTiles[^1];
            
            GridDirection selectionDirection =
                CoordUtils.GetRelativeAdjacentGridDirection(previousLetterTile.MapLetter.Coords,
                    letterTileToAdd.MapLetter.Coords);

            if (selectionDirection == GridDirection.None)
                return;

            GridDirection oppositeOfSelectionDirection = CoordUtils.GetOpposingDirection(selectionDirection);
                
            previousLetterTile.SelectionType = SelectedLetterTiles.Count == 1 ? TileSelectionType.PerimeterEdge : TileSelectionType.WordMiddle;
            previousLetterTile.Select();
            previousLetterTile.OutgoingConnection = selectionDirection;
            
            letterTileToAdd.SelectionType = TileSelectionType.PerimeterEdge;
            letterTileToAdd.Select();
            letterTileToAdd.IncomingConnection = oppositeOfSelectionDirection;
            
            SelectedLetterTiles.Add(letterTileToAdd);
        }

        public bool Contains(MapLetterTile mapLetterTile)
        {
            foreach (MapLetterTile selectedLetterTile in SelectedLetterTiles)
            {
                if (selectedLetterTile == mapLetterTile) return true;
            }

            return false;
        }

        private void MarkVerifiedVisuals()
        {
            foreach (MapLetterTile selectedLetterTile in SelectedLetterTiles)
            {
                selectedLetterTile.IsVerifiedSelection = true;
                selectedLetterTile.UpdateVisuals();
            }
        }

        public void UpdateVisuals()
        {
            foreach (MapLetterTile selectedLetterTile in SelectedLetterTiles)
            {
                selectedLetterTile.UpdateVisuals();
            }
        }
    }
}
