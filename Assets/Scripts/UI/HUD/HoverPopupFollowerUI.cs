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

        Vector2 screenPoint = GetMouseScreenPosition() + screenOffset;

        RectTransform referenceRect = popupParentRect != null ? popupParentRect : canvasRect;
        if (referenceRect == null)
        {
            return;
        }

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
            RectTransform clampRect = referenceRect;
            Vector2 size = popupRoot.rect.size;
            float minX = clampRect.rect.xMin;
            float maxX = clampRect.rect.xMax - size.x;
            float minY = clampRect.rect.yMin + size.y;
            float maxY = clampRect.rect.yMax;

            localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
            localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);
        }

        popupRoot.localPosition = new Vector3(localPoint.x, localPoint.y, popupRoot.localPosition.z);
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
