using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public GameObject platformPrefab;
    public int count = 2;

    public float timeMin = 1.25f, timeMax = 2.5f;
    public float yMin = -3.5f, yMax = 1.5f;
    public float xPos = 20f;

    private GameObject[] platforms;
    private int current = 0;
    private float lastTime = 0f, interval = 0f;

    void Start()
    {
        platforms = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            platforms[i] = Instantiate(platformPrefab, Vector2.up * 100f, Quaternion.identity);
            platforms[i].SetActive(false);
        }
    }

    void Update()
    {
        if (Time.time < lastTime + interval) return;

        lastTime = Time.time;
        interval = Random.Range(timeMin, timeMax);

        float y = Random.Range(yMin, yMax);
        var plat = platforms[current];
        plat.SetActive(false);
        plat.transform.position = new Vector2(xPos, y);
        plat.SetActive(true);  // 여기서 Platform.OnEnable이 불려 장애물 스폰

        current = (current + 1) % count;

        // 아이템 배치는 ItemSpawner에 위임
        var col = plat.GetComponent<Collider2D>();
        if (col != null && ItemSpawner.Instance != null)
        {
            ItemSpawner.Instance.SpawnItemsOnPlatform(plat);
        }
    }
}
