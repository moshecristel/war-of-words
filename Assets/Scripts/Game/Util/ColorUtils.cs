using UnityEngine;

namespace WarOfWords
{
    public static class ColorUtils
    {
        public static Color GetColor(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out var color) ? color : default;
        }
    }
}
