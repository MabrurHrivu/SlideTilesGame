using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Grid snappy;
    [SerializeField]
    BlockSpawner Bloccy;
    Image blockSprite;
    int posX, posY;
    int deltaX, deltaY;
    int dir;
    int tileID;
    int windX = 0, windY = 0;
    public void OnBeginDrag(PointerEventData eventData)
    {
        //print("start drag");
        Bloccy.startDragState();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3Int mousePos = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
        mousePos.z = 15;
        Vector3Int lastPos  = Vector3Int.FloorToInt(transform.position);

        //convert mouse co-ords to grid co-ords and back, to get snappy co-ords
        Vector3Int mousePos2 = snappy.LocalToCell(mousePos);
        mousePos = Vector3Int.FloorToInt(snappy.GetCellCenterLocal(mousePos2));
        
        //transform.position = mousePos;
        if (lastPos != mousePos)
        {
            //print("changed X:" + transform.position.x + " Y:" + transform.position.y + " Z:" + transform.position.z);
            dir =checkDir(lastPos.x, mousePos.x, lastPos.y, mousePos.y);
            Bloccy.checkMove(posX, posY,dir);
        }
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //print("end drag");
        Bloccy.endDragState();
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
    public void move(int posx, int posy)
    {
        transform.position = (snappy.GetCellCenterLocal(new Vector3Int(posx,posy,5)));
        posX = posx;
        posY= posy;
    }
    void wind(int x, int y)
    {
        windX+= x;
        windY+= y;
    }
    void unwind(int x, int y)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        Bloccy = GameObject.FindGameObjectWithTag("logic").GetComponent<BlockSpawner>();
        snappy = GameObject.FindGameObjectWithTag("snapping grid").GetComponent<Grid>();
        blockSprite = GetComponent<Image>();
        float randomHue = UnityEngine.Random.Range(0f, 1f);
        Color randomColor = Color.HSVToRGB(randomHue, 0.7f, 1);
        blockSprite.color = randomColor;
        posX = snappy.LocalToCell(transform.position).x;
        posY = snappy.LocalToCell(transform.position).y;
    }

    public void init(int ID)
    {
        tileID = ID;
    }
    // Update is called once per frame
    void Update()
    {


    }
}
