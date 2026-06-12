using UnityEngine;

public class PearlOwner : MonoBehaviour
{
    private Oyster oyster;

    public void Initialize(Oyster newOyster)
    {
        oyster = newOyster;
    }

    public void NotifyCaught()
    {
        if (oyster != null)
            oyster.OnPearlCaught();
    }
}