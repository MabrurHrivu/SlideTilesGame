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
    private TileDeathAnimation deathAnimation;

    public bool IsMoving => isMoving || moveQueue.Count > 0;

    // NEW: displacement properties
    public int dispX => posX - oldX;
    public int dispY => posY - oldY;

    public void move(int posx, int posy)
    {
        Vector3 target = RefList.Instance.gridd.GetCellCenterLocal(new Vector3Int(posx, posy, 5));
        if (Refs.IsSmoothMode())
        {
            
            // Queue the movement
            moveQueue.Enqueue(target);

            // Start processing queue if not already
            if (!isMoving)
            {
                isMoving = true;
                StartCoroutine(ProcessQueue());
            }
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
        while (moveQueue.Count > 0)
        {
            Vector3 target = moveQueue.Dequeue();
            yield return StartCoroutine(SmoothMove(target));
        }

        isMoving = false;
    }

    private IEnumerator SmoothMove(Vector3 target, float duration = 0.6f)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float x = Mathf.Clamp01(t);

            // EaseOutBounce curve
            float n1 = 7.5625f;
            float d1 = 2.75f;
            float easeT;

            if (x < 1f / d1)
            {
                easeT = n1 * x * x;
            }
            else if (x < 2f / d1)
            {
                x -= 1.5f / d1;
                easeT = n1 * x * x + 0.75f;
            }
            else if (x < 2.5f / d1)
            {
                x -= 2.25f / d1;
                easeT = n1 * x * x + 0.9375f;
            }
            else
            {
                x -= 2.625f / d1;
                easeT = n1 * x * x + 0.984375f;
            }

            transform.position = Vector3.Lerp(start, target, easeT);

            yield return null;
        }

        transform.position = target; // snap exact
    }

    void Awake()
    {
        tileData = GetComponent<TileData>();  // auto-find
        blockSprite = GetComponent<Image>();
        deathAnimation = GetComponent<TileDeathAnimation>();
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

    public IEnumerator PlayDeathAnimation()
    {
        if (deathAnimation != null)
        {
            yield return StartCoroutine(deathAnimation.Play(this));
            yield break;
        }

        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, 180f);
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        transform.localRotation = endRotation;
    }
}
