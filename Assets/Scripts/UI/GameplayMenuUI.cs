using UnityEngine;

public class GameplayMenyUI : MonoBehaviour
{
    [SerializeField] private GameObject boatModeUI;

    private void OnEnable()
    {
        boatModeUI.SetActive(false);
    }

    private void OnDisable()
    {
        boatModeUI.SetActive(true);
    }
}   