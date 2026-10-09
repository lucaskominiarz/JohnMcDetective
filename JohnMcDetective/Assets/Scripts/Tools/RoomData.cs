using System;
using UnityEngine;

public enum RoomType
{
    Empty,
    Corridor,
    CrimeScene,
    Bedroom
}

[Serializable]
public struct ObjectItem
{
    public GameObject prefab;
    public int layer;
    public float layerSpacing;
    public Vector2 position;
}

[Serializable]
public struct RoomData
{
    public RoomType type;
    public string roomName;
    public GameObject backGround;
    public GameObject canvas;
    public ObjectItem[] roomObjects;
    public bool isLocked;
    public bool inSpiritWorld;

    public bool IsEmpty => type == RoomType.Empty;
}