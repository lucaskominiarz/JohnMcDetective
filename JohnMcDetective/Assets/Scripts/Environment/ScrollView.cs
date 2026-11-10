using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ScrollView : MonoBehaviour
{
    [SerializeField] private Vector2 pageSize = new Vector2(4.5f, 10f);
    [SerializeField] private float snapSpeed = 15f;
    [SerializeField] private float velocityThreshold = 5f;
    [SerializeField] private float dragThresholdPixels = 20f;
    [SerializeField] private MapData mapData;
    [SerializeField] private float positionAccurancyThreshold = 0.2f;
    [SerializeField] private GameObject bookUi;
    [SerializeField] private float bookMultiplier = 1000f;
    [SerializeField] private Vector2 minMaxBookHeight;

    private Camera _cam;
    private Vector3 _targetPosition;
    private Vector2 _touchStartScreenPos;
    private Vector2 _lastScreenPos;
    private Vector2 _touchVelocity;
    private bool _isDragging = false;
    private Vector3 _targetUiPosition;

    private bool _canDrag = true;

    private Vector3 _startCamPos;
    private Vector3 _startBookPos;

    private bool _isInOverWorld = true;

    private enum DragDirection { None, Horizontal, Vertical }
    private DragDirection _currentDragDirection = DragDirection.None;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        transform.position = new Vector3(mapData.startPosition.x * pageSize.x, 
            mapData.startPosition.y * pageSize.y, 
            transform.position.z);
        _targetPosition = transform.position;
        _targetUiPosition = bookUi.transform.position;
    }

    private void Update()
    {
        HandleTouchInput();

        if (!_isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * snapSpeed);
            bookUi.transform.position = Vector3.Lerp(bookUi.transform.position, _targetUiPosition, Time.deltaTime * snapSpeed * bookMultiplier );
        }
    }

    private void HandleTouchInput()
    {
        if (DraggableItem.IsDraggingAnyItem || !_canDrag)
        {
            _isDragging = false;
            return;
        }
        

        Pointer currentPointer = Pointer.current;
        if (currentPointer == null) return;

        if (Vector2.Distance(transform.position, _targetPosition) > positionAccurancyThreshold && !_isDragging) return;

        if (currentPointer.press.wasPressedThisFrame)
        {
            if (IsPointerOverItem())
                return;

            _isDragging = true;
            _currentDragDirection = DragDirection.None;

            _touchStartScreenPos = currentPointer.position.ReadValue();
            _lastScreenPos = _touchStartScreenPos;
            _startCamPos = transform.position;
            if (bookUi != null)
            {
                _startBookPos = bookUi.transform.position;
            }
        }

        if (currentPointer.press.isPressed && _isDragging)
        {
            Vector2 currentScreenPos = currentPointer.position.ReadValue();
            Vector2 deltaPixels = currentScreenPos - _touchStartScreenPos;

            if (_currentDragDirection == DragDirection.None)
            {
                if (Mathf.Abs(deltaPixels.x) > dragThresholdPixels)
                {
                    _currentDragDirection = DragDirection.Horizontal;
                }
                else if (Mathf.Abs(deltaPixels.y) > dragThresholdPixels)
                {
                    _currentDragDirection = DragDirection.Vertical;
                }
            }

            float unitsPerPixel = (_cam.orthographicSize * 2f) / Screen.height;
            if (_currentDragDirection == DragDirection.Horizontal)
            {
                float deltaXWorld = deltaPixels.x * unitsPerPixel;

                transform.position = new Vector3(_startCamPos.x - deltaXWorld, _startCamPos.y, _startCamPos.z);
            }
            else if (_currentDragDirection == DragDirection.Vertical)
            {
                float deltaYWorld = deltaPixels.y * unitsPerPixel * bookMultiplier;
                if (bookUi != null)
                {
                    bookUi.transform.position = _startBookPos + new Vector3(0, deltaYWorld, 0);
                    bookUi.transform.position = new Vector3(bookUi.transform.position.x, 
                        Mathf.Clamp(bookUi.transform.position.y, minMaxBookHeight.x,minMaxBookHeight.y),
                        bookUi.transform.position.z);
                }
            }

            _touchVelocity = (currentScreenPos - _lastScreenPos) / Time.deltaTime;
            _lastScreenPos = currentScreenPos;
        }

        if (currentPointer.press.wasReleasedThisFrame && _isDragging)
        {
            _isDragging = false;
            SnapToSingleAxis();
            _currentDragDirection = DragDirection.None;
        }
    }

    private bool IsPointerOverItem()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Pointer.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<DraggableItem>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private void SnapToSingleAxis()
    {
        int startX = Mathf.RoundToInt(_startCamPos.x / pageSize.x);
        int startY = Mathf.RoundToInt(_startCamPos.y / pageSize.y);

        int targetX = startX;
        float targetY = startY;

        float deltaX = transform.position.x - _startCamPos.x;
        float deltaY = bookUi.transform.position.y - _startBookPos.y;

        if (_currentDragDirection == DragDirection.Horizontal)
        {
            if (Mathf.Abs(_touchVelocity.x) > velocityThreshold)
            {
                targetX += _touchVelocity.x < 0 ? 1 : -1;
            }
            else if (Mathf.Abs(deltaX) > pageSize.x * 0.35f)
            {
                targetX += deltaX > 0 ? 1 : -1;
            }
        }
        else if (_currentDragDirection == DragDirection.Vertical)
        {
            
            if (Mathf.Abs(_touchVelocity.y) > velocityThreshold)
            {
                targetY += _touchVelocity.y < 0 ? minMaxBookHeight.y : minMaxBookHeight.x;
            }
            else if (Mathf.Abs(deltaY) > minMaxBookHeight.y * 0.35f)
            {
                targetY += deltaY > 0 ? minMaxBookHeight.y :  minMaxBookHeight.x;
            }
            
        }

        if (mapData.GetRoom(targetX, startY).IsEmpty)
        {
            _targetPosition = new Vector3(startX * pageSize.x, startY * pageSize.y, transform.position.z);
            return;
        }

        _targetUiPosition = new Vector3(bookUi.transform.position.x, targetY , bookUi.transform.position.z);
        _targetPosition = new Vector3(targetX * pageSize.x, startY * pageSize.y, transform.position.z);
    }

    

    public void ChangeWorld()
    {
        Vector3 newPos;
        if (_isInOverWorld)
            newPos = new Vector3(mapData.spiritStartPosition.x * -pageSize.x, mapData.spiritStartPosition.y * pageSize.y,
                transform.position.z);
        else
            newPos = new Vector3(mapData.startPosition.x * -pageSize.x, mapData.startPosition.y * pageSize.y,
                transform.position.z);
        _targetPosition = newPos;
        transform.position = newPos;
        _lastScreenPos = newPos;
        _startCamPos = newPos;
        _isInOverWorld = !_isInOverWorld;
    }

    public void ChangeCanDragState(int newState)
    {
        _canDrag = newState != 0;
    }
}