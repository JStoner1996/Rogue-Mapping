using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragDropManagerUI : MonoBehaviour
{
    public static DragDropManagerUI Instance { get; private set; }

    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 ghostOffset = new Vector2(20f, -20f);
    [SerializeField] private Color ghostTint = new Color(1f, 1f, 1f, 0.85f);

    private RectTransform canvasRect;
    private Image ghostImage;
    private RectTransform ghostRect;
    private DragItemPayload currentPayload;
    private IDragDropTargetUI currentTarget;

    public bool IsDragging => currentPayload != null;
    public DragItemPayload CurrentPayload => currentPayload;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas != null)
        {
            canvasRect = canvas.transform as RectTransform;
        }
    }

    public void BeginDrag(DragItemPayload payload)
    {
        if (payload == null || !payload.IsValid())
        {
            return;
        }

        currentPayload = payload;
        EnsureGhost();
        RefreshGhost();
        UpdateGhostPosition();
    }

    public void UpdateDrag(PointerEventData eventData)
    {
        if (!IsDragging)
        {
            return;
        }

        UpdateGhostPosition(eventData);
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (!IsDragging)
        {
            return;
        }

        IDragDropTargetUI target = ResolveDropTarget(eventData);

        if (target != null && target.CanAcceptDrop(currentPayload))
        {
            target.OnDropReceived(currentPayload);
        }

        ClearCurrentTarget();
        currentPayload = null;

        if (ghostImage != null)
        {
            ghostImage.enabled = false;
        }
    }

    public void EvaluateHover(PointerEventData eventData)
    {
        if (!IsDragging)
        {
            return;
        }

        IDragDropTargetUI nextTarget = ResolveDropTarget(eventData);

        if (ReferenceEquals(nextTarget, currentTarget))
        {
            return;
        }

        ClearCurrentTarget();
        currentTarget = nextTarget;

        if (currentTarget != null && currentTarget.CanAcceptDrop(currentPayload))
        {
            currentTarget.OnDragHoverStart(currentPayload);
        }
    }

    private void ClearCurrentTarget()
    {
        if (currentTarget != null && currentPayload != null)
        {
            currentTarget.OnDragHoverEnd(currentPayload);
        }

        currentTarget = null;
    }

    private IDragDropTargetUI ResolveDropTarget(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return null;
        }

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        for (int i = 0; i < raycastResults.Count; i++)
        {
            GameObject hitObject = raycastResults[i].gameObject;

            if (hitObject == null)
            {
                continue;
            }

            IDragDropTargetUI target = hitObject.GetComponentInParent<IDragDropTargetUI>();

            if (target != null)
            {
                return target;
            }
        }

        if (eventData.pointerEnter == null)
        {
            return null;
        }

        return eventData.pointerEnter.GetComponentInParent<IDragDropTargetUI>();
    }

    private void EnsureGhost()
    {
        if (ghostImage != null)
        {
            return;
        }

        if (canvasRect == null)
        {
            return;
        }

        GameObject ghostObject = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ghostObject.transform.SetParent(canvasRect, false);

        ghostRect = ghostObject.transform as RectTransform;
        ghostImage = ghostObject.GetComponent<Image>();
        ghostImage.raycastTarget = false;
        ghostImage.color = ghostTint;

        ghostRect.anchorMin = new Vector2(0f, 1f);
        ghostRect.anchorMax = new Vector2(0f, 1f);
        ghostRect.pivot = new Vector2(0.5f, 0.5f);
        ghostRect.sizeDelta = new Vector2(80f, 80f);
    }

    private void RefreshGhost()
    {
        if (ghostImage == null)
        {
            return;
        }

        ghostImage.sprite = currentPayload != null ? currentPayload.icon : null;
        ghostImage.enabled = currentPayload != null && currentPayload.icon != null;
    }

    private void UpdateGhostPosition(PointerEventData eventData = null)
    {
        if (ghostRect == null || canvasRect == null)
        {
            return;
        }

        Vector2 screenPosition = eventData != null ? eventData.position : GetMousePosition();
        screenPosition += ghostOffset;

        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, uiCamera, out Vector2 localPoint))
        {
            ghostRect.localPosition = new Vector3(localPoint.x, localPoint.y, ghostRect.localPosition.z);
        }
    }

    private Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            return UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }
#endif
        return Input.mousePosition;
    }
}
