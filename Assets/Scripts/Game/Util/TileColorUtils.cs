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
        public static List<Color> GetSelectionTileColors(TileSelectionType tileSelectionType)
        {
            switch (tileSelectionType)
            {
                case TileSelectionType.PerimeterEdge:
                    // return GetColors("#ffa503", "#e78000", "#e46704", "#666666");           // darker orange
                    // return GetColors("#E6F4C6", "#CEED97", "#B0CE6E", "#7E9350");
                    return GetColors("#e3ffd4", "#CEED97", "#B0CE6E", "#7E9350");
                case TileSelectionType.WordEdge:
                    
                    // return GetColors("#a9fd1e", "#8ee610", "#7dd902", "#666666");           // bright green
                
                    // return GetColors("#FFE5B8", "#FFD98D", "#E8B86B", "#A88356");
                    return GetColors("#fff6d9", "#ffdf8c", "#ffbf66", "#ff9d62");   // coin orange
                    // return GetColors("#fff6a9", "#ffdf8c", "#ff9d62", "#e8815c");   // coin orange
                case TileSelectionType.WordMiddle:
                    // return GetColors("#f7f94a", "#fad029", "#f9ba05", "#666666");           // yellow/orange
                    // return GetColors("#F2F2F2", "#FFFFFF", "#CCCCCC", "#988C34");   // White
                    // return GetColors("#FFFBAE", "#FFFF4D", "#E0D548", "#AD9D04");   // Bright Yellow
                    return GetColors("#fffcd9", "#FFFF4D", "#E0D548", "#AD9D04");   // Bright Yellow
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileSelectionType), tileSelectionType, null);
            }
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
