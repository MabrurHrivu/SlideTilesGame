using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    RefList Refs;
    bool dragging = false, draggingleft = false, draggingUp = false;
    int dir = 0, activeLine = -1;
    bool jammed = false;

    // New: whether this drag has a valid tile under cursor
    bool dragValid = false;

    int[,] preClickTable;
    Vector3Int mousePos, mousePos2, lastPos;
    GameObject activeTile;

    void Start()
    {
        Refs = RefList.Instance;
        preClickTable = new int[Refs.columns, Refs.rows];
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // reset per-drag state
        dragValid = false;
        activeTile = null;

        if (!dragging)
        {
            dragging = true;
            foreach (GameObject tile in Refs.spawnedTile)
            {
                if (tile != null)
                    tile.GetComponent<Tile>().recordOldPosition();
            }
            preClickTable = (int[,])Refs.positionTable.Clone();
        }

        mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        mousePos.z = 15;
        mousePos2 = Refs.gridd.LocalToCell(mousePos);
        mousePos = Vector3Int.FloorToInt(Refs.gridd.GetCellCenterLocal(mousePos2));

        GameObject tileGO = Refs.spawnedTile[Refs.positionTable[mousePos2.x, mousePos2.y]];
        if (tileGO == null)
            return;

        activeTile = tileGO;
        Tile tileComp = activeTile.GetComponent<Tile>();
        if (tileComp == null)
            return;

        lastPos = new Vector3Int(tileComp.posX, tileComp.posY, 15);
        dragValid = true; // ✅ only now do we consider this drag valid
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || !dragValid) return; // gate the whole drag

        mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        mousePos.z = 15;
        mousePos2 = Refs.gridd.LocalToCell(mousePos);
        mousePos2.z = 15;

        if (lastPos != mousePos2)
            {
                var tileComp = activeTile.GetComponent<Tile>();
                //set posX and posY to -1 or 1 depending on the direction of drag
                int posX = Convert.ToInt32(mousePos2.x > lastPos.x) - Convert.ToInt32(mousePos2.x < lastPos.x);
                int posY = Convert.ToInt32(mousePos2.y > lastPos.y) - Convert.ToInt32(mousePos2.y < lastPos.y);
                checkMoveDir(tileComp.posX, tileComp.posY, posX, posY);
                pushTiles(tileComp.posX, tileComp.posY, posX, posY);
                pullBackTiles();
                lastPos = mousePos2;
            }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        jammed = false;
        dragging = false;
        draggingUp = false;
        draggingleft = false;
        dragValid = false;
        activeTile = null;
        activeLine = -1;

        cancelMove(); // keep your original behavior
    }

    void cancelMove()
    {
        for (int i = 0; i < Refs.columns; i++)
        {
            for (int j = 0; j < Refs.rows; j++)
            {
                if (preClickTable[i, j] != 0)
                    Refs.spawnedTile[preClickTable[i, j]].GetComponent<Tile>().move(i, j);
            }
        }
        Refs.positionTable = (int[,])preClickTable.Clone();
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

        if (draggingleft && dirY != 0) return;
        if (draggingUp && dirX != 0) return;

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

            if (draggingleft) // horizontal pullback along row
            {
                for (int x = 0; x < Refs.columns; x++)
                {
                    int tileID = Refs.positionTable[x, activeLine];
                    if (tileID == 0 || Refs.spawnedTile[tileID] == null || Refs.spawnedTile[tileID] == activeTile) continue;

                    Tile tile = Refs.spawnedTile[tileID].GetComponent<Tile>();

                    if (tile.dispX != 0)
                    {
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
            }
            else if (draggingUp) // vertical pullback along column
            {
                for (int y = 0; y < Refs.rows; y++)
                {
                    int tileID = Refs.positionTable[activeLine, y];
                    if (tileID == 0 || Refs.spawnedTile[tileID] == null || Refs.spawnedTile[tileID] == activeTile) continue;

                    Tile tile = Refs.spawnedTile[tileID].GetComponent<Tile>();

                    if (tile.dispY != 0)
                    {
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
            }

        } while (moved);
    }
}
