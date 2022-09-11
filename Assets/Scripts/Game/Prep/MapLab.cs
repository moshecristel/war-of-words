using UnityEngine;

namespace WarOfWords
{
    public class MapLab : MonoBehaviour
    {
        [SerializeReference] private MapLabBoard _mapLabBoard;

        private void Awake()
        {
            CreateMap();
        }

        private void CreateMap()
        {
            _mapLabBoard.Map = MapShapesReader.LoadNewMapFromShape(State.Washington);
        }        
    }
}
