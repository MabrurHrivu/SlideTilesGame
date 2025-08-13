using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    RefList Refs;
    int uniqID = 1;
    void Start()
    {
        RefList.Initialize();
        Refs = RefList.Instance;
        prepareTileList(Refs.spawnedTile);
        List<Vector2Int> coords = GetShuffledCoords(Refs.rows, Refs.columns);
        foreach (var coord in coords)
        {
            if (uniqID >= Refs.spawnedTile.Length) break;
            createTileinCoord(uniqID, coord);
            uniqID++;
        }
    }

    // Generates all (i,j) coordinates and shuffles them randomly.
    List<Vector2Int> GetShuffledCoords(int rows, int cols)
    {
        var coords = new List<Vector2Int>(rows * cols);

        for (int j = 0; j < rows; j++)
            for (int i = 0; i < cols; i++)
                coords.Add(new Vector2Int(i, j));

        // Fisher–Yates shuffle
        for (int k = coords.Count - 1; k > 0; k--)
        {
            int r = Random.Range(0, k + 1);
            (coords[k], coords[r]) = (coords[r], coords[k]);
        }

        return coords;
    }

    void prepareTileList(GameObject[] tileList)
    {
        int currentIndex = 1;
        while (currentIndex < Refs.tileCount)
        {
            GameObject selectedPrefab = randomTile();
            tileList[currentIndex++] = selectedPrefab;
            tileList[currentIndex++] = selectedPrefab;
        }
        ShuffleArray(tileList);
    }

    GameObject randomTile()
    {
        if (Refs != null && Refs.tileType.Length > 0)
        {
            int randomIndex = Random.Range(0, Refs.tileType.Length);
            return Refs.tileType[randomIndex];
        }
        else
        {
            Debug.LogError("RefList is not initialized or has no prefabs!");
            return null;
        }
    }

    void ShuffleArray(GameObject[] array)
    {
        for (int i = array.Length - 1; i > 1; i--)
        {
            int randomIndex = Random.Range(1, i + 1);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }
    void createTileinCoord(int uniqID,Vector2Int coord)
    {
        if (Refs.spawnedTile[uniqID] != null)
            {
                Refs.spawnedTile[uniqID] = Instantiate(Refs.spawnedTile[uniqID], Refs.gridd.GetCellCenterLocal(new Vector3Int(coord.x, coord.y, 1)), Quaternion.identity, Refs.canv.transform);
                Refs.spawnedTile[uniqID].GetComponent<Tile>().init(uniqID);
                Refs.positionTable[coord.x, coord.y] = uniqID;
            }
    }
}
