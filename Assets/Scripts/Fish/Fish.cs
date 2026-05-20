using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Fish : MonoBehaviour
{
    public FishData Data { get; private set; }

    public void Initialize(FishData data) 
    {
        Data = data;
    }
}
