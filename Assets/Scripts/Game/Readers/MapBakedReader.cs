using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    public class MapBakedReader
    {
        public static Map LoadNewMapFromData(State state)
        {
            // Multiple maps per file with '#' prefixed lines
            // File begins with '# [State Name]'
            // Then '# [Avg Words Per Tile (float)]
            // Then grid of lines with '+' (null tile) or 'A' (uppercase character), etc.
            TextAsset textAsset = Resources.Load<TextAsset>($"Map/Baked/{state}");
            string[] lines = textAsset.text.Split("\n");

            List<List<string>> allMapsData = new List<List<string>>();

            bool started = false;

            List<string> currentMapData = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#"))
                {
                    if (currentMapData.Count > 0)
                    {
                        allMapsData.Add(currentMapData);
                        currentMapData = new List<string>();
                    }
                }
                else
                {
                    currentMapData.Add(lines[i]);
                }
            }

            if (currentMapData.Count > 0)
            {
                allMapsData.Add(currentMapData);
            }
            
            return new Map(state, allMapsData[0]);
        }
    }
}
