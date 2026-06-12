using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum UpgradeType
{
    RopeLength,
    HookSpeed,
    InitialDiveLaunch,
    NetFragments
}

[System.Serializable]
public class UpgradeTier
{
    public string tierName;
    [TextArea] public string description;
    public float value;
    public float duration = 0.35f;
    public float cost;
}

[System.Serializable]
public class UpgradeData
{
    public UpgradeType upgradeType;
    public string upgradeDisplayName;
    public Sprite upgradeImage;
    public List<UpgradeTier> tiers = new();

    [ReadOnly] public int currentTierIndex = 0;

    public UpgradeTier CurrentTier => tiers[currentTierIndex];

    public bool IsMaxTier => currentTierIndex >= tiers.Count - 1;

    public UpgradeTier NextTier => IsMaxTier ? null : tiers[currentTierIndex + 1];
}

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private List<UpgradeData> upgrades = new();

    public UpgradeData GetUpgrade(UpgradeType type)
    {
        return upgrades.Find(upgrade => upgrade.upgradeType == type);
    }

    public float GetUpgradeValue(UpgradeType type)
    {
        UpgradeData upgrade = GetUpgrade(type);

        if (upgrade == null || upgrade.tiers.Count == 0)
            return 0f;

        return upgrade.CurrentTier.value;
    }

    public bool TryBuyUpgrade(UpgradeType type, MoneyManager moneyManager)
    {
        UpgradeData upgrade = GetUpgrade(type);

        if (upgrade == null) return false;
        if (upgrade.IsMaxTier) return false;

        UpgradeTier nextTier = upgrade.NextTier;

        if (!moneyManager.TrySpendMoneyWithoutAffectingStats(nextTier.cost))
            return false;

        upgrade.currentTierIndex++;
        return true;
    }
}