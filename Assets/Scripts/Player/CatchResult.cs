using System.Collections.Generic;

public enum CatchRewardType
{
    Fish,
    SmallRock,
    BoatIncome,
    LoanPayment
}

public class CatchReward
{
    public FishData data;
    public CatchRewardType rewardType;
    public int quantity;
    public float amount;
    public string displayName;
}

public class CatchResult
{
    public readonly List<CatchReward> rewards = new();
    public float totalMoney;

    public bool HasCatchRewards
    {
        get
        {
            foreach (CatchReward reward in rewards)
            {
                if (reward.rewardType == CatchRewardType.Fish ||
                    reward.rewardType == CatchRewardType.SmallRock)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public void AddFish(Fish fish)
    {
        if (fish == null || fish.Data == null) return;

        AddReward(
            fish.Data,
            CatchRewardType.Fish,
            fish.Data.fishName,
            fish.Data.currentValue
        );
    }

    public void AddSmallRock(SmallRock rock)
    {
        if (rock == null || rock.Data == null) return;

        AddReward(
            rock.Data,
            CatchRewardType.SmallRock,
            rock.Data.fishName,
            rock.Data.currentValue
        );
    }

    public void AddBoatIncome(float amount)
    {
        if (amount <= 0f) return;

        rewards.Add(new CatchReward
        {
            rewardType = CatchRewardType.BoatIncome,
            displayName = "Boat Income",
            quantity = 1,
            amount = amount
        });

        totalMoney += amount;
    }

    private void AddReward(FishData data, CatchRewardType type, string displayName, float amount)
    {
        CatchReward existing = rewards.Find(reward =>
            reward.data == data &&
            reward.rewardType == type
        );

        if (existing == null)
        {
            existing = new CatchReward
            {
                data = data,
                rewardType = type,
                displayName = displayName,
                quantity = 0,
                amount = 0f
            };

            rewards.Add(existing);
        }

        existing.quantity++;
        existing.amount += amount;
        totalMoney += amount;
    }

    public float PositiveTotal
    {
        get
        {
            float total = 0f;

            foreach (CatchReward reward in rewards)
            {
                if (reward.amount > 0f)
                    total += reward.amount;
            }

            return total;
        }
    }

    public void AddLoanPayment(float amount)
    {
        if (amount <= 0f) return;

        rewards.Add(new CatchReward
        {
            rewardType = CatchRewardType.LoanPayment,
            displayName = "Loan Cut",
            quantity = 1,
            amount = -amount
        });

        totalMoney -= amount;
    }
}