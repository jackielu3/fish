using System.Collections.Generic;
using UnityEngine;

public class HookBaitDropper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject baitPrefab;
    [SerializeField] private BaitChargeVisual chargeVisual;

    private BaitInventoryManager baitInventoryManager;

    [Header("Drop Settings")]
    [SerializeField] private float holdDuration = 0.35f;
    [SerializeField] private int maxBaitDropsPerDive = 3;

    private float currentInputY;

    private int baitDroppedThisDive;
    private float holdTimer;
    private bool hasDroppedForCurrentHold;

    private readonly List<BaitObject> baitDroppedByThisHook = new();

    public void Initialize(BaitInventoryManager manager)
    {
        baitInventoryManager = manager;
    }

    private void Update()
    {
        UpdateBaitHold();
    }

    public void SetBaitInput(float inputY)
    {
        currentInputY = inputY;
    }

    private void UpdateBaitHold()
    {
        if (baitDroppedThisDive >= 3) return;

        bool isHoldingUp = currentInputY > 0.5f;

        if (!isHoldingUp)
        {
            holdTimer = 0f;
            hasDroppedForCurrentHold = false;

            if (chargeVisual != null)
                chargeVisual.Hide();

            return;
        }

        if (hasDroppedForCurrentHold)
        {
            if (chargeVisual != null)
                chargeVisual.Hide();
            return;
        }

        holdTimer += Time.deltaTime;

        float chargePercent = holdTimer / holdDuration;

        if (chargeVisual != null)
            chargeVisual.SetCharge(chargePercent);

        if (holdTimer >= holdDuration)
        {
            TryDropBait();

            if (chargeVisual != null)
                chargeVisual.Hide();

            hasDroppedForCurrentHold = true;
        }
    }

    private void TryDropBait()
    {
        if (baitInventoryManager == null) return;
        if (baitDroppedThisDive >= maxBaitDropsPerDive) return;
        if (!baitInventoryManager.TryUseBait()) return;
        if (chargeVisual != null)
            chargeVisual.Hide();

        GameObject baitObj = Instantiate(baitPrefab, transform.position, Quaternion.identity);

        if (baitObj.TryGetComponent(out BaitObject bait))
        {
            baitDroppedByThisHook.Add(bait);
            BaitLifetimeManager.Instance.RegisterBait(bait);
        }

        baitDroppedThisDive++;
    }

    public List<BaitObject> GetDroppedBait()
    {
        return baitDroppedByThisHook;
    }
}