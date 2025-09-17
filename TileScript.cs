using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    private Image blockSprite;
    private TileData tileData;   // now auto-assigned
    private int tileID;

    public int posX, posY, oldX, oldY;
    //public bool smoothMode = false; // Control from outside (dragging = false, auto = true)

    private Queue<Vector3> moveQueue = new Queue<Vector3>();
    private bool isMoving = false;
    RefList Refs;
    public TMP_Text myText;

    // NEW: displacement properties
    public int dispX => posX - oldX;
    public int dispY => posY - oldY;

    public void move(int posx, int posy)
    {
        Vector3 target = RefList.Instance.gridd.GetCellCenterLocal(new Vector3Int(posx, posy, 5));
        print(Refs);
        if (Refs.IsSmoothMode())
        {
            
            // Queue the movement
            moveQueue.Enqueue(target);

            // Start processing queue if not already
            if (!isMoving)
                StartCoroutine(ProcessQueue());
        }
        else
        {
            // Snap instantly
            transform.position = target;
        }

        /*
        // Queue the movement
        moveQueue.Enqueue(target);

        // Start processing queue if not already
        if (!isMoving)
            StartCoroutine(ProcessQueue());
        */
        posX = posx;
        posY = posy;
        setText();
    }

    private IEnumerator ProcessQueue()
    {
        isMoving = true;

        while (moveQueue.Count > 0)
        {
            Vector3 target = moveQueue.Dequeue();
            yield return StartCoroutine(SmoothMove(target));
        }

        isMoving = false;
    }

    private IEnumerator SmoothMove(Vector3 target)
    {
        Vector3 start = transform.position;
        float t = 0f;
        float duration = 0.8f; // adjust speed

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            // Bounce-like easing (ease out + little overshoot)
            float easeT = Mathf.Sin(t * Mathf.PI * 0.5f); // simple ease-out
            transform.position = Vector3.Lerp(start, target, easeT);

            yield return null;
        }

        transform.position = target; // snap exact
    }

    void Awake()
    {
        tileData = GetComponent<TileData>();  // auto-find
        blockSprite = GetComponent<Image>();
    }

    void Start()
    {
        Refs = RefList.Instance;
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
