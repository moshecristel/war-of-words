using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarOfWords
{
    public static class TileColorUtils
    {
        // 0 = Base, 1 = Soft Shadow, 2 = Drop Shadow
        public static List<Color> GetTileColors(TileColor tileColor)
        {
            switch (tileColor)
            {
                case TileColor.Standard:
                    return GetColors("#F7F1E8", "#E9E1D7", "#BEB2A4");
                case TileColor.Highlighted:
                    return GetColors("#FEFE8A", "#F6F47E", "#E0D548");
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileColor), tileColor, null);
            }
        }

        // 0 = Inner, 1 = Base, 2 = Drop Shadow, 3 = Outline
        public static List<Color> GetSelectionTileColors(TileSelectionType tileSelectionType, bool isVerified)
        {
            
            switch (tileSelectionType)
            {
                case TileSelectionType.VerifiedPerimeterEdge:
                    return GetColors("#E3FFD4", "#CEED97", "#B0CE6E", "#7E9350");   // Green
                case TileSelectionType.VerifiedWordEdge:
                    return GetColors("#FFF6D9", "#FFDF8C", "#FFBF66", "#FF9D62");   // Orange
                case TileSelectionType.VerifiedWordMiddle:
                    return GetColors("#FFFCD9", "#FFFF4D", "#E0D548", "#AD9D04");   // Yellow
                case TileSelectionType.UnverifiedEdge:
                    return GetColors("#FFFFFF", "#F2F2F2", "#CCCCCC", "#999999");   // White
                case TileSelectionType.UnverifiedMiddle:
                    return GetColors("#FFFFFF", "#F2F2F2", "#CCCCCC", "#999999");   // White
                case TileSelectionType.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileSelectionType), tileSelectionType, null);
            }
        }

        // 0 - Base, 1 - Drop Shadow, 2 - Outline
        public static List<Color> GetSelectionConnectionColors(bool isVerified)
        {
            return (isVerified) ?
                GetColors("#FFFF4D", "#E0D548", "#AD9D04") :
                GetColors("#F2F2F2", "#CCCCCC", "#999999");
                    
        }
        
        private static List<Color> GetColors(params string[] hexes)
        {
            return (new List<string>(hexes)).Select(hex =>
            {
                ColorUtility.TryParseHtmlString(hex, out var color);
                return color;
            }).ToList();
        }
    }
    
}
