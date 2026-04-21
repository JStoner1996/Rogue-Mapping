using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HoverPopupFollowerUI : MonoBehaviour
{
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 screenOffset = new Vector2(16f, -16f);
    [SerializeField] private bool clampToCanvas = true;
    [SerializeField] private bool autoFlipHorizontal = true;
    [SerializeField] private bool autoFlipVertical = true;

    private RectTransform canvasRect;
    private RectTransform popupParentRect;
    private Camera uiCamera;
    private bool isFollowing;

    void Awake()
    {
        if (popupRoot == null)
        {
            popupRoot = transform as RectTransform;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas != null)
        {
            canvasRect = canvas.transform as RectTransform;
            uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        popupParentRect = popupRoot != null ? popupRoot.parent as RectTransform : null;
    }

    void LateUpdate()
    {
        if (!isFollowing || popupRoot == null)
        {
            return;
        }

        RefreshPosition();
    }

    public void BeginFollowing()
    {
        isFollowing = true;
        RefreshPosition();
    }

    public void StopFollowing()
    {
        isFollowing = false;
    }

    public void RefreshPosition()
    {
        if (popupRoot == null)
        {
            return;
        }

        Vector2 mouseScreenPoint = GetMouseScreenPosition();

        RectTransform referenceRect = popupParentRect != null ? popupParentRect : canvasRect;
        if (referenceRect == null)
        {
            return;
        }

        Vector2 screenPoint = GetPreferredScreenPoint(mouseScreenPoint, referenceRect);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                referenceRect,
                screenPoint,
                uiCamera,
                out Vector2 localPoint))
        {
            return;
        }

        if (clampToCanvas)
        {
            localPoint = GetClampedLocalPoint(referenceRect, localPoint);
        }

        popupRoot.localPosition = new Vector3(localPoint.x, localPoint.y, popupRoot.localPosition.z);
    }

    private Vector2 GetPreferredScreenPoint(Vector2 mouseScreenPoint, RectTransform referenceRect)
    {
        Vector2 resolvedOffset = screenOffset;
        Vector2 size = popupRoot.rect.size;

        if (autoFlipHorizontal)
        {
            Vector2 rightCandidate = mouseScreenPoint + new Vector2(Mathf.Abs(screenOffset.x), resolvedOffset.y);
            Vector2 leftCandidate = mouseScreenPoint + new Vector2(-Mathf.Abs(screenOffset.x), resolvedOffset.y);

            if (WouldOverflowHorizontally(referenceRect, rightCandidate, size))
            {
                resolvedOffset.x = -Mathf.Abs(screenOffset.x);
            }
            else if (!WouldOverflowHorizontally(referenceRect, leftCandidate, size) && screenOffset.x < 0f)
            {
                resolvedOffset.x = -Mathf.Abs(screenOffset.x);
            }
            else
            {
                resolvedOffset.x = Mathf.Abs(screenOffset.x);
            }
        }

        if (autoFlipVertical)
        {
            Vector2 downCandidate = mouseScreenPoint + new Vector2(resolvedOffset.x, -Mathf.Abs(screenOffset.y));
            Vector2 upCandidate = mouseScreenPoint + new Vector2(resolvedOffset.x, Mathf.Abs(screenOffset.y));

            if (WouldOverflowVertically(referenceRect, downCandidate, size))
            {
                resolvedOffset.y = Mathf.Abs(screenOffset.y);
            }
            else if (!WouldOverflowVertically(referenceRect, upCandidate, size) && screenOffset.y > 0f)
            {
                resolvedOffset.y = Mathf.Abs(screenOffset.y);
            }
            else
            {
                resolvedOffset.y = -Mathf.Abs(screenOffset.y);
            }
        }

        return mouseScreenPoint + resolvedOffset;
    }

    private Vector2 GetClampedLocalPoint(RectTransform referenceRect, Vector2 localPoint)
    {
        Rect popupRect = popupRoot.rect;
        Vector2 size = popupRect.size;
        Vector2 pivot = popupRoot.pivot;
        Rect bounds = referenceRect.rect;

        float leftExtent = size.x * pivot.x;
        float rightExtent = size.x * (1f - pivot.x);
        float bottomExtent = size.y * pivot.y;
        float topExtent = size.y * (1f - pivot.y);

        float minX = bounds.xMin + leftExtent;
        float maxX = bounds.xMax - rightExtent;
        float minY = bounds.yMin + bottomExtent;
        float maxY = bounds.yMax - topExtent;

        localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
        localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);
        return localPoint;
    }

    private bool WouldOverflowHorizontally(RectTransform referenceRect, Vector2 candidateScreenPoint, Vector2 size)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(referenceRect, candidateScreenPoint, uiCamera, out Vector2 localPoint))
        {
            return false;
        }

        Rect bounds = referenceRect.rect;
        float leftEdge = localPoint.x - (size.x * popupRoot.pivot.x);
        float rightEdge = localPoint.x + (size.x * (1f - popupRoot.pivot.x));
        return leftEdge < bounds.xMin || rightEdge > bounds.xMax;
    }

    private bool WouldOverflowVertically(RectTransform referenceRect, Vector2 candidateScreenPoint, Vector2 size)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(referenceRect, candidateScreenPoint, uiCamera, out Vector2 localPoint))
        {
            return false;
        }

        Rect bounds = referenceRect.rect;
        float bottomEdge = localPoint.y - (size.y * popupRoot.pivot.y);
        float topEdge = localPoint.y + (size.y * (1f - popupRoot.pivot.y));
        return bottomEdge < bounds.yMin || topEdge > bounds.yMax;
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
}
