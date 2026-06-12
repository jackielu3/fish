using UnityEngine;

public class Oyster : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    [Header("Pearl")]
    [SerializeField] private FishData pearlFishData;
    [SerializeField] private Vector2 pearlSpawnOffset = new(0f, 0.35f);

    private Fish spawnedPearl;
    private bool hasOpened;

    public Fish SpawnedPearl => spawnedPearl;

    private void Awake()
    {
        Close();
    }

    public void TryOpen()
    {
        if (hasOpened) return;
        if (pearlFishData == null || pearlFishData.prefab == null) return;

        hasOpened = true;

        if (spriteRenderer != null)
            spriteRenderer.sprite = openSprite;

        Vector2 spawnPos = (Vector2)transform.position + pearlSpawnOffset;

        spawnedPearl = Instantiate(pearlFishData.prefab, spawnPos, Quaternion.identity);
        spawnedPearl.Initialize(pearlFishData, null);

        PearlOwner pearlOwner = spawnedPearl.gameObject.AddComponent<PearlOwner>();
        pearlOwner.Initialize(this);
    }

    public void Close()
    {
        hasOpened = false;

        if (spriteRenderer != null)
            spriteRenderer.sprite = closedSprite;

        if (spawnedPearl != null)
            Destroy(spawnedPearl.gameObject);

        spawnedPearl = null;
    }

    public void OnPearlCaught()
    {
        Destroy(gameObject);
    }
}