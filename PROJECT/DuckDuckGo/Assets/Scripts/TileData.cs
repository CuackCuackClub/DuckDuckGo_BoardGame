using UnityEngine;

public class TileData : MonoBehaviour
{
    [Header("Grid Position")]
    public Vector2Int gridPosition;

    [Header("Tile State")]
    public bool isOccupied = false;
}