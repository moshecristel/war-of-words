using UnityEngine;

namespace WarOfWords
{
    public static class CanvasExtensions
    {
        public static Matrix4x4 GetCanvasMatrix(this Canvas _Canvas)
        {
            RectTransform rectTr = _Canvas.transform as RectTransform;
            Matrix4x4 canvasMatrix = rectTr.localToWorldMatrix;
            canvasMatrix *= Matrix4x4.Translate(-rectTr.sizeDelta / 2);
            return canvasMatrix;
        }
    }
}
