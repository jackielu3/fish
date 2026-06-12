using TMPro;
using UnityEngine;

public class PlayerBoatVisual : MonoBehaviour
{
    [SerializeField] private Transform hookSpawn;
    [SerializeField] private TMP_Text boatNameText;

    public Transform HookSpawn => hookSpawn;

    public void SetBoatName(string boatName)
    {
        // TODO
    }
}