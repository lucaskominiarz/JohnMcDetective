using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isInfiniteSpawner = false;
    public bool destroyWhenDroppedInVoid = false;

    private RectTransform _rectTransform;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    
    private Transform _initialParent;
    private Vector2 _initialAnchoredPosition;
    private int _initialSiblingIndex;
    private ScrollView _cameraScrollView;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _cameraScrollView = Camera.main.GetComponent<ScrollView>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        SaveOriginalPosition();
    }

    public void SaveOriginalPosition()
    {
        _initialParent = transform.parent;
        _initialAnchoredPosition = _rectTransform.anchoredPosition;
        _initialSiblingIndex = transform.GetSiblingIndex();
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

        transform.SetParent(_canvas.transform, true);
        transform.SetAsLastSibling();

        _canvasGroup.blocksRaycasts = false;
        UpdateMousePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateMousePosition(eventData);
        _cameraScrollView.ChangeCanDragState(0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        _cameraScrollView.ChangeCanDragState(1);
        if (transform.parent == _canvas.transform)
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
        transform.SetParent(_initialParent);
        transform.SetSiblingIndex(_initialSiblingIndex);
        _rectTransform.localScale = Vector3.one;

        if (_initialParent != null && _initialParent.GetComponent<LayoutGroup>() != null)
        {
            _rectTransform.anchoredPosition = Vector2.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_initialParent as RectTransform);
        }
        else
        {
            _rectTransform.anchoredPosition = _initialAnchoredPosition;
        }
    }

    private void UpdateMousePosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out Vector3 globalMousePos
        );
        _rectTransform.position = globalMousePos;
    }
}