using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Spawn,
    Exit,
    Confirmed,
    Empty
}


public class DungeonRoom : MonoBehaviour
{
    public RoomType roomType;
    public List<DungeonConnector> connectors;
    public BoxCollider boundsCollider;

    public Bounds GetWorldBounds()
    {
        return boundsCollider.bounds;
    }
}
