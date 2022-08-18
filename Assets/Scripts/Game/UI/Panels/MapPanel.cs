using TMPro;
using UnityEngine;

namespace WarOfWords
{
    /// <summary>
    /// Attached to UI for GameView.Map and handles associated UI events (screen touch events are handled by "Game")
    /// </summary>
    public class MapPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        
        public MapBoard MapBoard { get; set; }

        public void SetTitle(string title)
        {
            _titleText.text = title;
        }
    }
}
