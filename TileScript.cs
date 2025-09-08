using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    private Image blockSprite;
    private TileData tileData;   // now auto-assigned
    private int tileID;

    public int posX, posY, oldX, oldY;
    public TMP_Text myText;

    // NEW: displacement properties
    public int dispX => posX - oldX;
    public int dispY => posY - oldY;

    public void move(int posx, int posy)
    {
        transform.position = RefList.Instance.gridd.GetCellCenterLocal(new Vector3Int(posx, posy, 5));
        posX = posx;
        posY = posy;
        setText();
    }

    void Awake()
    {
        tileData = GetComponent<TileData>();  // auto-find
        blockSprite = GetComponent<Image>();
    }

    void Start()
    {
        setType();
        setText();

        posX = RefList.Instance.gridd.LocalToCell(transform.position).x;
        posY = RefList.Instance.gridd.LocalToCell(transform.position).y;
    }

    void setType()
    {
        if (tileData != null)
            blockSprite.color = tileData.color;
    }

    public void setText()
    {
        myText.text = getDisp().ToString();
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

    public int getTileID()
    {
        return tileID;
    }

    // Expose tileType
    public string GetTileType()
    {
        return tileData != null ? tileData.tileType : "Unknown";
    }
    public int getDisp()
    {
        return Mathf.Abs(dispX) + Mathf.Abs(dispY);
    }
}
