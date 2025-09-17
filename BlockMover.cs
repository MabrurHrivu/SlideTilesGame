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
        Refs.DisableSmooth();
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
            if (draggingUp && lastPos.y != tileComp.posY) axisAligned = false;

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
        Refs.EnableSmooth();
        activeTile = null;
        if (!checkMatches()) pullBackTiles();
        else
        {
            CompactBoard();
        }
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
            if (posX != 0) { draggingleft = true; }
            else if (posY != 0) { draggingUp = true; }
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
            if (draggingUp) activeLine = posx;   // column
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

        int cols = Refs.columns;
        int rows = Refs.rows;
        int limit = draggingleft ? cols : rows;

        // Save initial positions for this line only
        var initialPos = new Dictionary<int, Vector2Int>();
        for (int i = 0; i < limit; i++)
        {
            int id = draggingleft ? Refs.positionTable[i, activeLine] 
                                : Refs.positionTable[activeLine, i];
            if (id != 0 && Refs.spawnedTile[id] != null && !initialPos.ContainsKey(id))
            {
                Tile t = Refs.spawnedTile[id].GetComponent<Tile>();
                initialPos[id] = new Vector2Int(t.posX, t.posY);
            }
        }

        // Clone just the logical table
        int[,] temp = (int[,])Refs.positionTable.Clone();

        bool moved;
        do
        {
            moved = false;

            for (int i = 0; i < limit; i++)
            {
                int tileID = draggingleft ? temp[i, activeLine] 
                                        : temp[activeLine, i];
                if (tileID == 0 || Refs.spawnedTile[tileID] == null || Refs.spawnedTile[tileID] == activeTile) continue;

                Tile tile = Refs.spawnedTile[tileID].GetComponent<Tile>();
                if (tile == null) continue;

                int disp = draggingleft ? tile.dispX : tile.dispY;
                if (disp == 0) continue;

                int step = disp > 0 ? -1 : 1;
                int target = i + step;

                bool inBounds = draggingleft ? (target >= 0 && target < cols) 
                                            : (target >= 0 && target < rows);
                if (!inBounds) continue;

                int nextID = draggingleft ? temp[target, activeLine] 
                                        : temp[activeLine, target];
                if (nextID != 0) continue; // blocked

                // --- simulate move ---
                if (draggingleft)
                {
                    temp[target, activeLine] = tileID;
                    temp[i, activeLine] = 0;
                    tile.posX = target; // logical only
                }
                else
                {
                    temp[activeLine, target] = tileID;
                    temp[activeLine, i] = 0;
                    tile.posY = target; // logical only
                }

                moved = true;
            }

        } while (moved);

        // Commit final logical state
        Refs.positionTable = temp;

        // Apply visual moves for tiles that changed
        foreach (var kv in initialPos)
        {
            int id = kv.Key;
            Tile tile = Refs.spawnedTile[id]?.GetComponent<Tile>();
            if (tile == null) continue;

            Vector2Int from = kv.Value;
            if (tile.posX != from.x || tile.posY != from.y)
                tile.move(tile.posX, tile.posY);
        }
    }


    bool checkMatches()
    {
        // Skip check if no drag is active
        if (!draggingleft && !draggingUp) return false;

        bool matched = false;
        int limit = draggingleft ? Refs.columns : Refs.rows;

        for (int i = 0; i < limit; i++)
        {
            // Select the tile from the active row or column
            Tile tile = draggingleft
                ? Refs.spawnedTile[Refs.positionTable[i, activeLine]]?.GetComponent<Tile>()
                : Refs.spawnedTile[Refs.positionTable[activeLine, i]]?.GetComponent<Tile>();

            if (tile == null) continue;

            // Only tiles with a displacement value can match
            if (tile.getDisp() != 0)
            {
                // Check for matching neighbor in each direction
                if (tryMatchNeighbor(tile, tile.posX, tile.posY + 1))      matched = true; // up
                else if (tryMatchNeighbor(tile, tile.posX + 1, tile.posY)) matched = true; // right
                else if (tryMatchNeighbor(tile, tile.posX, tile.posY - 1)) matched = true; // down
                else if (tryMatchNeighbor(tile, tile.posX - 1, tile.posY)) matched = true; // left
            }
        }

        return matched;
    }

    bool tryMatchNeighbor(Tile tile, int neighborX, int neighborY)
    {
        // bounds check
        if (neighborX < 0 || neighborY < 0 ||
            neighborX >= Refs.columns || neighborY >= Refs.rows)
            return false;

        Tile neighbor = Refs.spawnedTile[Refs.positionTable[neighborX, neighborY]]
                        ?.GetComponent<Tile>();
        if (neighbor == null) return false;

        if (neighbor.getDisp() == 0 &&
            tile.GetTileType() == neighbor.GetTileType())
        {
            removeTile(tile);
            removeTile(neighbor);
            return true;
        }

        return false;
    }
    void removeTile(Tile tile)
    {
        int x = tile.posX;
        int y = tile.posY;
        int id = Refs.positionTable[x, y];

        if (id != 0)
        {
            Refs.positionTable[x, y] = 0;
            Refs.spawnedTile[id] = null;
        }

        Destroy(tile.gameObject);
    }
    void CompactBoard()
    {
        int cols = Refs.columns;
        int rows = Refs.rows;

        // Work on a fresh table
        int[,] newTable = new int[cols, rows];

        int writeX = 0;
        for (int readX = 0; readX < cols; readX++)
        {
            // Check if column has any tile
            bool colHasTile = false;
            for (int y = 0; y < rows; y++)
            {
                if (Refs.positionTable[readX, y] != 0) { colHasTile = true; break; }
            }
            if (!colHasTile) continue;

            // Copy this column into newTable at writeX
            for (int y = 0; y < rows; y++)
                newTable[writeX, y] = Refs.positionTable[readX, y];

            writeX++;
        }

        // Now compact rows inside the already compacted columns
        int[,] finalTable = new int[cols, rows];
        int writeY = 0;
        for (int readY = 0; readY < rows; readY++)
        {
            bool rowHasTile = false;
            for (int x = 0; x < cols; x++)
            {
                if (newTable[x, readY] != 0) { rowHasTile = true; break; }
            }
            if (!rowHasTile) continue;

            for (int x = 0; x < cols; x++)
            {
                int id = newTable[x, readY];
                finalTable[x, writeY] = id;

                if (id != 0 && Refs.spawnedTile[id] != null)
                    Refs.spawnedTile[id].GetComponent<Tile>()?.move(x, writeY);
            }

            writeY++;
        }

        Refs.positionTable = finalTable;
    }

}
