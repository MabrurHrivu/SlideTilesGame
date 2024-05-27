using System.Collections;
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

        for (int j = 0; j < 4; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                if (uniqID == Refs.spawnedTile.Length)
                {
                    break;
                }
                Refs.spawnedTile[uniqID] = Instantiate(Refs.spawnedTile[uniqID], Refs.gridd.GetCellCenterLocal(new Vector3Int(i, j, 1)), Quaternion.identity, Refs.canv.transform);
                Refs.spawnedTile[uniqID].GetComponent<Tile>().init(uniqID);
                Refs.positionTable[i, j] = uniqID;
                print(Refs.positionTable[i, j]);
                uniqID++;
            }
        }
        
    }

    void prepareTileList(GameObject[] tileList)
    {
        //private GameObject[] prefabArray = new GameObject[20];
        int currentIndex = 1;
        while (currentIndex < tileList.Length)
        {
            GameObject selectedPrefab = randomTile();
            // Insert the prefab twice into the array
            tileList[currentIndex++] = selectedPrefab;
            tileList[currentIndex++] = selectedPrefab;
        }
        ShuffleArray(tileList);
    }

    //returns a random prefab from the global prefab list
    GameObject randomTile()
    {
        // Ensure RefList is initialized and has prefabs
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
            GameObject temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
