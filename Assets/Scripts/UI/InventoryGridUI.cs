using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InventoryGridUI : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int maxSlots = 20;

    private readonly List<InventorySlotUI> spawnedSlots = new List<InventorySlotUI>();

    public int MaxSlots => maxSlots;

    void Awake()
    {
        RebuildSlots();
    }

    void OnEnable()
    {
        if (spawnedSlots.Count == 0)
        {
            RebuildSlots();
        }
    }

    public void SetItems(
        InventoryGridModel model,
        InventoryGridInteractions interactions = null)
    {
        EnsureSlotCount();
        interactions ??= InventoryGridInteractions.Empty();

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            int localIndex = i;
            InventorySlotModel slotModel = model != null && i < model.SlotCount
                ? model.GetSlot(i)
                : InventorySlotModel.Empty();

            spawnedSlots[i].Bind(
                slotModel,
                (_, data) => interactions.OnSlotClicked?.Invoke(localIndex, data),
                (_, data) => interactions.OnSlotRightClicked?.Invoke(localIndex, data),
                (_, data) => interactions.OnSlotHoverEnter?.Invoke(localIndex, data),
                (_, data) => interactions.OnSlotHoverExit?.Invoke(localIndex, data),
                (_, payload) => interactions.CanAcceptDropAtIndex != null && interactions.CanAcceptDropAtIndex.Invoke(localIndex, payload),
                (_, payload) => interactions.OnSlotDropReceived?.Invoke(localIndex, payload));
        }
    }

    public void ClearItems()
    {
        EnsureSlotCount();

        foreach (InventorySlotUI slot in spawnedSlots)
        {
            slot.SetEmpty();
        }
    }

    [ContextMenu("Rebuild Slots")]
    public void RebuildSlots()
    {
        if (slotPrefab == null || slotParent == null)
        {
            return;
        }

        CacheExistingSlots();
        ClearSpawnedSlots();

        int slotCount = Mathf.Max(0, maxSlots);

        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI slot = InstantiateSlot();
            slot.SetEmpty();
            spawnedSlots.Add(slot);
        }
    }

    private void EnsureSlotCount()
    {
        CacheExistingSlots();

        if (spawnedSlots.Count != Mathf.Max(0, maxSlots))
        {
            RebuildSlots();
        }
    }

    private InventorySlotUI InstantiateSlot()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return PrefabUtility.InstantiatePrefab(slotPrefab, slotParent) as InventorySlotUI;
        }
#endif

        return Instantiate(slotPrefab, slotParent);
    }

    private void ClearSpawnedSlots()
    {
        for (int i = spawnedSlots.Count - 1; i >= 0; i--)
        {
            if (spawnedSlots[i] == null)
            {
                continue;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(spawnedSlots[i].gameObject);
                continue;
            }
#endif

            Destroy(spawnedSlots[i].gameObject);
        }

        spawnedSlots.Clear();
    }

    private void CacheExistingSlots()
    {
        if (slotParent == null)
        {
            return;
        }

        spawnedSlots.Clear();

        foreach (Transform child in slotParent)
        {
            if (child.TryGetComponent(out InventorySlotUI slot))
            {
                spawnedSlots.Add(slot);
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        maxSlots = Mathf.Max(0, maxSlots);
    }
#endif
}
