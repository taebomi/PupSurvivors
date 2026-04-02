using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace PupSurvivors.Stage.Map
{
    public class MapManager : Singleton<MapManager>
    {
        [field: SerializeField] public SerializedDictionary<string, Map> MapDict { get; private set; } = new();

        [SerializeField] private bool isIndoor;

        private LinkedList<Map> _currentMapList;

        protected override void AwakeAfter()
        {
            _currentMapList = new LinkedList<Map>();
        }

        public void ActivateMap(Map map)
        {
            if (isIndoor == map.isIndoor)
            {
                map.Activate(true);
            }

            _currentMapList.AddFirst(map);
        }

        public void DeactiveMap(Map map)
        {
            map.Activate(false);
            _currentMapList.Remove(map);
        }


        public void EnterRoom(Map enteredMap)
        {
            isIndoor = enteredMap.isIndoor;
            foreach (var map in _currentMapList)
            {
                if (map.isIndoor == enteredMap.isIndoor)
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
}