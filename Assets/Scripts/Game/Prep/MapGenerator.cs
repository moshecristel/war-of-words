using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using WarOfWords;
using Random = System.Random;

[RequireComponent(typeof(MapShuffler))]
public class MapGenerator : MonoBehaviour
{

    [PropertyOrder(1)]
    [TitleGroup("Create")]
    [Button(ButtonSizes.Medium, Name="Create Map - Easier")]
    public void CreateMapEasier()
    {
        CreateMap(true);
    }
    
    [PropertyOrder(1)]
    [TitleGroup("Create")]
    [Button(ButtonSizes.Medium, Name="Create Map - Harder")]
    public void CreateMapHarder()
    {
        CreateMap(false);
    }

    [PropertyOrder(2)] [TitleGroup("Shuffle")]
    public bool isEasier;
    
    [PropertyOrder(2)]
    [TitleGroup("Shuffle")]
    [LabelText("Shuffle Batch Size")]
    public int _shuffleBatchSize = 50;
    
    [PropertyOrder(2)]
    [TitleGroup("Shuffle")]
    [GUIColor(0, 1, 0)]
    [Button(ButtonSizes.Medium, Name="Shuffle")]
    public void Shuffle()
    {
        Shuffle(isEasier);
    }
    
    [PropertyOrder(2)]
    [TitleGroup("Shuffle")]
    [GUIColor(1, 0, 0)]
    [Button(ButtonSizes.Medium, Name="Stop Shuffle")]
    public void StopShuffle()
    {
        Stop();
    }
    
    private Map _map;
    private MapShuffler _mapShuffler;
    
    private void CreateMap(bool isEasier)
    {
        _map = MapReader.LoadNewMapFromShape(State.Washington, isEasier);
        _mapShuffler = GetComponent<MapShuffler>();
        _map.PrintStats();
    }

    private void Shuffle(bool isEasier)
    {
        if(_mapShuffler == null) Debug.Log("shuffler is null");
        if(_map == null) Debug.Log("map is null");
        _mapShuffler.StartShuffle(_map, isEasier);
    }

    private void Stop()
    {
        _mapShuffler.StopShuffle();
    }
}
