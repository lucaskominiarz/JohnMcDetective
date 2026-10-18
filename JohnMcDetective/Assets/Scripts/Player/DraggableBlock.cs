using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isInfiniteSpawner = false;
    public bool destroyWhenDroppedInVoid = false;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    
    private Transform initialParent;
    private Vector2 initialAnchoredPosition;
    private int initialSiblingIndex;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        SaveOriginalPosition();
    }

    public void SaveOriginalPosition()
    {
        initialParent = transform.parent;
        initialAnchoredPosition = rectTransform.anchoredPosition;
        initialSiblingIndex = transform.GetSiblingIndex();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isInfiniteSpawner)
        {
            GameObject clone = Instantiate(gameObject, transform.parent);
            clone.transform.SetSiblingIndex(transform.GetSiblingIndex());
            
            DraggableBlock cloneScript = clone.GetComponent<DraggableBlock>();
            cloneScript.isInfiniteSpawner = true;
            cloneScript.destroyWhenDroppedInVoid = false;
            cloneScript.SaveOriginalPosition();

            isInfiniteSpawner = false;
            destroyWhenDroppedInVoid = true;
        }

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        UpdateMousePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateMousePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == canvas.transform)
        {
            if (destroyWhenDroppedInVoid)
            {
                Destroy(gameObject);
            }
            else
            {
                ReturnToInitialPosition();
            }
        }
        else
        {
            SaveOriginalPosition();
        }
    }

    public void ReturnToInitialPosition()
    {
        transform.SetParent(initialParent);
        transform.SetSiblingIndex(initialSiblingIndex);
        rectTransform.localScale = Vector3.one;

        if (initialParent != null && initialParent.GetComponent<LayoutGroup>() != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(initialParent as RectTransform);
        }
        else
        {
            rectTransform.anchoredPosition = initialAnchoredPosition;
        }
    }

    private void UpdateMousePosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector3 globalMousePos
        );
        rectTransform.position = globalMousePos;
    }
}