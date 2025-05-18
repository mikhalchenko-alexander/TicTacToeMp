using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridPosition : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int x;
    [SerializeField] private int y;

    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.ClickedOnGridPosition(x, y);
    }
}
