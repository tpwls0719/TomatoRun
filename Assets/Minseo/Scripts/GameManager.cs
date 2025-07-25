using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }  // 싱글톤 인스턴스

    [Header("게임 상태")]
    public bool isGameOver = false;
    public bool isGameCleared = false;

    [Header("카메라 쉐이크 효과음")]
    public AudioClip cameraShakeSound;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<AudioSource>();
    }
    else if (Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    }

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM();
            Debug.Log("GameManager: BGM 재생 시작");
        }
        else
        {
            Debug.LogWarning("GameManager: AudioManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    // 게임 재시작 (씬 재로드)
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /*public void SetGlobalScrollSpeed(float multiplier)
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
    }*/

    public void RestartGame()
    {
        Debug.Log("게임 재시작 - 씬 재로드");

        // 게임 오버 UI 먼저 끄기 (씬 재로드 전에) - 안전한 방식으로
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null && uiManager.gameOverUI != null)
        {
            uiManager.gameOverUI.SetActive(false);
        }

        // 게임 상태 초기화
        isGameOver = false;
        isGameCleared = false;

        // 시간 정지 해제
        Time.timeScale = 1f;

        // 현재 씬 재로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 게임 재시작 (씬 재로드 없이)
    public void RestartGameWithoutReload()
    {
        Debug.Log("게임 재시작 - 씬 재로드 없이");

        // 게임 오버 UI 먼저 끄기 - 안전한 방식으로
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null && uiManager.gameOverUI != null)
        {
            uiManager.gameOverUI.SetActive(false);
        }

        // 게임 상태 초기화
        isGameOver = false;
        isGameCleared = false;

        // 시간 정지 해제
        Time.timeScale = 1f;

        // UIManager를 통한 재시작 - 안전한 방식으로
        if (uiManager != null)
        {
            uiManager.RestartGame();
        }

        // 플레이어 위치 및 상태 초기화
        ResetPlayerPosition();
        ResetPlayerState();

        // 게임 시간 초기화 (UIManager의 게임 시간 리셋)
        ResetGameTime();

        // 게임 오브젝트들 초기화
        ResetGameObjects();
    }

    // 플레이어 위치 초기화
    private void ResetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.ResetPlayerPosition();
            }
        }
    }

    // 플레이어 상태 초기화
    private void ResetPlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 플레이어 컨트롤러 초기화
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ResetPlayerState(); // 새로운 초기화 메서드 호출
                Debug.Log("플레이어 컨트롤러 상태 초기화 완료");
            }

            // 무적 상태 초기화
            InvincibilityItem invincibilityItem = player.GetComponent<InvincibilityItem>();
            if (invincibilityItem != null)
            {
                invincibilityItem.ResetInvincibilityState(); // 새로운 초기화 메서드 호출
                Debug.Log("무적 아이템 상태 초기화 완료");
            }

            Debug.Log("플레이어 상태 완전 초기화 완료");
        }
        else
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }
    }

    // 게임 시간 초기화
    private void ResetGameTime()
    {
        // UIManager의 gameTime을 0으로 리셋 - 안전한 방식으로
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

    // 게임 오브젝트들 초기화
    private void ResetGameObjects()
    {
        Debug.Log("게임 오브젝트들 초기화 시작");

        // 아이템 스포너 초기화 (있는 경우)
        ItemSpawner itemSpawner = FindFirstObjectByType<ItemSpawner>();
        if (itemSpawner != null)
        {
            Debug.Log("ItemSpawner 발견 - 필요한 경우 초기화 로직 추가");
            // ItemSpawner에 초기화 메서드가 있다면 여기서 호출
            // itemSpawner.ResetSpawner();
        }

        // 플랫폼 스포너 초기화 (있는 경우)
        PlatformSpawner platformSpawner = FindFirstObjectByType<PlatformSpawner>();
        if (platformSpawner != null)
        {
            Debug.Log("PlatformSpawner 발견 - 초기화 실행");
            //platformSpawner.ResetPlatformSpawner(); // 초기화 메서드 호출
        }

        // 활성화된 모든 아이템들 비활성화 (풀링된 아이템들)
        GameObject[] activeItems = GameObject.FindGameObjectsWithTag("Pill");
        for (int i = 0; i < activeItems.Length; i++)
        {
            if (activeItems[i].activeSelf)
            {
                activeItems[i].SetActive(false);
            }
        }

        GameObject[] activeWaterDrops = GameObject.FindGameObjectsWithTag("Water");
        for (int i = 0; i < activeWaterDrops.Length; i++)
        {
            if (activeWaterDrops[i].activeSelf)
            {
                activeWaterDrops[i].SetActive(false);
            }
        }

        GameObject[] activeSunLights = GameObject.FindGameObjectsWithTag("Sunlight");
        for (int i = 0; i < activeSunLights.Length; i++)
        {
            if (activeSunLights[i].activeSelf)
            {
                activeSunLights[i].SetActive(false);
            }
        }
        Stage stageManager = FindFirstObjectByType<Stage>();
        if (stageManager != null)
        {
            //stageManager.ResetStage();
        }

        Debug.Log("게임 오브젝트들 초기화 완료");
    }

    // 게임 오버 상태 설정
    public void EndGame()
    {
        // 이미 게임 오버 상태면 중복 처리 방지
        if (isGameOver) return;

        isGameOver = true;
        isGameCleared = false;
        Debug.Log("GameManager: 게임 오버 상태로 설정됨");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.GameOver(); // 이게 꼭 있어야 함!
        }
    }

    // 게임 클리어 상태 설정
    public void SetGameCleared()
    {
        isGameCleared = true;
        isGameOver = false;
        Debug.Log("GameManager: 게임 클리어 상태로 설정됨");

    }

    // 게임 상태 확인 메서드들
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
    
    // 카메라 쉐이크 메서드
    public void ShakeCamera(float duration, float intensity)
    {
        StartCoroutine(ShakeCameraCoroutine(duration, intensity));
    }
    
    private System.Collections.IEnumerator ShakeCameraCoroutine(float duration, float intensity)
    {
        if (cameraShakeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cameraShakeSound);
            Debug.Log("카메라 쉐이크 효과음 재생");
        }
        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;
        
        Vector3 originalPosition = mainCamera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            mainCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.localPosition = originalPosition;
        Debug.Log($"카메라 쉐이크 완료 - 지속시간: {duration}초, 강도: {intensity}");
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main") // 또는 scene.buildIndex == 메인 씬 인덱스
        {
            StartCoroutine(InitializeMainScene());
        }
    }

    private System.Collections.IEnumerator InitializeMainScene()
{
    yield return null; // 씬 로드 완료 대기

    Debug.Log("메인 씬 로드됨 - 자동 초기화 시작");

    isGameOver = false;
    isGameCleared = false;
    Time.timeScale = 1f;

    ResetPlayerPosition();
    ResetPlayerState();
    ResetGameTime();
    ResetGameObjects(); // 여기서 Stage도 초기화됨

    // 👇 이 코드 추가로 확실히 Stage 초기화
    Stage stageManager = FindFirstObjectByType<Stage>();
    if (stageManager != null)
    {
        stageManager.ResetStage();
        Debug.Log("Stage 초기화 완료 (씬 로드 후)");
    }
    else
    {
        Debug.LogWarning("Stage 오브젝트를 찾지 못했습니다");
    }
}

}
