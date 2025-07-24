using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("플랫폼 프리팹")]
    public GameObject longPlatformPrefab;
    public GameObject shortPlatformPrefab;
    [Range(0f, 1f)]
    public float longPlatformChance = 0.5f;

    public int count = 6;

    [Header("스폰 간격")]
    public float timeBetSpawnMin = 1.25f;
    public float timeBetSpawnMax = 2.5f;

    private float timeBetSpawn;
    private float lastSpawnTime;

    private float originalMin;
    private float originalMax;

    public float yMin = -5.0f;
    public float yMax = -1.0f;
    private float xPos = 20f;

    private GameObject[] platforms;
    private int currentIndex = 0;
    private Vector2 poolPosition = new Vector2(0, 25f);

    private void Start()
    {
        platforms = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            GameObject selectedPrefab = (Random.value < longPlatformChance) ? longPlatformPrefab : shortPlatformPrefab;
            platforms[i] = Instantiate(selectedPrefab, poolPosition, Quaternion.identity);
        }

        // 원래 간격 저장
        originalMin = timeBetSpawnMin;
        originalMax = timeBetSpawnMax;

        lastSpawnTime = 0f;
        timeBetSpawn = 0f;
    }

    private void Update()
    {
        if (Time.time >= lastSpawnTime + timeBetSpawn)
        {
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);

            float yPos = Random.Range(yMin, yMax);
            int platformIndex = FindInactivePlatform();

            if (platformIndex != -1)
            {
                platforms[platformIndex].SetActive(true);
                platforms[platformIndex].transform.position = new Vector2(xPos, yPos);
            }
            else
            {
                platforms[currentIndex].SetActive(false);
                platforms[currentIndex].SetActive(true);
                platforms[currentIndex].transform.position = new Vector2(xPos, yPos);
                currentIndex = (currentIndex + 1) % count;
            }

            int activeIndex = platformIndex != -1 ? platformIndex : currentIndex > 0 ? currentIndex - 1 : count - 1;
            if (ItemSpawner.Instance != null)
            {
                Vector2 platformPos = platforms[activeIndex].transform.position;
                Collider2D platformCol = platforms[activeIndex].GetComponent<Collider2D>();
                Vector2 platformSize = platformCol ? platformCol.bounds.size : Vector2.one;

                ItemSpawner.Instance.SpawnItemsOnPlatform(platformPos, platformSize, platforms[activeIndex]);
            }
        }
    }

    private int FindInactivePlatform()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            if (!platforms[i].activeSelf)
                return i;
        }
        return -1;
    }

    // 속도 증가 시 호출되는 메서드
    public void SetSpawnSpeedMultiplier(float multiplier)
{
    multiplier = Mathf.Clamp(multiplier, 0.1f, 10f);

    float elapsed = Time.time - lastSpawnTime;
    float percent = timeBetSpawn > 0 ? elapsed / timeBetSpawn : 0f;

    timeBetSpawnMin = Mathf.Max(0.2f, originalMin / multiplier);
    timeBetSpawnMax = Mathf.Max(0.3f, originalMax / multiplier);
    timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);

    lastSpawnTime = Time.time - (timeBetSpawn * percent); // 이전 진행도 유지
}


    // 속도 원상 복구
    public void ResetSpawnSpeed()
{
    float elapsed = Time.time - lastSpawnTime;
    float percent = timeBetSpawn > 0 ? elapsed / timeBetSpawn : 0f;

    timeBetSpawnMin = originalMin;
    timeBetSpawnMax = originalMax;
    timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);

    lastSpawnTime = Time.time - (timeBetSpawn * percent); // 진행률 유지
}
}
