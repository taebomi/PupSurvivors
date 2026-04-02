using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Stage;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Dictionary<Vector2Int, Map> MapDict { get; private set; }
    
    private LinkedList<Map> _currentMapList;
    private bool _indoorNow;

    private void Awake()
    {
        MapDict = new Dictionary<Vector2Int, Map>(transform.childCount);
        
        foreach (Transform childTr in transform)
        {
            var pos = childTr.position / Map.Length;
            var coordinate = Vector2Int.FloorToInt(pos);
            MapDict.Add(coordinate, childTr.GetComponent<Map>());
        }
    }

    public void ActivateMap(Map map)
    {
        if (_indoorNow == map.Indoor)
        {
            map.SetActive(true);
        }

        _currentMapList.AddFirst(map);
    }

    public void DeactivateMap(Map map)
    {
        map.SetActive(false);
        _currentMapList.Remove(map);
    }

    public void EnterRoom(Map enteredMap)
    {
        _indoorNow = enteredMap.Indoor;
        foreach (var map in _currentMapList)
        {
            if (map.Indoor == enteredMap.Indoor)
            {
                map.Enter().Forget();
            }
            else
            {
                map.Exit().Forget();
            }
        }
    }
}
