using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class BlockSpawner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject tilePrefab;
    public Canvas canv;
    public Grid gridd;
    bool dragging = false, draggingleft = false, draggingUp = false;
    int uniqID = 1, startingX = 0, startingY = 0, dir=0;
    bool jammed =false;
    int[,] positionTable = new int[20,20];
    int[,] preClickTable = new int[20,20];
    Vector3Int mousePos, mousePos2, lastPos;
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
        print("mousePos2:" +mousePos2);
        print("lastPos:" +lastPos);
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

    // Update is called once per frame
    void Update()
    {
        
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

