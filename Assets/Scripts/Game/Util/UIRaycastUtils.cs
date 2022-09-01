using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WarOfWords
{
    public static class UIRaycastUtils
    {
        public static bool PointerIsOverUI(Vector2 screenPos)
        {
            var hitObject = UIRaycast(ScreenPosToPointerData(screenPos));
            return hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI");
        }
        
        public static bool PointerIsOverUIWithTag(Vector2 screenPos, string tag)
        {
            var hitObject = UIRaycast(ScreenPosToPointerData(screenPos));
            return hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI") && hitObject.tag == tag;
        }
 
        static GameObject UIRaycast (PointerEventData pointerData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
 
            return results.Count < 1 ? null : results[0].gameObject;
        }
 
        static PointerEventData ScreenPosToPointerData (Vector2 screenPos)
            => new(EventSystem.current){position = screenPos};
    }
}
