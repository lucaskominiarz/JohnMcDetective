using System;
using NaughtyAttributes;
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

    [Header("Paramètres")] [SerializeField]
    private bool uiOnUse = false;

    [ShowIf("HaveUi")] [SerializeField] private GameObject uiObject;

    private Transform _originalParent;
    private Vector3 _originalPosition;

    private bool _canDrag = false;

    private NPC actualNpc;

   bool HaveUi(){return uiOnUse;}
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_canDrag)
            return;
        IsDraggingAnyItem = true;

        _originalParent = transform.parent;
        _originalPosition = transform.position;

        transform.SetParent(transform.parent.parent);
        transform.SetAsLastSibling();

        itemIcon.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_canDrag)
            return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDraggingAnyItem = false;

        itemIcon.raycastTarget = true;
        
        if (!_canDrag)
            return;
        
        DetectDropOnNPC(eventData.position);

        transform.SetParent(_originalParent);
        transform.position = _originalPosition;
    }

    private void DetectDropOnNPC(Vector2 screenPosition)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider != null)
        {
            actualNpc = hit.collider.GetComponent<NPC>();
            
            if (actualNpc != null && itemData != null)
            {
                if (itemData.ObjectType == ObjectType.Mandat && uiOnUse)
                {
                    uiObject.SetActive(true);
                }
                else
                {
                    actualNpc.UseObject(itemData);
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (itemData == null)
            return;
        if (itemData.ObjectType == ObjectType.Mandat)
            LockCode.OnOpened += () =>
            {
                _canDrag = true; 
                itemIcon.sprite = itemData.ObjectSprite;
            };
        if (itemIcon != null)
        {
            if (!_canDrag)
                itemIcon.sprite = itemData.ObjectUnactiveSprite;
            else
                itemIcon.sprite = itemData.ObjectSprite;
        }
        
    }

    public void ConfirmArrestation()
    {
        actualNpc.UseObject(itemData);
        Destroy(gameObject);
    }
    
}