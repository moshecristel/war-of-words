using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    public static class MapShapesReader
    {
        private static List<Color> _cityColors = new List<Color>
        {
            ColorUtils.GetColor("#ff0000"),     // 1 - Red (Largest City)
            ColorUtils.GetColor("#00ff00"),     // 2 - Green
            ColorUtils.GetColor("#0000ff"),     // 3 - Blue
            ColorUtils.GetColor("#ffff00"),     // 4 - Yellow
            ColorUtils.GetColor("#ff00ff"),     // 5 - Magenta
            ColorUtils.GetColor("#00ffff"),     // 6 - Cyan
            ColorUtils.GetColor("#ffffff"),     // 7 - White
            ColorUtils.GetColor("#ff8000"),     // 8 - Orange
            ColorUtils.GetColor("#0080ff"),     // 9 - Dark Blue
            ColorUtils.GetColor("#80ff00"),     // 10 - Bright Green
            ColorUtils.GetColor("#8000ff"),     // 11 - Purple
            ColorUtils.GetColor("#ff0080"),     // 12 - Bright Pink
            ColorUtils.GetColor("#800080"),     // 13 - Light Purple
            ColorUtils.GetColor("#808000"),     // 14 - Olive Green
            ColorUtils.GetColor("#333333"),     // 15 - Dark Gray
            ColorUtils.GetColor("#000000"),     // 16 - Black (Capital)
        };
        
        public static Map LoadNewMapFromShape(State state, bool initWeighted = true)
        {
            Texture2D mapTexture = Resources.Load<Texture2D>($"Map/Shapes/{state}");
            
            short[,] fullMapShape = new short[mapTexture.width, mapTexture.height];
            for (int y = mapTexture.height - 1; y >= 0; y--)
            {
                for (int x = 0; x < mapTexture.width; x++)
                {
                    int newY = mapTexture.height - 1 - y;
                    Color c = mapTexture.GetPixel(x, y);
                    fullMapShape[x, newY] = (short)(c.a < 0.3f ? -1 : 0);

                    if (fullMapShape[x, newY] >= 0)
                    {
                        for (short i = 0; i < _cityColors.Count; i++)
                        {
                            Color color = _cityColors[i];
                            if (color.r == c.r && color.g == c.g && color.b == c.b)
                            {
                                fullMapShape[x, newY] = (short)(i + 1);
                            }
                        }
                    }
                }
            }

            Dictionary<int,string> numToCityName = LoadCitiesFromData(state);
            short[,] truncatedMapShape = Truncate(fullMapShape);


            Dictionary<Vector2Int, string> coordToCityName = new();
            for (int x = 0; x < truncatedMapShape.GetLength(0); x++)
            {
                for (int y = 0; y < truncatedMapShape.GetLength(1); y++)
                {
                    if (truncatedMapShape[x, y] > 0)
                    {
                        coordToCityName[new Vector2Int(x, y)] = numToCityName[truncatedMapShape[x, y]];
                    }
                }
            }

            return new Map(state, truncatedMapShape, coordToCityName, initWeighted);
        }

        public static Dictionary<int, string> LoadCitiesFromData(State state)
        {
            Dictionary<int, string> numToCityName = new();
            TextAsset textAsset = Resources.Load<TextAsset>($"Map/Data/{state}");
            string[] lines = textAsset.text.Split("\n");
            foreach (string line in lines)
            {
                string[] tokens = line.Split("|");
                int num = int.Parse(tokens[0]);
                numToCityName[num] = tokens[1];
            }

            return numToCityName;
        }

        

        private static short[,] Truncate(short[,] mapShape)
        {
            int maxX = -1, maxY = -1, minX = -1, minY = -1;

            for (int y = 0; y < mapShape.GetLength(1); y++)
            {
                for (int x = 0; x < mapShape.GetLength(0); x++)
                {
                    if (mapShape[x, y] >= 0)
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

            short[,] truncatedMapShape = new short[cols, rows];
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
