using UnityEngine;
using UnityEngine.UI;

namespace WarOfWords
{
    public class PanButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _arrowImage;
        [SerializeField] private Color _disabledColor;
        

        public void SetEnabled(bool enabled)
        {
            _button.interactable = enabled;
            _arrowImage.color = enabled ? Color.white : _disabledColor;
        }
    }
}
