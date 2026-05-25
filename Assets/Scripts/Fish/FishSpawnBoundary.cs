using UnityEngine;

public class FishSpawnBoundary : MonoBehaviour
{
    public FishSpawner Owner { get; private set; }
    public Vector2 BounceNormal { get; private set; }

    public void Initialize(FishSpawner owner, Vector2 bounceNormal)
    {
        Owner = owner;
        BounceNormal = bounceNormal.normalized;
    }
}
