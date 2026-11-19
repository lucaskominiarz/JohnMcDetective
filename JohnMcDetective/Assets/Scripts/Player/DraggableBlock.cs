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
    
    private DraggableBlock _spawnerOwner;
    private GameObject _activeSpawnedInstance;
    private bool _isDraggedCopy = false;
    private bool _spawnedFromInfinite = false;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_cameraScrollView == null && Camera.main != null)
            _cameraScrollView = Camera.main.GetComponent<ScrollView>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        SaveOriginalPosition();
    }

    private void OnDestroy()
    {
        if (_isDraggedCopy && _spawnerOwner != null)
        {
            _spawnerOwner.NotifyInstanceDestroyed(gameObject);
        }
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
            cloneScript._spawnedFromInfinite = false;
            cloneScript.SaveOriginalPosition();

            isInfiniteSpawner = false;
            destroyWhenDroppedInVoid = true;
            _spawnedFromInfinite = true;
        }
        else if (!_isDraggedCopy && !_spawnedFromInfinite)
        {
            if (_activeSpawnedInstance != null) return;

            GameObject clone = Instantiate(gameObject, transform.parent);
            clone.transform.SetSiblingIndex(transform.GetSiblingIndex());

            DraggableBlock cloneScript = clone.GetComponent<DraggableBlock>();
            cloneScript.isInfiniteSpawner = false;
            cloneScript.destroyWhenDroppedInVoid = destroyWhenDroppedInVoid;
            cloneScript._spawnedFromInfinite = false;
            cloneScript._spawnerOwner = this;
            cloneScript._isDraggedCopy = true;
            cloneScript.SaveOriginalPosition();

            _activeSpawnedInstance = clone;
            SetPlaceholderActive(false);

            eventData.pointerDrag = clone;
            cloneScript.OnBeginDrag(eventData);
            return;
        }

        transform.SetParent(_canvas.transform, true);
        transform.SetAsLastSibling();

        _canvasGroup.blocksRaycasts = false;
        UpdateMousePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateMousePosition(eventData);
        if (_cameraScrollView != null)
            _cameraScrollView.ChangeCanDragState(0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        if (_cameraScrollView != null)
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

    private void SetPlaceholderActive(bool active)
    {
        _canvasGroup.interactable = active;
        _canvasGroup.blocksRaycasts = active;
        _canvasGroup.alpha = active ? 1f : 0.35f;
    }

    public void NotifyInstanceDestroyed(GameObject destroyedInstance)
    {
        if (_activeSpawnedInstance == destroyedInstance)
        {
            _activeSpawnedInstance = null;
            SetPlaceholderActive(true);
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