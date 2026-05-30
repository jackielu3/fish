

using System.Collections.Generic;

public class CatchResult
{
    public readonly Dictionary<FishData, int> fishCounts = new();
    public float totalMoney;

    public void AddFish(Fish fish)
    {
        if (fish == null || fish.Data == null) return;

        if (!fishCounts.ContainsKey(fish.Data))
        {
            fishCounts.Add(fish.Data, 0);
        }

        fishCounts[fish.Data]++;
        totalMoney += fish.Data.value;
    }
}