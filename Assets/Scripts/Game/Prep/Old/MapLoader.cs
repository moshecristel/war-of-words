using System;
using UnityEngine;

namespace WarOfWords
{
    public class MapLoader : MonoBehaviour
    {
        private void Awake()
        {
            Map map = MapBakedReader.LoadNewMapFromData(State.Washington);
            map.Print();
            map.PrintStats();
        }
    }
}
