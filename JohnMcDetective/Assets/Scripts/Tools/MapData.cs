using UnityEngine;

[CreateAssetMenu(fileName = "NewMapData", menuName = "Map/Map Data")]
public class MapData : ScriptableObject
{
    public int gridWidth = 5;
    public int gridHeight = 5;
    public Vector2Int startPosition = Vector2Int.zero;

    [SerializeField] private RoomData[] rooms = new RoomData[25];

    public RoomData GetRoom(int x, int y)
    {
        if (!IsValidIndex(x, y)) return default;
        return rooms[y * gridWidth + x];
    }

    public void SetRoom(int x, int y, RoomData room)
    {
        if (!IsValidIndex(x, y)) return;
        
        if (rooms == null || rooms.Length != gridWidth * gridHeight)
        {
            ResizeGrid(gridWidth, gridHeight);
        }

        rooms[y * gridWidth + x] = room;
    }

    public void ResizeGrid(int newWidth, int newHeight)
    {
        RoomData[] newRooms = new RoomData[newWidth * newHeight];

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                if (x < gridWidth && y < gridHeight && rooms != null)
                {
                    newRooms[y * newWidth + x] = GetRoom(x, y);
                }
            }
        }

        gridWidth = newWidth;
        gridHeight = newHeight;
        rooms = newRooms;
    }

    private bool IsValidIndex(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }
}