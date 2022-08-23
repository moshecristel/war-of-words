using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarOfWords
{
    // Officially recognized set of MapBoardSelections
    public class MapBoardSelectionPerimeter
    {
        public List<MapBoardSelection> Selections { get; set; }
        public int SelectionCount => Selections?.Count ?? 0;

        public MapLetterTile TerminalLetterTileStart { get; set; }
        public MapLetterTile TerminalLetterTileEnd { get; set; }
        public List<MapLetterTile> TerminalLetterTiles
        {
            get
            {
                if (TerminalLetterTileStart == null || TerminalLetterTileEnd == null) return null;
                return new List<MapLetterTile>
                {
                    TerminalLetterTileStart,
                    TerminalLetterTileEnd
                };
            }
        }

        public bool IsComplete { 
            get
            {
                if (TerminalLetterTiles == null || TerminalLetterTiles.Count == 0) return false;
                return TerminalLetterTiles[0] == TerminalLetterTiles[1];
            }
        }

        public void AddCompletedSelection(MapBoardSelection selection)
        {
            if (!IsComplete && CanBeExtendedBy(selection))
            {
                Selections ??= new List<MapBoardSelection>();

                if (Selections?.Count > 0)
                {
                    // 4 scenarios
                    
                    // A. selection STARTS on start of perimeter (selection: start == WordEdge, end == PerimeterEdge)
                    // C. selection STARTS on end of perimeter (selection: start == WordEdge, end == PerimeterEdge)
                    
                    // B. selection ENDS on start of perimeter (selection: start == PerimeterEdge, end == WordEdge)
                    // D. selection ENDS on end of perimeter (selection: start == PerimeterEdge, end == WordEdge)

                    TileSelectionType selectionStartType = TileSelectionType.None;
                    TileSelectionType selectionEndType = TileSelectionType.None;
                    
                    if (selection.SelectedLetterTiles.Contains(TerminalLetterTiles[0]))
                    {
                        // Append to BEGINNING of selections
                        Selections.Insert(0, selection);

                        if (selection.SelectedLetterTiles[0] == TerminalLetterTiles[0])
                        {
                            // A
                            selectionStartType = TileSelectionType.WordEdge;
                            selectionEndType = TileSelectionType.PerimeterEdge;
                        }
                        else
                        {
                            // B
                            selectionStartType = TileSelectionType.PerimeterEdge;
                            selectionEndType = TileSelectionType.WordEdge;
                        }
                    }
                    else if(selection.SelectedLetterTiles.Contains(TerminalLetterTiles[1])) // Should be true (verified by CanBeExtendedBy)
                    {
                        // Append to END of selections
                        Selections.Add(selection);

                        if (selection.SelectedLetterTiles[0] == TerminalLetterTiles[1])
                        {
                            // C
                            selectionStartType = TileSelectionType.WordEdge;
                            selectionEndType = TileSelectionType.PerimeterEdge;
                        }
                        else
                        {
                            // D
                            selectionStartType = TileSelectionType.PerimeterEdge;
                            selectionEndType = TileSelectionType.WordEdge;
                        }
                    }

                    selection.SelectedLetterTiles[0].SelectionType = selectionStartType;
                    selection.SelectedLetterTiles[^1].SelectionType = selectionEndType;
                    
                }
                else
                {
                    RegisterSelection(selection);
                }
            }
            else
            {
                throw new Exception("Oops, can't add to perimeter.");
            }
        }

        private void RegisterSelection(MapBoardSelection selection, bool insertAtBeginning = false)
        {
            
        }

        public bool IsATerminalLetterTile(MapLetterTile mapLetterTile)
        {
            bool isTerminal = false;
            if (TerminalLetterTiles is { Count: > 0 })
            {
                isTerminal = TerminalLetterTiles[0] == mapLetterTile || TerminalLetterTiles[1] == mapLetterTile;
            }

            Debug.Log("IsATerminalLetterTile: " + isTerminal);
            return isTerminal;
        }

        public bool IsAnIntermediateLetterTile(MapLetterTile mapLetterTile)
        {
            return !IsATerminalLetterTile(mapLetterTile) && Contains(mapLetterTile);
        }

        public bool Contains(MapLetterTile mapLetterTile)
        {
            return Selections != null && Selections.Any(selection => selection.Contains(mapLetterTile));
        }

        public bool CanBeExtendedBy(MapBoardSelection selection)
        {
            if (SelectionCount == 0) return true;
            MapLetterTile first = selection.SelectedLetterTiles[0];
            MapLetterTile last = selection.SelectedLetterTiles[^1];
            return TerminalLetterTiles.Contains(first) || TerminalLetterTiles.Contains(last);
        }

        public void UpdateVisuals()
        {
            if (Selections == null) return;
            foreach (MapBoardSelection selection in Selections)
            {
                selection.UpdateVisuals();
            }
        }
    }
}

