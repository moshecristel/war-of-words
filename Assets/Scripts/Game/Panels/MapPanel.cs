using TMPro;
using UnityEngine;

namespace WarOfWords
{
    public class MapPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;

        public void SetTitle(string title)
        {
            _titleText.text = title;
        }
    }
}
