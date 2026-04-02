using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    private List<Transform> _followers;

    private const float SqrDist = 1.5f;
    private PlayerController _player;

    private void Awake()
    {
        _followers = new List<Transform>(5);
        _player = GetComponent<PlayerController>();
        _followers.Add(transform);
    }

    public void AddFollower(Transform tr)
    {
        // TODO player 대신 movable 인터페이스로 변경
        tr.position = _followers[^1].position - (Vector3)_player.LastInputDir;
        _followers.Add(tr);
    }

    public void RemoveFollower(Transform tr)
    {
        _followers.Remove(tr);
    }

    private void Update()
    {
        for (var i = 1; i < _followers.Count; i++)
        {
            if ((_followers[i].position - _followers[i-1].position).sqrMagnitude < SqrDist)
            {
                continue;
            }
            _followers[i].position =
                Vector3.Lerp(_followers[i].position, _followers[i - 1].position, Time.deltaTime * 2f);
        }
    }
}
