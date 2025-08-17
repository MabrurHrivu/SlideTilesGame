using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{

    Image blockSprite;
    int tileID;

    [SerializeField] Color color = Color.HSVToRGB(0, 0, 0.7f);

    public int posX,posY, oldX, oldY;
    public TMP_Text myText;

    // NEW: displacement properties
    public int dispX => posX - oldX;
    public int dispY => posY - oldY;

    public void move(int posx, int posy)
    {
        transform.position = (RefList.Instance.gridd.GetCellCenterLocal(new Vector3Int(posx, posy, 5)));
        posX = posx;
        posY = posy;
        setText();
    }

    // Start is called before the first frame update
    void Start()
    {
        blockSprite = GetComponent<Image>();
        setType();
        setText();
        posX = RefList.Instance.gridd.LocalToCell(transform.position).x;
        posY = RefList.Instance.gridd.LocalToCell(transform.position).y;     
    }

    void setType()
    {
        blockSprite.color = color;
    }
    public void setText()
    {
        myText.text =(Mathf.Abs(dispX) + Mathf.Abs(dispY)).ToString();
    }

    public void init(int ID)
    {
        tileID = ID;
    }

    public void recordOldPosition()
    {
        oldX = posX;
        oldY = posY;
    }
    public int getoldX()
    {
        return oldX;
    }
    public int getoldY()
    {
        return oldY;
    }
    public int getTileID()
    {
        return tileID;
    }
}
