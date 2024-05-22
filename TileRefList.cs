using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileRefList : MonoBehaviour
{
    // Singleton instance
    public static TileRefList Instance { get; private set; }

    // Array to store references to prefabs
    public GameObject[] tileType;

    private void Awake()
    {
        // Check if there is already an instance of TileReflist
        if (Instance == null)
        {
            // If not, set it to this instance
            Instance = this;
            // Optionally, make the singleton persistent across scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If there is already an instance, destroy this one
            Destroy(gameObject);
        }
    }

    // Example of how you might instantiate a prefab from the array
}
