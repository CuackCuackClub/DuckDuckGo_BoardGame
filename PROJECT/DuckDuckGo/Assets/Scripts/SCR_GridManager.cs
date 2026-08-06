using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_GridManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float tileSize = 1.1f;
    public GameObject tileObject;
    public Vector3 unitOffset = new Vector3(0, 0.5f, 0); // Ajuste vertical para prefabs

    [System.Serializable]
    public struct EntitySpawnData
    {
        public string entityName;
        public GameObject prefab;
        public Vector2Int spawnPosition;
        public EntitiesData entityType;
        public int hp;
        public int attack;
        public int defense;
        public int moveDistance;
    }

    [Header("Entities to Spawn")]
    public List<EntitySpawnData> initialEntities = new List<EntitySpawnData>();

    private EntityData selectedEntity = null;
    private GameObject[,] gridTiles;

    void Start()
    {
        GenerateGrid();
        SpawnInitialEntities();
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
                GameObject tile = Instantiate(tileObject, new Vector3(i * tileSize, 0, j * tileSize), Quaternion.identity, this.transform);
                tile.name = $"Tile_{i}_{j}";

                TileData data = tile.AddComponent<TileData>();
                data.gridPosition = new Vector2Int(i, j);

                gridTiles[i, j] = tile;
            }
        }
    }

    void SpawnInitialEntities()
    {
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
        foreach (var entityInfo in initialEntities)
        {
            if (entityInfo.prefab != null)
            {
                SpawnEntity(entityInfo);
                occupiedPositions.Add(entityInfo.spawnPosition);
            }
            else
            {
                Debug.LogWarning($"[GridManager]: El prefab para {entityInfo.entityName} no está asignado en el Inspector.");
            }
        }
    }

    public EntityData SpawnEntity(EntitySpawnData spawnData)
    {
        if (IsTileOccupied(spawnData.spawnPosition))
        {
            Debug.LogWarning($"[Spawn Warning]: Intentando spawnear en casilla ocupada {spawnData.spawnPosition}.");
        }

        Vector3 spawnWorldPos = GetWorldPosition(spawnData.spawnPosition) + unitOffset;
        GameObject unitObj = Instantiate(spawnData.prefab, spawnWorldPos, Quaternion.identity);
        unitObj.name = spawnData.entityName;

        EntityData data = unitObj.GetComponent<EntityData>();
        if (data == null)
        {
            data = unitObj.AddComponent<EntityData>();
        }

        data.entityName = spawnData.entityName;
        data.gridPosition = spawnData.spawnPosition;
        data.maxHealth = spawnData.hp;
        data.currentHealth = spawnData.hp;
        data.attackPower = spawnData.attack;
        data.defensePower = spawnData.defense;
        data.moveDistance = spawnData.moveDistance;
        data.SetEntityType(spawnData.entityType);

        return data;
    }

    void HandleInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                EntityData clickedEntity = hit.collider.GetComponentInParent<EntityData>();
                if (clickedEntity != null)
                {
                    if (clickedEntity.entities == EntitiesData.Player)
                    {
                        SelectEntity(clickedEntity);
                    }
                    return;
                }

                TileData clickedTile = hit.collider.GetComponent<TileData>();
                if (clickedTile != null && selectedEntity != null)
                {
                    TryMoveEntity(clickedTile.gridPosition);
                }
            }
            else
            {
                DeselectEntity();
            }
        }
    }

    void SelectEntity(EntityData entity)
    {
        selectedEntity = entity;
        Debug.Log($"[Selected]: {entity.entityName} en {entity.gridPosition} (Límite de movimiento: {entity.moveDistance})");
    }

    void DeselectEntity()
    {
        if (selectedEntity != null)
        {
            Debug.Log($"[Deselected]: {selectedEntity.entityName}");
            selectedEntity = null;
        }
    }

    void TryMoveEntity(Vector2Int targetGridPos)
    {
        // Verificar si la casilla destino ya tiene otra unidad
        if (IsTileOccupied(targetGridPos))
        {
            Debug.LogWarning($"[Invalid Move]: La casilla {targetGridPos} está ocupada.");
            return;
        }

        int distance = CalculateManhattanDistance(selectedEntity.gridPosition, targetGridPos);

        if (distance <= selectedEntity.moveDistance && distance > 0)
        {
            selectedEntity.gridPosition = targetGridPos;
            selectedEntity.transform.position = GetWorldPosition(targetGridPos) + unitOffset;

            Debug.Log($"[Moved]: {selectedEntity.entityName} a {targetGridPos} (Distancia: {distance})");

            DeselectEntity();
        }
        else
        {
            Debug.LogWarning($"[Invalid Move]: La distancia {distance} excede el límite de {selectedEntity.moveDistance} de {selectedEntity.entityName}.");
        }
    }

    public bool IsTileOccupied(Vector2Int gridPos)
    {
        EntityData[] allEntities = FindObjectsByType<EntityData>(FindObjectsSortMode.None);
        foreach (var entity in allEntities)
        {
            if (entity.gridPosition == gridPos)
            {
                return true;
            }
        }
        return false;
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