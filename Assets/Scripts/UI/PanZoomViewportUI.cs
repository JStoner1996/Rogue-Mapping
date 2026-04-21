using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Reusable UI viewport that pans and zooms a larger content rect inside a fixed window.
public class PanZoomViewportUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform viewportRect;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private Canvas canvas;

    [Header("Pan")]
    [SerializeField] private bool allowLeftMouseDrag = true;
    [SerializeField] private bool requireEmptySpaceForDrag = true;

    [Header("Zoom")]
    [SerializeField] private bool allowScrollWheelZoom = true;
    [SerializeField, Min(0.01f)] private float zoomStep = 0.1f;
    [SerializeField, Min(0.1f)] private float minZoom = 0.6f;
    [SerializeField, Min(0.1f)] private float maxZoom = 1.5f;
    [SerializeField] private float defaultZoom = 1f;
    [SerializeField] private bool zoomTowardPointer = true;

    [Header("Clamping")]
    [SerializeField] private bool clampToViewport = true;
    [SerializeField] private bool centerWhenSmallerThanViewport = true;

    private Camera uiCamera;
    private bool isDragging;
    private Vector2 previousMouseScreenPosition;
    private Vector2 initialAnchoredPosition;

    void Awake()
    {
        if (viewportRect == null)
        {
            viewportRect = transform as RectTransform;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas != null)
        {
            uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        if (contentRect != null)
        {
            initialAnchoredPosition = contentRect.anchoredPosition;
            SetZoom(defaultZoom);
            ClampContentToViewport();
        }
    }

    void Update()
    {
        if (viewportRect == null || contentRect == null)
        {
            return;
        }

        Vector2 mouseScreenPosition = GetMouseScreenPosition();

        HandleZoom(mouseScreenPosition);
        HandleDrag(mouseScreenPosition);
    }

    public void ResetView()
    {
        if (contentRect == null)
        {
            return;
        }

        contentRect.localScale = Vector3.one * Mathf.Clamp(defaultZoom, minZoom, maxZoom);
        contentRect.anchoredPosition = initialAnchoredPosition;
        ClampContentToViewport();
    }

    public void SetZoom(float zoom)
    {
        if (contentRect == null)
        {
            return;
        }

        float clampedZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        contentRect.localScale = Vector3.one * clampedZoom;
    }

    private void HandleZoom(Vector2 mouseScreenPosition)
    {
        if (!allowScrollWheelZoom || !IsPointerInsideViewport(mouseScreenPosition))
        {
            return;
        }

        float scrollDelta = GetScrollDelta();
        if (Mathf.Approximately(scrollDelta, 0f))
        {
            return;
        }

        float currentZoom = contentRect.localScale.x;
        float targetZoom = Mathf.Clamp(
            currentZoom + (Mathf.Sign(scrollDelta) * zoomStep * currentZoom),
            minZoom,
            maxZoom);

        if (Mathf.Approximately(currentZoom, targetZoom))
        {
            return;
        }

        if (zoomTowardPointer && RectTransformUtility.ScreenPointToWorldPointInRectangle(viewportRect, mouseScreenPosition, uiCamera, out Vector3 pointerWorldBefore))
        {
            Vector3 localPointBefore = contentRect.InverseTransformPoint(pointerWorldBefore);
            contentRect.localScale = Vector3.one * targetZoom;
            Vector3 pointerWorldAfter = contentRect.TransformPoint(localPointBefore);
            contentRect.position += pointerWorldBefore - pointerWorldAfter;
        }
        else
        {
            contentRect.localScale = Vector3.one * targetZoom;
        }

        ClampContentToViewport();
    }

    private void HandleDrag(Vector2 mouseScreenPosition)
    {
        if (!allowLeftMouseDrag)
        {
            return;
        }

        if (GetLeftMouseButtonDown())
        {
            isDragging = IsPointerInsideViewport(mouseScreenPosition) && CanStartDrag(mouseScreenPosition);
            previousMouseScreenPosition = mouseScreenPosition;
        }

        if (!GetLeftMouseButtonHeld())
        {
            isDragging = false;
            return;
        }

        if (!isDragging)
        {
            return;
        }

        Vector2 screenDelta = mouseScreenPosition - previousMouseScreenPosition;
        previousMouseScreenPosition = mouseScreenPosition;

        float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
        contentRect.anchoredPosition += screenDelta / Mathf.Max(0.0001f, canvasScale);
        ClampContentToViewport();
    }

    private bool CanStartDrag(Vector2 mouseScreenPosition)
    {
        if (!requireEmptySpaceForDrag || EventSystem.current == null)
        {
            return true;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mouseScreenPosition,
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        for (int i = 0; i < results.Count; i++)
        {
            GameObject hitObject = results[i].gameObject;
            if (hitObject == null)
            {
                continue;
            }

            if (hitObject == gameObject || hitObject.transform.IsChildOf(transform))
            {
                Selectable selectable = hitObject.GetComponentInParent<Selectable>();
                if (selectable != null && selectable.gameObject != gameObject)
                {
                    return false;
                }

                if (hitObject.GetComponentInParent<IPointerClickHandler>() != null && hitObject != gameObject)
                {
                    return false;
                }

                return true;
            }
        }

        return true;
    }

    private void ClampContentToViewport()
    {
        if (!clampToViewport || viewportRect == null || contentRect == null)
        {
            return;
        }

        Vector2 viewportSize = viewportRect.rect.size;
        Vector2 scaledContentSize = Vector2.Scale(contentRect.rect.size, contentRect.localScale);
        Vector2 anchoredPosition = contentRect.anchoredPosition;

        anchoredPosition.x = GetClampedAxisPosition(
            anchoredPosition.x,
            scaledContentSize.x,
            viewportSize.x);
        anchoredPosition.y = GetClampedAxisPosition(
            anchoredPosition.y,
            scaledContentSize.y,
            viewportSize.y);

        contentRect.anchoredPosition = anchoredPosition;
    }

    private float GetClampedAxisPosition(float currentPosition, float contentSize, float viewportSize)
    {
        if (contentSize <= viewportSize)
        {
            return centerWhenSmallerThanViewport ? 0f : currentPosition;
        }

        float maxOffset = (contentSize - viewportSize) * 0.5f;
        return Mathf.Clamp(currentPosition, -maxOffset, maxOffset);
    }

    private bool IsPointerInsideViewport(Vector2 mouseScreenPosition)
    {
        return viewportRect != null
            && RectTransformUtility.RectangleContainsScreenPoint(viewportRect, mouseScreenPosition, uiCamera);
    }

    private Vector2 GetMouseScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif
        return Input.mousePosition;
    }

    private float GetScrollDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.scroll.ReadValue().y;
        }
#endif
        return Input.mouseScrollDelta.y;
    }

    private bool GetLeftMouseButtonDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
        return Input.GetMouseButtonDown(0);
    }

    private bool GetLeftMouseButtonHeld()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.isPressed;
        }
#endif
        return Input.GetMouseButton(0);
    }
}
