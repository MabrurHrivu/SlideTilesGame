using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class BlockSpawner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject tilePrefab;
    
    public Canvas canv;
    public Grid gridd;
    public int tileCount;
    bool dragging = false, draggingleft = false, draggingUp = false;
    int uniqID = 1, startingX = 0, startingY = 0, dir=0;
    bool jammed =false;
    int[,] positionTable = new int[30,10];
    int[,] preClickTable = new int[30,10];
    Vector3Int mousePos, mousePos2, lastPos;
    GameObject[] spawnedTile;
    GameObject activeTile;
    // Start is called before the first frame update
    void Start()
    {
        spawnedTile = new GameObject[tileCount+1];
        prepareTileList(spawnedTile);

        for (int j = 0; j < 4; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                if (uniqID == spawnedTile.Length)
                {
                    break;
                }
                spawnedTile[uniqID] = Instantiate(spawnedTile[uniqID], gridd.GetCellCenterLocal(new Vector3Int(i, j, 1)), Quaternion.identity, canv.transform);
                spawnedTile[uniqID].GetComponent<Tile>().init(uniqID);
                positionTable[i, j] = uniqID;
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
        // Ensure TileRefList is initialized and has prefabs
        if (TileRefList.Instance != null && TileRefList.Instance.tileType.Length > 0)
        {
            int randomIndex = Random.Range(0, TileRefList.Instance.tileType.Length);
            return TileRefList.Instance.tileType[randomIndex];
        }
        else
        {
            Debug.LogError("TileRefList is not initialized or has no prefabs!");
            return null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
                if (dragging != true)
        {
            dragging = true;
            preClickTable = (int[,])positionTable.Clone();
        }
        mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
        mousePos.z = 15;
        //convert mouse co-ords to grid co-ords and back, to get gridd co-ords
        mousePos2 = gridd.LocalToCell(mousePos);
        
        mousePos = Vector3Int.FloorToInt(gridd.GetCellCenterLocal(mousePos2));

        activeTile = spawnedTile[positionTable[mousePos2.x, mousePos2.y]];
        lastPos  = Vector3Int.FloorToInt(new Vector3Int(activeTile.GetComponent<Tile>().posX, activeTile.GetComponent<Tile>().posY, 15));
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
        mousePos.z = 15;
        mousePos2 = gridd.LocalToCell(mousePos);
        mousePos2.z = 15;
        mousePos = Vector3Int.FloorToInt(gridd.GetCellCenterLocal(mousePos2));
        //transform.position = mousePos;
        if (lastPos != mousePos2)
        {
            //print("changed X:" + transform.position.x + " Y:" + transform.position.y + " Z:" + transform.position.z);
            dir =checkDir(lastPos.x, mousePos2.x, lastPos.y, mousePos2.y);
            checkMove(activeTile.GetComponent<Tile>().posX, activeTile.GetComponent<Tile>().posY,dir);
            lastPos = mousePos2;
        }
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        jammed = false;
        dragging = false;
        draggingUp = false;
        draggingleft = false;
        cancelMove();
    }
    void cancelMove()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                
                if (preClickTable[i, j] != 0)
                {
                    spawnedTile[preClickTable[i, j]].GetComponent<Tile>().move(i, j);
                }

            }

        }
        positionTable = (int[,])preClickTable.Clone();
    }

     int checkDir(int oldx, int newx, int oldy, int newy)
    {
        if (oldx < newx)
        {
            return 2;
        }
        if (oldx > newx)
        {
            return 4;
        }
        if (oldy < newy)
        {
            return 1;
        }
        if (oldy > newy)
        {
            return 3;
        }
        return 0;
    }


    public void writeInitXY(int x, int y)
    {
        startingX = x;
        startingY = y;
        print (startingX + ", " + startingY);
    }
    public void checkMove(int posx, int posy,int direction)
    {
        int horizMovement = -((direction - 3) * ((direction+1) % 2)); // gives -1 for left and 1 for right
        int vertMovement = -((direction - 2) * (direction%2)); // gives 1 for up and -1 for down
        if(!draggingleft && !draggingUp)
        {
            if(horizMovement != 0)
            {
                draggingleft = true;
            }
            else if(vertMovement != 0)
            {
                draggingUp = true;
            }
        }
        moveTiles(posx, posy, horizMovement, vertMovement);  
    }  
    
    void moveTiles(int posx, int posy, int dirX, int dirY)
    {
        
        if (positionTable[posx, posy] == 0)
        {
            return;
        }
        if (posx + dirX < 0 || posx + dirX > 25)
        {
            print("Dragged too far");
            jammed = true;
            return;
        }
        if (posy + dirY < 0 || posy + dirY > 8)
        {
            print("Dragged too far");
            jammed = true;
            return;
        }
        moveTiles(posx + dirX, posy + dirY, dirX, dirY);
        if (jammed) 
        { 
            return; 
        }
        if(draggingleft && dirY != 0)
        {
            //jammed = true;
            return;
        }
        else if(draggingUp && dirX != 0)
        {
            return;
        }
        
        
        spawnedTile[positionTable[posx, posy]].GetComponent<Tile>().move(posx + dirX, posy + dirY);
        positionTable[posx + dirX, posy + dirY] = positionTable[posx, posy];
        positionTable[posx, posy] = 0;
        return;
    }

    // Method to shuffle an array, without touching index 0
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

