using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish/Fish Data")]
public class FishData : ScriptableObject
{
    public string fishName;
    public Color fishColor;

    public Fish prefab;

    public Sprite sprite;
    public Sprite closeup;

    [Header("Economy")]
    public float baseValue;
    [ReadOnly] public float currentValue;

    public float moveSpeed;

    public int numberCaught;
    [TextArea] public string description;
}