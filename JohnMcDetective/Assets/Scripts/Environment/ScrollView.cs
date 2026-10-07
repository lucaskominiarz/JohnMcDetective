using UnityEngine;
using UnityEngine.InputSystem;

public class ScrollView : MonoBehaviour
{
    [SerializeField] private Vector2 pageSize = new Vector2(4.5f, 10f);
    [SerializeField] private float snapSpeed = 15f;
    [SerializeField] private float velocityThreshold = 5f;
    [SerializeField] private float dragThresholdPixels = 20f;
    [SerializeField] private MapData mapData;
    [SerializeField] private float positionAccurancyThreshold = 0.2f;

    private Camera _cam;
    private Vector3 _targetPosition;
    private Vector2 _touchStartScreenPos;
    private Vector2 _lastScreenPos;
    private Vector2 _touchVelocity;
    private bool _isDragging = false;

    private Vector3 _startCamPos;

    private enum DragDirection { None, Horizontal, Vertical }
    private DragDirection _currentDragDirection = DragDirection.None;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        transform.position = new Vector3(mapData.startPosition.x * pageSize.x, 
            mapData.startPosition.y * pageSize.y, 
            transform.position.z);
        _targetPosition = transform.position;
    }

    private void Update()
    {
        HandleTouchInput();

        if (!_isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * snapSpeed);
        }
    }

    private void HandleTouchInput()
    {
        Pointer currentPointer = Pointer.current;
        if (currentPointer == null) return;
        if (Vector2.Distance(transform.position, _targetPosition) > positionAccurancyThreshold &&!_isDragging) return;
        if (currentPointer.press.wasPressedThisFrame)
        {
            _isDragging = true;
            _currentDragDirection = DragDirection.None;

            _touchStartScreenPos = currentPointer.position.ReadValue();
            _lastScreenPos = _touchStartScreenPos;
            _startCamPos = transform.position;
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
                float deltaYWorld = deltaPixels.y * unitsPerPixel;
                transform.position = new Vector3(_startCamPos.x, _startCamPos.y - deltaYWorld, _startCamPos.z);
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

    private void SnapToSingleAxis()
    {
        int startX = Mathf.RoundToInt(_startCamPos.x / pageSize.x);
        int startY = Mathf.RoundToInt(_startCamPos.y / pageSize.y);

        int targetX = startX;
        int targetY = startY;

        float deltaX = transform.position.x - _startCamPos.x;
        float deltaY = transform.position.y - _startCamPos.y;

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
                targetY += _touchVelocity.y < 0 ? 1 : -1;
            }
            else if (Mathf.Abs(deltaY) > pageSize.y * 0.35f)
            {
                targetY += deltaY > 0 ? 1 : -1;
            }
        }

        if (mapData.GetRoom(targetX, targetY).IsEmpty )
        {
            _targetPosition = new Vector3(startX * pageSize.x, startY * pageSize.y, transform.position.z);
            return;
        }

        _targetPosition = new Vector3(targetX * pageSize.x, targetY * pageSize.y, transform.position.z);
    }
}