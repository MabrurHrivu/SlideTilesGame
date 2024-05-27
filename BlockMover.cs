using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class BlockMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Canvas canv;
    RefList Refs;
    bool dragging = false, draggingleft = false, draggingUp = false;
    int startingX = 0, startingY = 0, dir=0;
    bool jammed =false;
    int[,] preClickTable = new int[30,10];
    Vector3Int mousePos, mousePos2, lastPos;
    GameObject activeTile;
    // Start is called before the first frame update
    void Start()
    {
        Refs = RefList.Instance;
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
        if (dragging != true)
        {
            dragging = true;
            preClickTable = (int[,])Refs.positionTable.Clone();
        }
        mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
        mousePos.z = 15;
        //convert mouse co-ords to grid co-ords and back, to get Refs.gridd co-ords
        mousePos2 = Refs.gridd.LocalToCell(mousePos);
        
        mousePos = Vector3Int.FloorToInt(Refs.gridd.GetCellCenterLocal(mousePos2));

        activeTile = Refs.spawnedTile[Refs.positionTable[mousePos2.x, mousePos2.y]];
        lastPos  = Vector3Int.FloorToInt(new Vector3Int(activeTile.GetComponent<Tile>().posX, activeTile.GetComponent<Tile>().posY, 15));
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
        mousePos.z = 15;
        mousePos2 = Refs.gridd.LocalToCell(mousePos);
        mousePos2.z = 15;
        mousePos = Vector3Int.FloorToInt(Refs.gridd.GetCellCenterLocal(mousePos2));
        if (lastPos != mousePos2)
        {
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
                    Refs.spawnedTile[preClickTable[i, j]].GetComponent<Tile>().move(i, j);
                }

            }

        }
        Refs.positionTable = (int[,])preClickTable.Clone();
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
        
        if (Refs.positionTable[posx, posy] == 0)
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

        Refs.spawnedTile[Refs.positionTable[posx, posy]].GetComponent<Tile>().move(posx + dirX, posy + dirY);
        Refs.positionTable[posx + dirX, posy + dirY] = Refs.positionTable[posx, posy];
        Refs.positionTable[posx, posy] = 0;
        return;
    }
}

