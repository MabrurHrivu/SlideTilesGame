using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{

    Image blockSprite;
    int tileID;

    [SerializeField] Color color = Color.HSVToRGB(0, 0, 0.7f);

    public int posX,posY;

    public void move(int posx, int posy)
    {
        transform.position = (RefList.Instance.gridd.GetCellCenterLocal(new Vector3Int(posx,posy,5)));
        posX = posx;
        posY= posy;
    }

    // Start is called before the first frame update
    void Start()
    {
        blockSprite = GetComponent<Image>();
        setType();
        posX = RefList.Instance.gridd.LocalToCell(transform.position).x;
        posY = RefList.Instance.gridd.LocalToCell(transform.position).y;     
    }

    void setType()
    {
        blockSprite.color = color;
    }

    public void init(int ID)
    {
        tileID = ID;
    }
}
