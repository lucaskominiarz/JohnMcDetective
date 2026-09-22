using UnityEngine;
using UnityEngine.InputSystem;

public class ScrollView : MonoBehaviour
{
    [SerializeField] private Vector2 pageSize = new Vector2(4.5f, 10f);
    [SerializeField] private float snapSpeed = 12f;
    [SerializeField] private float velocityThreshold = 200f;
    [SerializeField] private float dragThresholdPixels = 15f;
    [SerializeField] private Vector2 minPos; // a changer quand j'aurais fait le tool
    [SerializeField] private Vector2 maxPos;// pareil

    private Camera cam;
    private Vector3 targetPosition;
    private Vector2 touchStartScreenPos;
    private Vector2 lastScreenPos;
    private Vector2 touchVelocity;
    private bool isDragging = false;

    private Vector3 startCamPos;

    private enum DragDirection { None, Horizontal, Vertical }
    private DragDirection currentDragDirection = DragDirection.None;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
    }

    private void Update()
    {
        HandleTouchInput();

        if (!isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * snapSpeed);
        }
    }

    private void HandleTouchInput()
    {
        Pointer currentPointer = Pointer.current;
        if (currentPointer == null) return;
        if (currentPointer.press.wasPressedThisFrame)
        {
            isDragging = true;
            currentDragDirection = DragDirection.None;

            touchStartScreenPos = currentPointer.position.ReadValue();
            lastScreenPos = touchStartScreenPos;
            startCamPos = transform.position;
        }

        if (currentPointer.press.isPressed && isDragging)
        {
            Vector2 currentScreenPos = currentPointer.position.ReadValue();
            Vector2 deltaPixels = currentScreenPos - touchStartScreenPos;
            if (currentDragDirection == DragDirection.None)
            {
                if (Mathf.Abs(deltaPixels.x) > dragThresholdPixels)
                {
                    currentDragDirection = DragDirection.Horizontal;
                }
                else if (Mathf.Abs(deltaPixels.y) > dragThresholdPixels)
                {
                    currentDragDirection = DragDirection.Vertical;
                }
            }


            float unitsPerPixel = (cam.orthographicSize * 2f) / Screen.height;
            if (currentDragDirection == DragDirection.Horizontal)
            {
                float deltaXWorld = deltaPixels.x * unitsPerPixel;
                transform.position = new Vector3(startCamPos.x - deltaXWorld, startCamPos.y, startCamPos.z);
            }
            else if (currentDragDirection == DragDirection.Vertical)
            {
                float deltaYWorld = deltaPixels.y * unitsPerPixel;
                transform.position = new Vector3(startCamPos.x, startCamPos.y - deltaYWorld, startCamPos.z);
            }

            touchVelocity = (currentScreenPos - lastScreenPos) / Time.deltaTime;
            lastScreenPos = currentScreenPos;
        }

        if (currentPointer.press.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            SnapToSingleAxis();
            currentDragDirection = DragDirection.None;
        }
    }

    private void SnapToSingleAxis()
    {
        int startX = Mathf.RoundToInt(startCamPos.x / pageSize.x);
        int startY = Mathf.RoundToInt(startCamPos.y / pageSize.y);

        int targetX = startX;
        int targetY = startY;

        float deltaX = transform.position.x - startCamPos.x;
        float deltaY = transform.position.y - startCamPos.y;

        if (currentDragDirection == DragDirection.Horizontal)
        {
            if (Mathf.Abs(touchVelocity.x) > velocityThreshold)
            {
                targetX += touchVelocity.x < 0 ? 1 : -1;
            }
            else if (Mathf.Abs(deltaX) > pageSize.x * 0.35f)
            {
                targetX += deltaX > 0 ? 1 : -1;
            }
        }
        else if (currentDragDirection == DragDirection.Vertical)
        {
            if (Mathf.Abs(touchVelocity.y) > velocityThreshold)
            {
                targetY += touchVelocity.y < 0 ? 1 : -1;
            }
            else if (Mathf.Abs(deltaY) > pageSize.y * 0.35f)
            {
                targetY += deltaY > 0 ? 1 : -1;
            }
        }

        targetPosition = new Vector3(Mathf.Clamp(targetX * pageSize.x, minPos.x, maxPos.x), Mathf.Clamp(targetY * pageSize.y,minPos.y, maxPos.y), transform.position.z);
    }
}