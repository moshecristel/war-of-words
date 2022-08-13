using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    public class MapBoardSelection
    {
        public List<MapLetterTile> SelectedLetterTiles { get; set; }

        public MapBoardSelection(MapLetterTile initialTile)
        {
            initialTile.Select();
            SelectedLetterTiles = new List<MapLetterTile> { initialTile };
        }

        public void AddLetterTile(MapLetterTile letterTile)
        {
            if (SelectedLetterTiles.Contains(letterTile)) return;
            
            MapLetterTile previousLetterTile = SelectedLetterTiles[^1];
            
            GridDirection selectionDirection =
                CoordUtils.GetRelativeAdjacentGridDirection(previousLetterTile.MapLetter.Coords,
                    letterTile.MapLetter.Coords);

            if (selectionDirection == GridDirection.None)
                return;

            GridDirection opposingDirection = CoordUtils.GetOpposingDirection(selectionDirection);

            previousLetterTile.SelectOutgoing(selectionDirection);
            letterTile.Select(opposingDirection);
            SelectedLetterTiles.Add(letterTile);
        }

        public void Clear()
        {
            foreach (MapLetterTile letterTile in SelectedLetterTiles)
                letterTile.Deselect();
        }
    }
}
