using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public GameObject tilePrefab;
    public Canvas canv;
    public Grid gridd;
    bool dragging = false, draggingleft = false, draggingUp = false;
    int uniqID = 1, startingX = 0, startingY = 0;
    bool jammed =false;
    int[,] positionTable = new int[20,20];
    int[,] preClickTable = new int[20,20];
    GameObject[] spawnedTile;
    GameObject activeTile;
    // Start is called before the first frame update
    void Start()
    {
        spawnedTile = new GameObject[100];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                spawnedTile[uniqID] = Instantiate(tilePrefab, gridd.GetCellCenterLocal(new Vector3Int(i, j, 1)), Quaternion.identity, canv.transform);
                spawnedTile[uniqID].GetComponent<Tile>().init(uniqID);
                positionTable[i, j] = uniqID;
                uniqID++;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startDragState(int tileID)
    {
        if (dragging != true)
        {
            dragging = true;
            preClickTable = (int[,])positionTable.Clone();
        }
        activeTile = spawnedTile[tileID];
    }
    public void endDragState()
    {
        jammed = false;
        dragging = false;
        draggingUp = false;
        draggingleft = false;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                
                if (preClickTable[i, j] != 0)
                {
                    spawnedTile[preClickTable[i, j]].GetComponent<Tile>().move(i, j);
                    print("tried moving " + i + ", " +j);
                }

            }

        }
        positionTable = (int[,])preClickTable.Clone();

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
        if (posx + dirX < 0)
        {
            print("Dragged too far");
            jammed = true;
            return;
        }
        if (posy + dirY < 0)
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
}

