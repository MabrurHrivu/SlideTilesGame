using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class BlockMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    RefList Refs;
    bool dragging = false, draggingleft = false, draggingUp = false;
    int dir=0,activeLine=0 ;
    bool jammed =false;
    int[,] preClickTable;
    Vector3Int mousePos, mousePos2, lastPos;
    GameObject activeTile;
    // Start is called before the first frame update
    void Start()
    {
        Refs = RefList.Instance;
        preClickTable = new int[Refs.columns,Refs.rows];
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
        if (dragging != true)
        {
            dragging = true;
            foreach (GameObject tile in Refs.spawnedTile)
            {
                if (tile != null)
                {
                    tile.GetComponent<Tile>().recordOldPosition();
                }

            }
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
            int posX = Convert.ToInt32(mousePos2.x > lastPos.x) - Convert.ToInt32(mousePos2.x < lastPos.x);
            int posY = Convert.ToInt32(mousePos2.y > lastPos.y) - Convert.ToInt32(mousePos2.y < lastPos.y);
            checkMoveDir(activeTile.GetComponent<Tile>().posX, activeTile.GetComponent<Tile>().posY, posX, posY);
            pushTiles(activeTile.GetComponent<Tile>().posX, activeTile.GetComponent<Tile>().posY, posX, posY);
            //pullBackTiles();
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
        for (int i = 0; i < Refs.columns; i++)
        {
            for (int j = 0; j < Refs.rows; j++)
            {
                
                if (preClickTable[i, j] != 0)
                {
                    Refs.spawnedTile[preClickTable[i, j]].GetComponent<Tile>().move(i, j);
                }

            }

        }
        Refs.positionTable = (int[,])preClickTable.Clone();
    }


    public void checkMoveDir(int posx, int posy,int posX, int posY)
    {
        if(!draggingleft && !draggingUp)
        {
            if(posX != 0)
            {
                draggingleft = true;
                activeLine = posx;
            }
            else if(posY != 0)
            {
                draggingUp = true;
                activeLine = posy;
            }
        }  
    }  
    
    void pushTiles(int posx, int posy, int dirX, int dirY)
    {
        
        if (Refs.positionTable[posx, posy] == 0)
        {
            return;
        }

        //ignore vertical movement when dragging sideways
        if(draggingleft && dirY != 0)
        {
            //jammed = true;
            return;
        }

        //ignore horizontal movement when dragging vertically
        else if(draggingUp && dirX != 0)
        {
            return;
        }
        if (posx + dirX < 0 || posx + dirX > Refs.columns-1)
        {
            print("Dragged too far");
            jammed = true;
            return;
        }
        if (posy + dirY < 0 || posy + dirY > Refs.rows-1)
        {
            print("Dragged too far");
            jammed = true;
            return;
        }
        pushTiles(posx + dirX, posy + dirY, dirX, dirY);
        if (jammed) 
        { 
            return; 
        }
        /*
*/
        Refs.spawnedTile[Refs.positionTable[posx, posy]].GetComponent<Tile>().move(posx + dirX, posy + dirY);
        Refs.positionTable[posx + dirX, posy + dirY] = Refs.positionTable[posx, posy];
        Refs.positionTable[posx, posy] = 0;
        return;
    }
    void pullBackTiles()
    {
        if (draggingleft)
        {
            for (int i=0; i<Refs.columns;i++)
            {
                GameObject currentObject = Refs.spawnedTile[Refs.positionTable[i,activeLine]];
                if (currentObject != null)
                {
                    Tile currentTile = currentObject.GetComponent<Tile>();
                    int dist = i - currentTile.getoldX();
                    currentTile.setText(dist.ToString());
                }
            }
        }
        else if (draggingUp)
        {
            for (int j=0; j<Refs.rows;j++)
            {
                GameObject currentObject = Refs.spawnedTile[Refs.positionTable[activeLine,j]];
                if (currentObject != null)
                {
                    Tile currentTile = currentObject.GetComponent<Tile>();
                    int dist = j - currentTile.getoldY();
                    currentTile.setText(dist.ToString());
                }    
            }
        }
    }
}

