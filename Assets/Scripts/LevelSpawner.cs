using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridTile
{
    public Transform tileTransform;
    public int row;
    public int column;
    public bool occupied = false;
}
public class LevelSpawner : MonoBehaviour
{
    [Header("GameObject Prefabs")]
    public GameObject wallPrefab;
    public GameObject trapPrefab;
    public GameObject exitPrefab;
    public GameObject keyPrefab;
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public List<Transform> wallSpawnPoints;
    public List<Transform> keySpawnPoints;
    public Transform playerSpawnPoint;

    public List<GridTile> gridTiles = new List<GridTile>();

    // 14, 10, 12, 13, 11, 15 are the tiles we dont want

    [Header("Item info")]
    [SerializeField] private int maxWallsSpawned = 5;
    [SerializeField] private int maxTrapsSpawned = 15;
    [SerializeField] private int maxKeysSpawned = 1;
    [SerializeField] private float tileSize = 1f;

    void Start()
    {
        PopulateGridTiles();
        SpawnKey();
        SpawnWalls();
        SpawnTraps();


    }

    void SpawnKey()
    {
        List<GridTile> availableTiles = new List<GridTile>();

        foreach (GridTile tile in gridTiles)
        {
            if (!tile.occupied)
            {
                availableTiles.Add(tile);
            }
        }

        if (availableTiles.Count == 0)
        {
            Debug.Log("No free tiles");
            return;
        }

        int randomIndex = Random.Range(0, availableTiles.Count);
        GridTile chosenTile = availableTiles[randomIndex];

        Instantiate(keyPrefab, chosenTile.tileTransform.position + new Vector3(0f, 2.5f, 0f), Quaternion.identity);

        chosenTile.occupied = true;

    }
    // void SpawnKey()
    // {
    //     if (keySpawnPoints == null || keySpawnPoints.Count == 0)
    //     {
    //         Debug.Log("No key spawn points provided");
    //         return;
    //     }

    //     int randomIndex = Random.Range(0, keySpawnPoints.Count);
    //     Transform keyPoint = keySpawnPoints[randomIndex];

    //     Instantiate(keyPrefab, keyPoint.position + new Vector3(0, 2.5f, 0), Quaternion.identity);

    // }
    void SpawnWalls()
    {
        int spawnedWallCount = 0;
        int attempts = 0;
        int maxAttempts = maxWallsSpawned * 10;

        while (spawnedWallCount < maxWallsSpawned && attempts < maxAttempts)
        {
            attempts++;
            int randomIndex = Random.Range(0, gridTiles.Count);
            GridTile tile = gridTiles[randomIndex];

            // Only spawn if the tile is unoccupied
            if (!tile.occupied)
            {
                Instantiate(wallPrefab, tile.tileTransform.position + new Vector3(0, 3f, 0), Quaternion.identity);
                tile.occupied = true;
                spawnedWallCount++;
            }
        }
    }

    void SpawnTraps()
    {
        int spawnedTrapCount = 0;
        int attempts = 0;
        int maxAttempts = maxTrapsSpawned * 10;

        while (spawnedTrapCount < maxTrapsSpawned && attempts < maxAttempts)
        {
            attempts++;
            int randomIndex = Random.Range(0, gridTiles.Count);
            GridTile tile = gridTiles[randomIndex];

            // Spawn a trap only if nothing occupies the tile.
            if (!tile.occupied)
            {
                Instantiate(trapPrefab, tile.tileTransform.position + new Vector3(0, 4f, 0), Quaternion.identity);
                tile.occupied = true;
                spawnedTrapCount++;
            }
        }
    }

    void PopulateGridTiles()
    {
        if (wallSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points set!");
            return;
        }

        // Determine grid origin (minimum x and z values)
        float minX = wallSpawnPoints[0].position.x;
        float minZ = wallSpawnPoints[0].position.z;

        foreach (Transform t in wallSpawnPoints)
        {
            if (t.position.x < minX)
            {
                minX = t.position.x;
            }
            if (t.position.z < minZ)
            {
                minZ = t.position.z;
            }
        }

        // Create a GridTile for each spawn point
        // Chatgpt helped with this one
        foreach (Transform t in wallSpawnPoints)
        {
            int col = Mathf.RoundToInt((t.position.x - minX) / tileSize);
            int row = Mathf.RoundToInt((t.position.z - minZ) / tileSize);

            GridTile gridTile = new GridTile();
            gridTile.tileTransform = t;
            gridTile.column = col;
            gridTile.row = row;
            gridTile.occupied = false;

            gridTiles.Add(gridTile);
        }
    }

}


