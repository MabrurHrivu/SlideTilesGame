using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Grid snappy;
    [SerializeField]
    BlockSpawner Bloccy;
    Image blockSprite;
    int deltaX, deltaY, dir, tileID, initX, initY;

    public int posX,posY;
    bool outMoused = false;
    int windX = 0, windY = 0;
    


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
