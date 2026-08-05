using System;
using UnityEngine;
using UnityEngine.InputSystem; // Required for the New Input System

public class SCR_GridManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float tileSize = 1.1f;

    [Header("Movement Rules")]
    public int maxMovementDistance = 3;

    private Transform selectedPlayer = null;
    private Vector2Int selectedPlayerGridPos;
    private GameObject[,] gridTiles;

    void Start()
    {
        GenerateGrid();
        SpawnPlayers();
    }

    void Update()
    {
        HandleInput();
    }

    void GenerateGrid()
    {
        gridTiles = new GameObject[gridWidth, gridHeight];

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Tile_{i}_{j}";
                tile.transform.position = new Vector3(i * tileSize, 0, j * tileSize);
                tile.transform.localScale = new Vector3(1f, 0.1f, 1f);
                tile.transform.parent = this.transform;

                TileData data = tile.AddComponent<TileData>();
                data.gridPosition = new Vector2Int(i, j);

                gridTiles[i, j] = tile;
            }
        }
    }

    void SpawnPlayers()
    {
        SpawnPlayer("Player 1", new Vector2Int(0, 0), Color.red);
        SpawnPlayer("Player 2", new Vector2Int(7, 7), Color.blue);
    }

    void SpawnPlayer(string name, Vector2Int gridPos, Color color)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = name;
        player.tag = "Player";
        player.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        player.transform.position = GetWorldPosition(gridPos) + new Vector3(0, 0.4f, 0);

        Renderer renderer = player.GetComponent<Renderer>();
        renderer.material.color = color;

        PlayerData data = player.AddComponent<PlayerData>();
        data.gridPosition = gridPos;
    }

    void HandleInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PlayerData clickedPlayer = hit.collider.GetComponent<PlayerData>();
                if (clickedPlayer != null)
                {
                    SelectPlayer(clickedPlayer);
                    return;
                }

                TileData clickedTile = hit.collider.GetComponent<TileData>();
                if (clickedTile != null && selectedPlayer != null)
                {
                    TryMovePlayer(clickedTile.gridPosition);
                }
            }
        }
    }

    void SelectPlayer(PlayerData player)
    {
        selectedPlayer = player.transform;
        selectedPlayerGridPos = player.gridPosition;
        Debug.Log($"[Selected]: {player.gameObject.name} at Grid {selectedPlayerGridPos}");
    }

    void TryMovePlayer(Vector2Int targetGridPos)
    {
        int distance = CalculateManhattanDistance(selectedPlayerGridPos, targetGridPos);

        if (distance <= maxMovementDistance && distance > 0)
        {
            PlayerData data = selectedPlayer.GetComponent<PlayerData>();
            data.gridPosition = targetGridPos;
            selectedPlayerGridPos = targetGridPos;

            selectedPlayer.position = GetWorldPosition(targetGridPos) + new Vector3(0, 0.4f, 0);
            Debug.Log($"[Moved]: {selectedPlayer.name} to {targetGridPos} (Distance: {distance})");

            selectedPlayer = null;
        }
        else
        {
            Debug.LogWarning($"[Invalid Move]: Distance {distance} exceeds max limit of {maxMovementDistance} tiles.");
        }
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tileSize, 0, gridPos.y * tileSize);
    }

    public static int CalculateManhattanDistance(Vector2Int start, Vector2Int end)
    {
        return Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
    }
} 


public class TileData : MonoBehaviour
{
    public Vector2Int gridPosition;
}

public class PlayerData : MonoBehaviour
{
    public Vector2Int gridPosition;
}