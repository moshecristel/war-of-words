using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarOfWords
{
    /// <summary>
    /// Manages all selections including:
    /// 1. Verified Selections - Confirmed words that have properly extended the perimeter)
    /// 2. Current Selection - An unconfirmed sequence (if present) that may or may not be a word or properly extend
    ///                        the perimeter but has not violated any of the constraints such as including an already
    ///                        selected letter in the perimeter.
    ///
    ///  
    /// </summary>
    public class MapBoardSelectionPerimeter
    {
        public List<MapBoardSelection> VerifiedSelections { get; set; } = new();
        private List<bool> _reversedFlags = new(); 
        
        public MapBoardSelection CurrentSelection { get; set; }

        public MapLetterTile TerminalVerifiedStartTile { get; set; }
        public MapLetterTile TerminalVerifiedEndTile { get; set; }
        public List<MapLetterTile> TerminalVerifiedTiles
        {
            get
            {
                if (TerminalVerifiedStartTile == null || TerminalVerifiedEndTile == null) return null;
                return new List<MapLetterTile>
                {
                    TerminalVerifiedStartTile,
                    TerminalVerifiedEndTile
                };
            }
        }

        public bool IsTerminalTile(MapLetterTile letterTile)
        {
            return letterTile == TerminalVerifiedStartTile || letterTile == TerminalVerifiedEndTile;
        }
        
        public bool IsComplete { get; set; }

        public bool AddLetterTileToCurrentSelection(MapLetterTile letterTile)
        {
            // If the PERIMETER IS COMPLETE, don't add
            if (IsComplete)
            {
                Debug.Log("Can't add due to complete");
                return false;
            }
            
            // If this is a DUPLICATE IN CURRENT SELECTION, don't add
            if (CurrentSelection != null && CurrentSelection.Contains(letterTile))
            {
                Debug.Log("Can't add due to contained in selection");
                return false;
            }
            else if (!IsTerminalTile(letterTile) && Contains(letterTile, true)) // Go ahead and add if DUPLICATE TERMINAL VERIFIED
            {
                // If this is a DUPLICATE INTERMEDIATE VERIFIED (non-terminal), don't add
                // (We've already dealt with it above if it's terminal)
                Debug.Log("Can't add due to duplicate intermediate");
                return false;
            }
            else if (CurrentSelection == null)
            {
                Debug.Log("Creating new CurrentSelection with " + letterTile.MapLetter.Character);
                // NEW CURRENT SELECTION 
                CurrentSelection = new MapBoardSelection(letterTile);
                UpdateConnections();
                return true;
            }
            else if (CurrentSelection.CanBeExtendedBy(letterTile))
            {
                // ADD TO CURRENT SELECTION 
                Debug.Log("Adding " + letterTile.MapLetter.Character + " to " + CurrentSelection.ToCharacterSequence());
                CurrentSelection.AddTile(letterTile);
                UpdateConnections();
                return true;
            }

            Debug.Log("Can't add for some other reason");
            return false;
        }

        /// <summary>
        /// Adds the CurrentSelection to the perimeter if it is valid and marks the tiles verified.
        /// Return true if it was successful and false otherwise.
        ///
        /// CurrentSelection remains unchanged if the merge is unsuccessful (must be deselected elsewhere)
        /// </summary>
        public bool MergeCurrent()
        {
            if (CurrentSelection == null || CurrentSelection.LetterTileCount == 0)
            {
                Debug.Log("Not merging due to no current selection");
                return false;
            }
            
            bool isMerged = false;
            if (VerifiedSelections.Count == 0)
            {
                Debug.Log("Merged with 0 verified selections");
                // FIRST VERIFIED SELECTION
                VerifiedSelections.Add(CurrentSelection);
                _reversedFlags.Add(false);
                isMerged = true;
            } 
            else if (TerminalVerifiedEndTile == CurrentSelection.LetterTiles[0])
            {
                Debug.Log("Merged: Valid END extension");
                // Valid END extension
                VerifiedSelections.Add(CurrentSelection);
                _reversedFlags.Add(false);
                isMerged = true;
            } 
            else if (TerminalVerifiedEndTile == CurrentSelection.LetterTiles[^1])
            {
                Debug.Log("Merged: Valid END extension (REVERSED)");
                // Valid END extension (REVERSED)
                VerifiedSelections.Add(CurrentSelection);
                _reversedFlags.Add(true);
                isMerged = true;
            }
            else if (TerminalVerifiedStartTile == CurrentSelection.LetterTiles[^1])
            {
                Debug.Log("Merged: Valid START extension");
                // Valid START extension
                VerifiedSelections.Insert(0, CurrentSelection);
                _reversedFlags.Insert(0, false);
                isMerged = true;
            }
            else if (TerminalVerifiedStartTile == CurrentSelection.LetterTiles[0])
            {
                Debug.Log("Merged: Valid START extension (REVERSED)");
                // Valid START extension (REVERSED)
                VerifiedSelections.Insert(0, CurrentSelection);
                _reversedFlags.Insert(0, true);
                isMerged = true;
            }

            if (isMerged)
            {
                Debug.Log("MERGED!");
                // Mark verified
                CurrentSelection.IsVerified = true;
                UpdateSelectionTypes();
                UpdateTerminalTiles();

                CurrentSelection = null;
                
                UpdateConnections();
            }

            return isMerged;
        }

        /// <summary>
        /// Run:
        /// 1. On Merge (because direction of selection may change in context of perimeter)
        /// 2. On CurrentSelection Change (because connection between terminal tile and CurrentSelection may have changed)
        /// </summary>
        
        private void UpdateConnections()
        {
            var orderedVerifiedTiles = GetOrderedVerifiedTiles();
            for (int i = 0; i < orderedVerifiedTiles.Count - 1; i++)
            {
                MapLetterTile current = orderedVerifiedTiles[i];
                MapLetterTile next = orderedVerifiedTiles[i + 1];

                GridDirection outgoingDirection = CoordUtils.GetRelativeAdjacentGridDirection(current.MapLetter.Coords, next.MapLetter.Coords);
                
                // This should override any multiple connections added when the tile was in the CurrentSelection
                current.OutgoingConnections = new List<GridDirection> { outgoingDirection };
            }

            List<MapLetterTile> currentTiles = CurrentSelection != null ? CurrentSelection.GetTiles() : new List<MapLetterTile>();
            
            //? T
            // if (CurrentSelection != null && CurrentSelection.LetterTileCount > 0 && orderedVerifiedTiles.Count > 0)
            // {    
            //         
            //     // If the CurrentSelection is connected to a terminal tile, make sure that the proper connections are made
            //     if (TerminalVerifiedTiles.Contains(CurrentSelection.LetterTiles[0]) ||
            //         TerminalVerifiedTiles.Contains(CurrentSelection.LetterTiles[^1]))
            //     {
            //         // NOTE: Perimeter is connected in a START --outgoing--> X --outgoing--> END manner
            //         // So if the current selection is at the beginning, no perimeter connections need modifying
            //         // But if the current selection is at the end, the TerminalVerifiedEndTile needs an outgoing connection
            //         
            //         // Connected to perimeter
            //         bool currentIsReversed = TerminalVerifiedEndTile == CurrentSelection.LetterTiles[^1] || TerminalVerifiedStartTile == CurrentSelection.LetterTiles[^1];
            //         currentTiles = CurrentSelection.GetTiles(currentIsReversed);
            //         
            //         if(!currentTiles[^1].IsVerifiedSelection)
            //         {
            //             currentTiles[^1].OutgoingConnection = CoordUtils.GetRelativeAdjacentGridDirection(currentTiles[^1].MapLetter.Coords,
            //                 TerminalVerifiedStartTile.MapLetter.Coords);
            //         }
            //     }    
            // }
            
            for (int i = 0; i < currentTiles.Count - 1; i++)
            {
                GridDirection direction = CoordUtils.GetRelativeAdjacentGridDirection(currentTiles[i].MapLetter.Coords,
                    currentTiles[i + 1].MapLetter.Coords);
                
                // Only a CurrentSelection will add an outgoing connection to a tile to avoid the complications of 
                // ordering a current selection to align with the perimeter while it is in flux
                if(!currentTiles[i].OutgoingConnections.Contains(direction))
                    currentTiles[i].OutgoingConnections.Add(direction);     
            }
        }

        private List<MapLetterTile> GetOrderedVerifiedTiles()
        {
            List<MapLetterTile> orderedTiles = new();
            for (int i = 0; i < VerifiedSelections.Count; i++)
            {
                orderedTiles.AddRange(VerifiedSelections[i].GetTiles(_reversedFlags[i]));
            }

            return orderedTiles;
        }

        private void UpdateTerminalTiles()
        {
            if (VerifiedSelections.Count == 0)
            {
                TerminalVerifiedStartTile = TerminalVerifiedEndTile = null;
                return;
            }
            
            MapBoardSelection firstSelection = VerifiedSelections[0];
            MapBoardSelection lastSelection = VerifiedSelections[^1];

            TerminalVerifiedStartTile = !_reversedFlags[0] ? firstSelection.LetterTiles[0] : firstSelection.LetterTiles[^1];
            TerminalVerifiedEndTile =
                !_reversedFlags[^1] ? lastSelection.LetterTiles[^1] : lastSelection.LetterTiles[0];
            
            Debug.Log("Terminal Start: " + TerminalVerifiedStartTile.MapLetter.Character + ", Terminal End: " + TerminalVerifiedEndTile.MapLetter.Character);
        }

        private void UpdateSelectionTypes()
        {
            if (VerifiedSelections.Count == 0) return;
            if (VerifiedSelections.Count == 1)
            {
                for (int i = 0; i < VerifiedSelections[0].LetterTiles.Count; i++)
                {
                    VerifiedSelections[0].LetterTiles[i].SelectionType =
                        i == 0 || i == VerifiedSelections[0].LetterTiles.Count - 1
                            ? TileSelectionType.VerifiedPerimeterEdge
                            : TileSelectionType.VerifiedWordMiddle;
                }
                return;
            }
            
            // 2 or more in verified selection
            for (int i = 0; i < VerifiedSelections.Count; i++)
            {
                MapBoardSelection selection = VerifiedSelections[i];
                
                // Default all tiles to VerifiedWordMiddle
                foreach (MapLetterTile letterTile in selection.LetterTiles)
                {
                    letterTile.SelectionType = TileSelectionType.VerifiedWordMiddle;
                }

                if (i == 0)
                {
                    if (_reversedFlags[i])
                    {
                        // FIRST => WordEdge
                        // LAST => PerimeterEdge
                        selection.LetterTiles[0].SelectionType = TileSelectionType.VerifiedWordEdge;
                        selection.LetterTiles[^1].SelectionType = TileSelectionType.VerifiedPerimeterEdge;
                        
                    }
                    else
                    {
                        // FIRST => PerimeterEdge
                        // LAST => WordEdge
                        selection.LetterTiles[0].SelectionType = TileSelectionType.VerifiedPerimeterEdge;
                        selection.LetterTiles[^1].SelectionType = TileSelectionType.VerifiedWordEdge;
                        
                    }
                } 
                else if (i == VerifiedSelections.Count - 1)
                {
                    if (_reversedFlags[i])
                    {
                        // FIRST => PerimeterEdge
                        // LAST => WordEdge
                        selection.LetterTiles[0].SelectionType = TileSelectionType.VerifiedPerimeterEdge;
                        selection.LetterTiles[^1].SelectionType = TileSelectionType.VerifiedWordEdge;
                        
                    }
                    else
                    {
                        // FIRST => WordEdge
                        // LAST => PerimeterEdge
                        selection.LetterTiles[0].SelectionType = TileSelectionType.VerifiedWordEdge;
                        selection.LetterTiles[^1].SelectionType = TileSelectionType.VerifiedPerimeterEdge;
                        
                    }
                }
                else
                {
                    // Middle selection has WordEdge on both ends
                    selection.LetterTiles[0].SelectionType = TileSelectionType.VerifiedWordEdge;
                    selection.LetterTiles[^1].SelectionType = TileSelectionType.VerifiedWordEdge;
                }
            }
        } 

        private bool Contains(MapLetterTile letterTile, bool verifiedOnly = false)
        {
            if (VerifiedSelections.Any(verifiedSelection => verifiedSelection.Contains(letterTile)))
            {
                return true;
            }

            if (verifiedOnly) return false;
            return CurrentSelection != null && CurrentSelection.Contains(letterTile);
        }

        public void DeselectCurrent()
        {
            if (CurrentSelection == null) return;
            CurrentSelection.Deselect();
            CurrentSelection.UpdateVisuals();
            CurrentSelection = null;
            
            UpdateConnections();
        }

        public void UpdateVisuals()
        {
            foreach (MapBoardSelection verifiedSelection in VerifiedSelections)
            {
                verifiedSelection.UpdateVisuals();
            }

            CurrentSelection?.UpdateVisuals();
        }

        public void Print()
        {
            string log = "Verified: ";
            for (int i = 0; i < VerifiedSelections.Count; i++)
            {
                string selectionString = VerifiedSelections[i].ToCharacterSequence();
                if (_reversedFlags[i])
                    selectionString = new string(selectionString.Reverse().ToArray());
                log += selectionString + " --> ";
            }
            Debug.Log(log);
            Debug.Log("Current: " + (CurrentSelection == null ? "<none>" : CurrentSelection.ToCharacterSequence()));
        }
    }
}

