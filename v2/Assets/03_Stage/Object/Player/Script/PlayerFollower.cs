using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class PlayerFollower : MonoBehaviour
    {
        private List<Transform> _followerTrList;

        private const float SqrDist = 1.5f;
        private Player _player;

        private void Awake()
        {
            _followerTrList = new List<Transform>(5);
            _player = GetComponent<Player>();
            _followerTrList.Add(transform);
        }

        private void Update()
        {
            for (var i = 1; i < _followerTrList.Count; i++)
            {
                var prevTr = _followerTrList[i - 1];
                var curTr = _followerTrList[i];
                if ((curTr.position - prevTr.position).sqrMagnitude < SqrDist)
                {
                    continue;
                }
                curTr.position =
                    Vector3.Lerp(curTr.position, prevTr.position, Time.deltaTime * 2f);
            }
        }

        public void AddFollower(Transform followerTr, bool instantPosition = true)
        {
            if (instantPosition)
            {
                followerTr.position = _followerTrList[^1].position - (Vector3)_player.LastInputDir;
            }
            _followerTrList.Add(followerTr);
        }

        public void RemoveFollower(Transform followerTr)
        {
            _followerTrList.Remove(followerTr);
        }
    
    
    }
}