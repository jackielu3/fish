using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish/Fish Data")]
public class FishData : ScriptableObject
{
    public string fishName;

    public Sprite sprite;
    public Sprite closeup;
    
    public float value;
    public float moveSpeed;

    public int numberCaught;
    [TextArea] public string description;
}