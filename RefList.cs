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
    public int tileCount, columns = 24, rows = 8;
    public GameObject[] spawnedTile;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            spawnedTile = new GameObject[rows * columns];

            positionTable = new int[columns, rows];
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

    private bool smoothMode = true;
    public void EnableSmooth()
    {
        smoothMode = true;
    }

    // Disable smooth movement
    public void DisableSmooth()
    {
        smoothMode = false;
    }

    // Check if smooth movement is enabled
    public bool IsSmoothMode()
    {
        return smoothMode;
    }
    
}
