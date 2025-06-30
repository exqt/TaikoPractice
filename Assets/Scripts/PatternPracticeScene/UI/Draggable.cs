using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class UIDraggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool clampToCanvasBounds = true;

    [Header("Drag Bounds")]
    public float minX = float.NegativeInfinity;
    public float maxX = float.PositiveInfinity;
    public float minY = float.NegativeInfinity;
    public float maxY = float.PositiveInfinity;

    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private bool isDragging = false;
    private Vector2 offset;
    private Camera uiCamera;
    private Canvas rootCanvas;

    private string PositionPrefsKey => $"UIDraggable_{gameObject.scene.name}_{gameObject.name}_pos";

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (transform.parent != null)
            parentRectTransform = transform.parent.GetComponent<RectTransform>();

        // Find root canvas for bounds
        rootCanvas = GetComponentInParent<Canvas>();

        // Restore saved position if exists
        LoadPosition();
    }

    private void Start()
    {
        // Find the UI camera
        if (Camera.main != null && Camera.main.GetComponent<Canvas>() != null)
            uiCamera = Camera.main;
        else
            uiCamera = Camera.current;
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 position = GetLocalPointInRectangle(mousePosition);
            position -= offset;
            rectTransform.localPosition = position;

            // Clamp to user-defined bounds (localPosition)
            Vector3 clamped = rectTransform.localPosition;
            clamped.x = Mathf.Clamp(clamped.x, minX, maxX);
            clamped.y = Mathf.Clamp(clamped.y, minY, maxY);
            rectTransform.localPosition = clamped;

            // Clamp to canvas bounds (anchoredPosition)
            if (clampToCanvasBounds && rootCanvas != null && rootCanvas.renderMode != RenderMode.WorldSpace)
            {
                RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
                Vector2 size = rectTransform.rect.size;
                Vector2 pivot = rectTransform.pivot;
                Vector2 min = canvasRect.rect.min + size * pivot;
                Vector2 max = canvasRect.rect.max - size * (Vector2.one - pivot);
                Vector2 anchored = rectTransform.anchoredPosition;
                anchored.x = Mathf.Clamp(anchored.x, min.x, max.x);
                anchored.y = Mathf.Clamp(anchored.y, min.y, max.y);
                rectTransform.anchoredPosition = anchored;
            }
        }
    }

    private Vector2 GetLocalPointInRectangle(Vector2 screenPoint)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform != null ? parentRectTransform : rectTransform,
            screenPoint,
            uiCamera,
            out Vector2 localPoint))
        {
            return localPoint;
        }

        return rectTransform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        // Calculate the offset to prevent the element from jumping to the pointer
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform != null ? parentRectTransform : rectTransform,
            eventData.position,
            uiCamera,
            out offset);
        offset -= (Vector2)rectTransform.localPosition;
    }

    private void LoadPosition()
    {
        if (DraggablePositionManager.TryLoadPosition(PositionPrefsKey, out DraggablePosition position))
        {
            rectTransform.anchoredPosition = new Vector2(position.x, position.y);
        }
    }

    private void SavePosition()
    {
        Vector2 pos = rectTransform.anchoredPosition;
        DraggablePositionManager.SavePosition(PositionPrefsKey, new DraggablePosition(pos.x, pos.y));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        SavePosition();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}
