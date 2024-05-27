using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefList : MonoBehaviour
{
    // Singleton instance
    private static RefList _instance;
    public static RefList Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("RefList is not initialized. Make sure it is added to a GameObject in the scene.");
            }
            return _instance;
        }
    }

    // Array to store references to prefabs
    public GameObject[] tileType;
    public Grid gridd;
    public int[,] positionTable;
    public Canvas canv;
    public int tileCount;
    public GameObject[] spawnedTile; 

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            spawnedTile = new GameObject[tileCount+1]; 
            positionTable = new int[30,10];
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        
    }
    public static void Initialize()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<RefList>();
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(RefList).Name);
                _instance = singletonObject.AddComponent<RefList>();
                DontDestroyOnLoad(singletonObject);
            }
        }
    }

    // Example of how you might instantiate a prefab from the array
}
