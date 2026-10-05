using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private MapData mapData;
    [SerializeField] private Vector2 cameraMoveValue = new Vector2(4.5f, 10f);

    public MapData GetMapData(){return mapData; }
    
    private void Awake()
    {
        GameObject roomsParent = Instantiate(new GameObject("RoomsParent"));
        for (int i = 0; i < mapData.gridWidth; i++)
        {
            for (int j = 0; j < mapData.gridHeight; j++)
            {
                RoomData actualRoom = mapData.GetRoom(i, j);
                if (actualRoom.type != RoomType.Empty)
                {
                    GameObject room = Instantiate(new GameObject("Room"), roomsParent.transform);
                    
                    GameObject newBg = Instantiate(actualRoom.backGround, room.transform);
                    newBg.transform.position =
                        new Vector2(cameraMoveValue.x * i , cameraMoveValue.y * j);
                    
                    SpawnObjects(actualRoom, room.transform,new Vector2(i,j));
                    
                }
            }
        }
    }

    void SpawnObjects(RoomData room, Transform parent, Vector2 tabPosition)
    {
        foreach (ObjectItem obj in room.roomObjects)
        {
            GameObject newObj = Instantiate(obj.prefab, parent);
            newObj.transform.position =
                new Vector2(cameraMoveValue.x * tabPosition.x + Mathf.Clamp(obj.position.x, -cameraMoveValue.x / 2f,cameraMoveValue.x / 2f ), 
                    cameraMoveValue.y * tabPosition.y + Mathf.Clamp(obj.position.y, -cameraMoveValue.y / 2f,cameraMoveValue.y / 2f ));
            newObj.GetComponent<SpriteRenderer>().sortingOrder = obj.layer;
        }
    }
}
