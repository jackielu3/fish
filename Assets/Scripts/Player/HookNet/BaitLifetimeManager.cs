using System.Collections.Generic;
using UnityEngine;

public class BaitLifetimeManager : MonoBehaviour
{
    public static BaitLifetimeManager Instance { get; private set; }

    private readonly List<BaitObject> activeBait = new();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterBait(BaitObject bait)
    {
        if (bait != null && !activeBait.Contains(bait))
            activeBait.Add(bait);
    }

    public void OnReturnedToBoat()
    {
        for (int i = activeBait.Count - 1; i >= 0; i--)
        {
            if (activeBait[i] == null)
            {
                activeBait.RemoveAt(i);
                continue;
            }

            activeBait[i].OnReturnedToBoat();
        }
    }
}