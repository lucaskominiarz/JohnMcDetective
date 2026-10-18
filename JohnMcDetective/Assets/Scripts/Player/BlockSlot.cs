using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject != null)
        {
            droppedObject.transform.SetParent(transform);
            
            RectTransform rt = droppedObject.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;

            if (transform.GetComponent<LayoutGroup>() != null)
            {
                rt.anchoredPosition = Vector2.zero;
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            }
        }
    }
}