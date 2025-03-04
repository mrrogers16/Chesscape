using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEditor.Rendering;
using UnityEngine;

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
    public List<Transform> spawnPoints;
    public List<GridTile> gridTiles = new List<GridTile>();

    // 14, 10, 12, 13, 11, 15 are the tiles we dont want

    [SerializeField] private int maxWallsSpawned = 30;
    [SerializeField] private int maxTrapsSpawned = 15;
    [SerializeField] private float tileSize = 1f;

    void Start()
    {
        PopulateGridTiles();
        SpawnExitRoom();
        SpawnWalls();
        SpawnTraps();
    }

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
                Instantiate(wallPrefab, tile.tileTransform.position + new Vector3(0, 5f, 0), Quaternion.identity);
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
                Instantiate(trapPrefab, tile.tileTransform.position + new Vector3(0, 5f, 0), Quaternion.identity);
                tile.occupied = true;
                spawnedTrapCount++;
            }
        }
    }

    void PopulateGridTiles()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points set!");
            return;
        }

        // Determine grid origin (minimum x and z values)
        float minX = spawnPoints[0].position.x;
        float minZ = spawnPoints[0].position.z;

        foreach (Transform t in spawnPoints)
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
        foreach (Transform t in spawnPoints)
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
    // Helper method to find a GridTile with a specific row and column.
    GridTile FindGridTile(int row, int col)
    {
        foreach (GridTile tile in gridTiles)
        {
            if (tile.row == row && tile.column == col)
            {
                return tile;
            }
        }
        return null;
    }

    // Spawns the exit room (a 4x4 block with border walls and one door on a middle side).
    void SpawnExitRoom()
    {
        // Determine maximum row and column values.
        int maxRow = 0;
        int maxCol = 0;
        foreach (GridTile tile in gridTiles)
        {
            if (tile.row > maxRow)
            {
                maxRow = tile.row;
            }
            if (tile.column > maxCol)
            {
                maxCol = tile.column;
            }
        }

        // Find candidate bottom-left anchors for a 4x4 block.
        List<GridTile> candidateAnchors = new List<GridTile>();
        foreach (GridTile tile in gridTiles)
        {
            if (tile.row <= maxRow - 3 && tile.column <= maxCol - 3)
            {
                bool valid = true;
                // Check that every tile in the 4x4 block is present and unoccupied.
                for (int r = tile.row; r < tile.row + 4; r++)
                {
                    for (int c = tile.column; c < tile.column + 4; c++)
                    {
                        GridTile checkTile = FindGridTile(r, c);
                        if (checkTile == null || checkTile.occupied)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid)
                    {
                        break;
                    }
                }
                if (valid)
                {
                    candidateAnchors.Add(tile);
                }
            }
        }

        if (candidateAnchors.Count == 0)
        {
            Debug.LogWarning("No valid exit room location found.");
            return;
        }

        // Pick a random candidate anchor.
        GridTile anchorTile = candidateAnchors[Random.Range(0, candidateAnchors.Count)];
        int baseRow = anchorTile.row;
        int baseCol = anchorTile.column;

        // Choose the door side.
        // Allowed door positions are the middle tiles on each wall:
        // Top: (baseRow+3, baseCol+1 or baseCol+2)
        // Bottom: (baseRow, baseCol+1 or baseCol+2)
        // Left: (baseRow+1 or baseRow+2, baseCol)
        // Right: (baseRow+1 or baseRow+2, baseCol+3)
        int doorSide = Random.Range(0, 4); // 0 = top, 1 = bottom, 2 = left, 3 = right
        int doorRow = 0;
        int doorCol = 0;

        if (doorSide == 0)
        {
            doorRow = baseRow + 3;
            doorCol = baseCol + Random.Range(1, 3); // either baseCol+1 or baseCol+2
        }
        else if (doorSide == 1)
        {
            doorRow = baseRow;
            doorCol = baseCol + Random.Range(1, 3);
        }
        else if (doorSide == 2)
        {
            doorRow = baseRow + Random.Range(1, 3);
            doorCol = baseCol;
        }
        else if (doorSide == 3)
        {
            doorRow = baseRow + Random.Range(1, 3);
            doorCol = baseCol + 3;
        }

        // Determine the exit ladder's location.
        // The exit should be in one of the free interior 2x2 tiles, on the side opposite the door.
        int exitRow = 0;
        int exitCol = 0;
        if (doorSide == 0)
        {
            // Door on top, so exit is on the bottom interior row.
            exitRow = baseRow + 1;
            exitCol = baseCol + Random.Range(1, 3);
        }
        else if (doorSide == 1)
        {
            // Door on bottom, so exit is on the top interior row.
            exitRow = baseRow + 2;
            exitCol = baseCol + Random.Range(1, 3);
        }
        else if (doorSide == 2)
        {
            // Door on left, so exit is on the right interior column.
            exitCol = baseCol + 2;
            exitRow = baseRow + Random.Range(1, 3);
        }
        else if (doorSide == 3)
        {
            // Door on right, so exit is on the left interior column.
            exitCol = baseCol + 1;
            exitRow = baseRow + Random.Range(1, 3);
        }

        // Build the room.
        // Loop through the 4x4 block.
        for (int r = baseRow; r < baseRow + 4; r++)
        {
            for (int c = baseCol; c < baseCol + 4; c++)
            {
                GridTile currentTile = FindGridTile(r, c);
                if (currentTile == null)
                {
                    continue;
                }

                // Mark every tile in this exit room as occupied.
                currentTile.occupied = true;

                // Border tiles get walls except for the door.
                if (r == baseRow || r == baseRow + 3 || c == baseCol || c == baseCol + 3)
                {
                    if (r == doorRow && c == doorCol)
                    {
                        // Leave a gap for the door.
                        continue;
                    }
                    else
                    {
                        Instantiate(wallPrefab, currentTile.tileTransform.position + new Vector3(0, 5f, 0), Quaternion.identity);
                    }
                }
            }
        }

        // Spawn the exit ladder on the chosen interior tile.
        GridTile exitTile = FindGridTile(exitRow, exitCol);
        if (exitTile != null)
        {
            Instantiate(exitPrefab, exitTile.tileTransform.position + new Vector3(0, 5f, 0), Quaternion.identity);
            // exitTile.occupied is already true from above.
        }
        else
        {
            Debug.LogWarning("Exit tile not found.");
        }
    }
}


