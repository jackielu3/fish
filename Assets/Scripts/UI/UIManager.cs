using UnityEngine;

public class UIManager : MonoBehaviour
{
    // THIS WAS ALWAYS MEANT TO BE A TEMPORARY FILE, BUT IT HAS STUCK SINCE IT WORKS

    public static UIManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CatchResultsUI catchResultsUI;

    public CatchResultsUI CatchResultsUI => catchResultsUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}