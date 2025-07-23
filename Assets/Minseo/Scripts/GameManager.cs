using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }  // 싱글톤 인스턴스
    
    [Header("게임 설정")]
    public bool isGameOver = false;
    public bool isGamePaused = false;
    
    private void Awake()
    {
        // 싱글톤 패턴 구현 (DontDestroyOnLoad 제거)
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad 제거 - UI 오브젝트와의 충돌 방지
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 게임 시작 시 초기화
        InitializeGame();
    }

    // Update is called once per frame
    void Update()
    {
        // ESC 키로 게임 일시정지/재개
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        // R 키로 게임 재시작 (디버그용)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
    
    // 게임 초기화
    private void InitializeGame()
    {
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;
        
        Debug.Log("게임 초기화 완료");
    }

    // 게임 재시작 (씬 재로드)
    
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void RestartGame()
    {
        Debug.Log("게임 재시작 - 씬 재로드");

        // 게임 오버 UI 먼저 끄기 (씬 재로드 전에) - 안전한 방식으로
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null && uiManager.gameOverUI != null)
        {
            uiManager.gameOverUI.SetActive(false);
        }

        // 시간 정지 해제
        Time.timeScale = 1f;
        isGameOver = false;
        isGamePaused = false;

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
        
        // 시간 정지 해제
        Time.timeScale = 1f;
        isGameOver = false;
        isGamePaused = false;
        
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
    
    // 게임 일시정지
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        
        Debug.Log("게임 일시정지");
        
        // 일시정지 UI 표시 (있는 경우)
        // pauseUI.SetActive(true);
    }
    
    // 게임 재개
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        
        Debug.Log("게임 재개");
        
        // 일시정지 UI 숨기기 (있는 경우)
        // pauseUI.SetActive(false);
    }
    
    // 게임 오버 처리
    public void GameOver()
    {
        isGameOver = true;
        
        Debug.Log("GameManager - 게임 오버 처리 시작");
        
        // UIManager의 게임 오버 처리 호출 - 안전한 방식으로
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            Debug.Log("UIManager를 통해 게임 오버 UI 표시");
            uiManager.GameOver();
        }
        else
        {
            Debug.LogError("UIManager를 찾을 수 없습니다! 게임 오버 UI를 표시할 수 없습니다.");
            // 최소한 게임 시간은 정지
            Time.timeScale = 0f;
        }
    }
    
    // 플레이어 위치 초기화
    private void ResetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 플레이어를 시작 위치로 이동 (필요에 따라 좌표 조정)
            player.transform.position = new Vector3(0, 0, 0);
            
            // 플레이어의 속도 초기화
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }
            
            Debug.Log("플레이어 위치 초기화");
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
        
        GameObject[] activeWaterDrops = GameObject.FindGameObjectsWithTag("WaterDrop");
        for (int i = 0; i < activeWaterDrops.Length; i++)
        {
            if (activeWaterDrops[i].activeSelf)
            {
                activeWaterDrops[i].SetActive(false);
            }
        }
        
        GameObject[] activeSunLights = GameObject.FindGameObjectsWithTag("SunLight");
        for (int i = 0; i < activeSunLights.Length; i++)
        {
            if (activeSunLights[i].activeSelf)
            {
                activeSunLights[i].SetActive(false);
            }
        }
        
        Debug.Log("게임 오브젝트들 초기화 완료");
    }
    
    // 메인 메뉴로 돌아가기
    public void GoToMainMenu()
    {
        Debug.Log("메인 메뉴로 이동");
        
        // 시간 정지 해제
        Time.timeScale = 1f;
        
        // 메인 메뉴 씬 로드 (씬 이름을 실제 메인 메뉴 씬 이름으로 변경)
        SceneManager.LoadScene("MainMenu");
    }
    
    // 게임 종료
    public void QuitGame()
    {
        Debug.Log("게임 종료");
        
#if UNITY_EDITOR
        // 에디터에서 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서 애플리케이션 종료
        Application.Quit();
#endif
    }
}
