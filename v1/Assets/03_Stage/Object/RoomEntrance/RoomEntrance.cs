using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Stage.Map;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomEntrance : MonoBehaviour
{
    [SerializeField] private string mapName;

    public void MapName(string mapName)
    {
        this.mapName = mapName;
    }

    public void XSize(float x)
    {
        var bc = GetComponent<BoxCollider2D>();
        bc.size = new Vector2(x, bc.size.y);
        transform.position = new Vector3(transform.position.x + x * 0.5f, transform.position.y, 0f);
    }

    public void YSize(float y)
    {
        var bc = GetComponent<BoxCollider2D>();
        bc.size = new Vector2(bc.size.x, y);
        transform.position = new Vector3(transform.position.x, transform.position.y - y*0.5f, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MapManager.Instance.EnterRoom(MapManager.Instance.MapDict[mapName]);
    }
}
