using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class whitedot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Grid snappy;

    public void OnBeginDrag(PointerEventData eventData)
    {
        print("start drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 lastpos = transform.position;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mousePos2 = snappy.LocalToCell(mousePos);
        mousePos = snappy.GetCellCenterLocal(mousePos2);
        mousePos.z = 0;
        if (lastpos != mousePos)
        {
            print("changed");
        }
        transform.position = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        print("end drag");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
}
