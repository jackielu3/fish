using UnityEngine;

[RequireComponent(typeof(Fish))]
public class SmallFish : MonoBehaviour
{
    private Fish fish;

    private void Awake()
    {
        fish = GetComponent<Fish>();
    }

    private void Update()
    {
        
    }
}