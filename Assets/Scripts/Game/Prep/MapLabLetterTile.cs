using TMPro;
using UnityEngine;

namespace WarOfWords
{
    public class MapLabLetterTile : MonoBehaviour
    {
        public MapLetter MapLetter { get; set; }
        
        [SerializeField] private TMP_Text _letterText;

        public void UpdateVisuals()
        {
            if(MapLetter != null)
                _letterText.text = MapLetter.Character;
        }
    }
}
