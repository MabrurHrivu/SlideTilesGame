using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Grid snappy;
    Image blockSprite;
    int tileID;

    [SerializeField] Color color = Color.HSVToRGB(0, 0, 0.7f);

    public int posX,posY;

    public void move(int posx, int posy)
    {
        transform.position = (snappy.GetCellCenterLocal(new Vector3Int(posx,posy,5)));
        posX = posx;
        posY= posy;
    }

    // Start is called before the first frame update
    void Start()
    {
        snappy = GameObject.FindGameObjectWithTag("snapping grid").GetComponent<Grid>();
        blockSprite = GetComponent<Image>();
        //float randomHue = UnityEngine.Random.Range(0f, 1f);
        //Color randomColor = Color.HSVToRGB(randomHue, 0.7f, 1);
        //blockSprite.color = randomColor;
        setType();
        posX = snappy.LocalToCell(transform.position).x;
        posY = snappy.LocalToCell(transform.position).y;     
    }

    void setType()
    {
        blockSprite.color = color;
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
