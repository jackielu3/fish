using System;
using TMPro;
using UnityEngine;

public class BoatNameText : MonoBehaviour
{
    [SerializeField] private TextMeshPro boatNameText;

    public void NameChange(String name)
    {
        boatNameText.text = name;
    }
}
