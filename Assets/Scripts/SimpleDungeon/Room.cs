using UnityEngine;

public class Room
{
    public Vector2Int center;
    public int size;
    public RoomType type;

    public Room(Vector2Int center, int size, RoomType type)
    {
        this.center = center;
        this.size = size;
        this.type = type;
    }

    public Color GetColor()
    {
        switch (type)
        {
            case RoomType.Start:
                return Color.green;
            case RoomType.Treasure:
                return Color.yellow;
            case RoomType.Boss:
                return Color.red;
            default:
                return Color.white;
        }
    }
}
