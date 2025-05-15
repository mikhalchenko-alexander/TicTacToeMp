using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridPosition : MonoBehaviour, IPointerDownHandler
{
    
    [SerializeField] private int x;
    [SerializeField] private int y;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.LogFormat("Click {0}, {1}", x, y); 
    }
}
