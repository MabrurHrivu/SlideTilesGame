using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    RefList Refs;
    bool dragging = false, draggingleft = false, draggingUp = false;
    int activeLine = -1;
    bool jammed = false;

    // New: whether this drag has a valid tile under cursor
    bool dragValid = false;

    Vector3Int mousePos, lastPos;
    GameObject activeTile;

    void Start()
    {
        Refs = RefList.Instance;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        //tell the tiles to write down their positions before rrag
        foreach (GameObject tile in Refs.spawnedTile)
        {
            if (tile != null)
                tile.GetComponent<Tile>().recordOldPosition();
        }
        mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)); //mouse position relative to game
        mousePos = Refs.gridd.LocalToCell(mousePos); //mouse position to cell co-ordinate

        GameObject tileGO = Refs.spawnedTile[Refs.positionTable[mousePos.x, mousePos.y]];
        if (tileGO == null)
            return;

        activeTile = tileGO;
        Tile tileComp = activeTile.GetComponent<Tile>();
        if (tileComp == null)
            return;

        lastPos = new Vector3Int(tileComp.posX, tileComp.posY, 15);
        dragValid = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || !dragValid) return;

        mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)); //mouse position relative to game
        mousePos = Refs.gridd.LocalToCell(mousePos); //mouse position to cell co-ordinate
        mousePos.z = 15; // to make sure the next condition is only true when it should be

        if (lastPos != mousePos)
        {
            var tileComp = activeTile.GetComponent<Tile>();

            bool axisAligned = true;
            if (draggingleft && lastPos.x != tileComp.posX) axisAligned = false;
            if (draggingUp   && lastPos.y != tileComp.posY) axisAligned = false;

            if (axisAligned)
            {
                jammed = false;

                int posX = Math.Sign(mousePos.x - lastPos.x);
                int posY = Math.Sign(mousePos.y - lastPos.y);

                // only proceed if movement matches drag axis
                bool validMove = !(draggingleft && posY != 0) && !(draggingUp && posX != 0);

                if (validMove)
                {
                    checkMoveDir(tileComp.posX, tileComp.posY, posX, posY);
                    pushTiles(tileComp.posX, tileComp.posY, posX, posY);
                    pullBackTiles();
                }
            }

            // update the cell that the drag is compared against
            lastPos = mousePos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        activeTile = null;
        pullBackTiles();
        jammed = false;
        dragging = false;
        draggingUp = false;
        draggingleft = false;
        dragValid = false;
        activeLine = -1;
    }
    public void checkMoveDir(int posx, int posy, int posX, int posY)
    {
        if (!draggingleft && !draggingUp)
        {
            if (posX != 0) { draggingleft = true;}
            else if (posY != 0) { draggingUp = true;}
        }
    }

    void pushTiles(int posx, int posy, int dirX, int dirY)
    {
        int tileID = Refs.positionTable[posx, posy];
        if (tileID == 0 || Refs.spawnedTile[tileID] == null) return;

        // Set activeLine on first move
        if (activeLine == -1)  // assuming -1 = unset
        {
            if (draggingleft) activeLine = posy;   // row
            if (draggingUp)   activeLine = posx;   // column
        }

        int nextX = posx + dirX;
        int nextY = posy + dirY;

        if (nextX < 0 || nextX >= Refs.columns || nextY < 0 || nextY >= Refs.rows)
        {
            jammed = true;
            return;
        }

        if (Refs.positionTable[nextX, nextY] != 0)
        {
            pushTiles(nextX, nextY, dirX, dirY);
            if (jammed) return;
        }

        Refs.spawnedTile[tileID].GetComponent<Tile>().move(nextX, nextY);
        Refs.positionTable[nextX, nextY] = tileID;
        Refs.positionTable[posx, posy] = 0;
    }

   void pullBackTiles()
    {
        if (activeLine == -1) return;

        bool moved;
        do
        {
            moved = false;

            // Horizontal pullback
            if (draggingleft)
            {
                for (int x = 0; x < Refs.columns; x++)
                {
                    int tileID = Refs.positionTable[x, activeLine];
                    if (tileID == 0 || Refs.spawnedTile[tileID] == null || Refs.spawnedTile[tileID] == activeTile) continue;

                    Tile tile = Refs.spawnedTile[tileID].GetComponent<Tile>();
                    if (tile.dispX == 0) continue;

                    int stepX = tile.dispX > 0 ? -1 : 1;
                    int targetX = x + stepX;

                    if (targetX >= 0 && targetX < Refs.columns && Refs.positionTable[targetX, activeLine] == 0)
                    {
                        tile.move(targetX, activeLine);
                        Refs.positionTable[targetX, activeLine] = tileID;
                        Refs.positionTable[x, activeLine] = 0;
                        moved = true;
                    }
                }
            }
            // Vertical pullback
            else if (draggingUp)
            {
                for (int y = 0; y < Refs.rows; y++)
                {
                    int tileID = Refs.positionTable[activeLine, y];
                    if (tileID == 0 || Refs.spawnedTile[tileID] == null || Refs.spawnedTile[tileID] == activeTile) continue;

                    Tile tile = Refs.spawnedTile[tileID].GetComponent<Tile>();
                    if (tile.dispY == 0) continue;

                    int stepY = tile.dispY > 0 ? -1 : 1;
                    int targetY = y + stepY;

                    if (targetY >= 0 && targetY < Refs.rows && Refs.positionTable[activeLine, targetY] == 0)
                    {
                        tile.move(activeLine, targetY);
                        Refs.positionTable[activeLine, targetY] = tileID;
                        Refs.positionTable[activeLine, y] = 0;
                        moved = true;
                    }
                }
            }

        } while (moved);
    }

}
