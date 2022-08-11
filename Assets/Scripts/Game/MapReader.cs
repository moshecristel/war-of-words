using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    public static class MapReader
    {
        public static Map LoadNewMapFromShape(State state, bool initWeighted = true)
        {
            Texture2D mapTexture = Resources.Load<Texture2D>($"Map/Shapes/{state}");
            
            bool[,] fullMapShape = new bool[mapTexture.width, mapTexture.height];
            for (int y = 0; y < mapTexture.height; y++)
            {
                for (int x = 0; x < mapTexture.width; x++)
                {
                    Color c = mapTexture.GetPixel(x, y);
                    fullMapShape[x, mapTexture.height - y - 1] = c.a > 0.5f;
                }
            }

            return new Map(Truncate(fullMapShape), initWeighted);
        }

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
            
            // TODO Store list of map data somewhere official
            return new Map(allMapsData[0]);
        }

        private static bool[,] Truncate(bool[,] mapShape)
        {
            int maxX = -1, maxY = -1, minX = -1, minY = -1;

            for (int y = 0; y < mapShape.GetLength(1); y++)
            {
                for (int x = 0; x < mapShape.GetLength(0); x++)
                {
                    if (mapShape[x, y])
                    {
                        if (minX == -1 || x < minX) minX = x;
                        if (minY == -1 || y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            int cols = maxX - minX + 1;
            int rows = maxY - minY + 1;

            bool[,] truncatedMapShape = new bool[cols, rows];
            for (int origY = minY; origY <= maxY; origY++)
            {
                int y = origY - minY;
                
                for (int origX = minX; origX <= maxX; origX++)
                {
                    int x = origX - minX;

                    truncatedMapShape[x, y] = mapShape[origX, origY];
                }
            }

            return truncatedMapShape;
        }
    }
}
