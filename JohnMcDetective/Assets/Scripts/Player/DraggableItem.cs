using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static bool IsDraggingAnyItem { get; private set; }

    [Header("Données de l'objet")]
    public ObjectScriptable itemData;

    [Header("Composants UI")]
    [SerializeField] private Image itemIcon;

    private Transform originalParent;
    private Vector3 originalPosition;

    private void Start()
    {
        if (itemData != null && itemIcon != null)
        {
            itemIcon.sprite = itemData.ObjectSprite;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDraggingAnyItem = true;

        originalParent = transform.parent;
        originalPosition = transform.position;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        itemIcon.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDraggingAnyItem = false;

        itemIcon.raycastTarget = true;

        DetectDropOnNPC(eventData.position);

        transform.SetParent(originalParent);
        transform.position = originalPosition;
    }

    private void DetectDropOnNPC(Vector2 screenPosition)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider != null)
        {
            NPC targetNPC = hit.collider.GetComponent<NPC>();
            if (targetNPC != null && itemData != null)
            {
                targetNPC.UseObject(itemData);
                Destroy(gameObject);
            }
        }
    }
}