using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }  // 싱글톤 인스턴스

    [Header("게임 상태")]
    public bool isGameOver = false;
    public bool isGameCleared = false;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🎵 게임 시작 시 BGM 재생
    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM();
        }
    }

    // 게임 재시작 (씬 재로드)
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetGlobalScrollSpeed(float multiplier)
    {
        ScrollingObject[] scrolls = FindObjectsOfType<ScrollingObject>();
        foreach (var scroll in scrolls)
        {
            scroll.SetSpeedMultiplier(multiplier);
        }
        PlatformSpawner[] spawners = FindObjectsOfType<PlatformSpawner>();
        foreach (var spawner in spawners)
        {
            spawner.SetSpawnSpeedMultiplier(multiplier);
        }
    }

    public void ResetGlobalScrollSpeed()
    {
        ScrollingObject[] scrolls = FindObjectsOfType<ScrollingObject>();
        foreach (var scroll in scrolls)
        {
            scroll.ResetSpeed();
        }
        PlatformSpawner[] spawners = FindObjectsOfType<PlatformSpawner>();
        foreach (var spawner in spawners)
        {
            spawner.ResetSpawnSpeed();
        }
    }

    public void RestartGame()
    {
        Debug.Log("게임 재시작 - 씬 재로드");

        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null && uiManager.gameOverUI != null)
        {
            uiManager.gameOverUI.SetActive(false);
        }

        isGameOver = false;
        isGameCleared = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartGameWithoutReload()
    {
        Debug.Log("게임 재시작 - 씬 재로드 없이");

        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null && uiManager.gameOverUI != null)
        {
            uiManager.gameOverUI.SetActive(false);
        }

        isGameOver = false;
        isGameCleared = false;
        Time.timeScale = 1f;

        if (uiManager != null)
        {
            uiManager.RestartGame();
        }

        ResetPlayerPosition();
        ResetPlayerState();
        ResetGameTime();
        ResetGameObjects();
    }

    private void ResetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0, 0, 0);

            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }

            Debug.Log("플레이어 위치 초기화");
        }
    }

    private void ResetPlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ResetPlayerState();
                Debug.Log("플레이어 컨트롤러 상태 초기화 완료");
            }

            InvincibilityItem invincibilityItem = player.GetComponent<InvincibilityItem>();
            if (invincibilityItem != null)
            {
                invincibilityItem.ResetInvincibilityState();
                Debug.Log("무적 아이템 상태 초기화 완료");
            }

            Debug.Log("플레이어 상태 완전 초기화 완료");
        }
        else
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }
    }

    private void ResetGameTime()
    {
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ResetGameTime();
            Debug.Log("게임 시간 초기화 완료");
        }
        else
        {
            Debug.LogWarning("UIManager를 찾을 수 없어서 게임 시간을 초기화할 수 없습니다.");
        }
    }

    private void ResetGameObjects()
    {
        Debug.Log("게임 오브젝트들 초기화 시작");

        ItemSpawner itemSpawner = FindFirstObjectByType<ItemSpawner>();
        if (itemSpawner != null)
        {
            Debug.Log("ItemSpawner 발견 - 필요한 경우 초기화 로직 추가");
        }

        PlatformSpawner platformSpawner = FindFirstObjectByType<PlatformSpawner>();
        if (platformSpawner != null)
        {
            Debug.Log("PlatformSpawner 발견 - 초기화 실행");
        }

        GameObject[] activeItems = GameObject.FindGameObjectsWithTag("Pill");
        foreach (var item in activeItems)
        {
            if (item.activeSelf) item.SetActive(false);
        }

        GameObject[] activeWaterDrops = GameObject.FindGameObjectsWithTag("WaterDrop");
        foreach (var drop in activeWaterDrops)
        {
            if (drop.activeSelf) drop.SetActive(false);
        }

        GameObject[] activeSunLights = GameObject.FindGameObjectsWithTag("SunLight");
        foreach (var light in activeSunLights)
        {
            if (light.activeSelf) light.SetActive(false);
        }

        Debug.Log("게임 오브젝트들 초기화 완료");
    }

    // 🟥 게임 오버 처리: BGM 정지 + 게임 오버 효과음 재생
    public void EndGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        isGameCleared = false;
        Debug.Log("GameManager: 게임 오버 상태로 설정됨");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.PlayGameOver();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.GameOver();
        }
    }

    // 🟩 게임 클리어 처리: BGM 정지 + 게임 클리어 효과음 재생
    public void SetGameCleared()
    {
        isGameCleared = true;
        isGameOver = false;
        Debug.Log("GameManager: 게임 클리어 상태로 설정됨");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.PlayGameClear();
        }
    }

    public bool GameOver()
    {
        return isGameOver;
    }

    public bool IsGameCleared()
    {
        return isGameCleared;
    }

    public bool IsGameActive()
    {
        return !isGameOver && !isGameCleared;
    }
}
