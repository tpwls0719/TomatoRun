using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("캐릭터 아이콘 슬라이더")]
    public RectTransform characterIcon;
    public RectTransform sliderBackground;
    
    [Header("점수 시스템")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;    // 최고 점수 텍스트
    public int waterDropScore = 10;  // 물방울당 점수
    private int currentScore = 0;
    private int bestScore = 0;       // 최고 점수
    
    [Header("생명 시스템")]
    public Image[] heartImages;      // 하트 이미지 배열
    public int maxHearts = 3;        // 최대 생명 수
    public int currentHearts = 3;    // 현재 생명 수
    
    [Header("무적 모드")]
    public InvincibilityItem invincibilityController;  // 무적 모드 컨트롤러
    
    [Header("게임 오버 UI")]
    public GameObject gameOverUI;        // 게임 오버 UI 패널 (GameManager에서도 접근 가능)
    public TextMeshProUGUI gameOverScoreText;  // 게임 오버 시 현재 점수 표시
    
    [Header("게임 클리어 UI")]
    public GameObject gameClearUI;       // 게임 클리어 UI 패널
    public TextMeshProUGUI gameClearScoreText;  // 게임 클리어 시 현재 점수 표시
    
    [Header("설정")]
    public float stageDuration = 30f;
    public int maxStages = 5;
    
    private float gameTime = 0f;
    private bool gameCleared = false;  // 게임 클리어 상태 체크
    
    // 데미지 처리 중복 방지용 변수
    private float lastDamageTime = 0f;
    private float damageCooldown = 1f;  // 1초 쿨다운
    
    // 다른 스크립트에서 게임 시간에 접근할 수 있도록 프로퍼티 추가
    public float GameTime { get { return gameTime; } }
    
    public static UIManager Instance { get; private set; }  // 싱글톤 인스턴스
    
    private void Awake()
    {
        // 싱글톤 패턴 구현 (DontDestroyOnLoad 제거)
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad 제거 - UI는 씬에 종속되어야 함
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // 초기 점수 설정
        LoadBestScore();  // 최고 점수 로드
        UpdateScore(0);
        
        // 초기 생명 설정
        UpdateHeartDisplay();
        
        // 게임 오버 UI 초기화
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        // 게임 클리어 UI 초기화
        if (gameClearUI != null)
        {
            gameClearUI.SetActive(false);
        }
    }

    void Update()
    {
        // 슬라이더에 캐릭터 아이콘 위치 업데이트
        UpdateIconPosition();
    }
    
    void UpdateIconPosition()
    {
        if (characterIcon == null || sliderBackground == null || gameCleared) return;
        
        gameTime += Time.deltaTime;
        
        // 전체 게임 시간 기준으로 진행률 계산 (0~1)
        float totalTime = stageDuration * maxStages;
        float progress = Mathf.Clamp01(gameTime / totalTime);
        
        // 슬라이더 너비 기준으로 X 위치 계산
        float width = sliderBackground.rect.width;
        float x = (-width / 2f) + (progress * width);
        
        characterIcon.anchoredPosition = new Vector2(x, characterIcon.anchoredPosition.y);
        
        // 게임 클리어 체크 - 아이콘이 슬라이더 끝에 도달하면 클리어
        float sliderEndPosition = width / 2f; // 슬라이더 오른쪽 끝 위치
        if (x >= sliderEndPosition - 20f && !gameCleared) // 20픽셀 여유를 두고 클리어 체크 (슬라이더 끝 근처에서 클리어)
        {
            Debug.Log($"게임 클리어! 아이콘 위치: {x:F1}, 슬라이더 끝: {sliderEndPosition:F1}");
            GameClear();
        }
    }
    
    // 점수 업데이트 메서드
    public void UpdateScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        
        // 현재 점수 텍스트 업데이트
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
        
        // 최고 점수 갱신 확인
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
            UpdateBestScoreDisplay();
        }
    }
    
    // 최고 점수 로드
    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateBestScoreDisplay();
    }
    
    // 최고 점수 저장
    void SaveBestScore()
    {
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.Save();
    }
    
    // 최고 점수 UI 업데이트
    void UpdateBestScoreDisplay()
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = "Best: " + bestScore.ToString();
        }
    }
    
    // 물방울 아이템 획득 처리
    public void CollectWaterDrop()
    {
        // 물방울 점수 추가
        UpdateScore(waterDropScore);
        Debug.Log($"물방울 획득! +{waterDropScore}점 (현재 점수: {currentScore})");
    }
    
    // 알약(무적) 아이템 획득 처리
    public void CollectPill()
    {
        Debug.Log("알약 획득 - 무적 모드 발동!");
        
        // ItemSpawner에 알약 수집 알림 (스폰 제한 해제)
        if (ItemSpawner.Instance != null)
        {
            ItemSpawner.Instance.OnPillCollected();
            Debug.Log("ItemSpawner에 알약 수집 알림 전송됨");
        }
        else
        {
            Debug.LogWarning("ItemSpawner.Instance가 null입니다!");
        }
        
        if (invincibilityController != null)
        {
            // 이 메서드는 InvincibilityItem 스크립트에서  
            // 무적 상태 시작을 담당하는 함수라고 가정
            invincibilityController.ActivateInvincibility();  
        }
        else
        {
            Debug.LogWarning("InvincibilityItem 컨트롤러가 연결되지 않았습니다!");
        }
    }
    
    // 햇빛(생명) 아이템 획득 처리
    public void CollectSunlight()
    {
        // 최대 생명 수를 초과하지 않도록 생명 추가
        if (currentHearts < maxHearts)
        {
            currentHearts++;
            UpdateHeartDisplay();
            Debug.Log($"햇빛 획득! 하트 +1 (현재 하트: {currentHearts}/{maxHearts})");
        }
        else
        {
            Debug.Log($"햇빛 획득했지만 하트가 이미 최대입니다! ({currentHearts}/{maxHearts})");
        }
    }
    
    // 장애물 충돌 처리 (생명 감소) - 데미지 처리 중복 방지
    public void TakeDamage()
    {
        // 게임이 클리어된 상태면 데미지 처리 안함
        if (gameCleared) return;
        
        // 쿨다운 체크 - 마지막 데미지로부터 1초가 지나지 않았으면 무시
        if (Time.time - lastDamageTime < damageCooldown)
        {
            Debug.Log("데미지 쿨다운 중 - 무시됨");
            return;
        }
        
        // 무적 상태가 아닐 때만 데미지 처리
        if (invincibilityController == null || !invincibilityController.IsInvincible)
        {
            currentHearts--;
            UpdateHeartDisplay();
            lastDamageTime = Time.time; // 데미지 시간 기록
            
            Debug.Log($"하트 감소! 현재 하트: {currentHearts}/{maxHearts}");
            
            // 생명이 0이 되면 게임 오버
            if (currentHearts <= 0)
            {
                GameOver();
            }
        }
        else
        {
            Debug.Log("무적 상태이므로 데미지를 받지 않습니다!");
        }
    }
    
    // 하트 UI 업데이트 - 기존 메서드와 매개변수 있는 메서드 통합
    public void UpdateHeartDisplay()
    {
        UpdateHeartDisplay(currentHearts);
    }
    
    public void UpdateHeartDisplay(int currentHealth)
    {
        if (heartImages == null || heartImages.Length == 0) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].enabled = (i < currentHealth);
            }
        }
    }
    
    // 게임 오버 처리
    public void GameOver()
    {
        // 게임이 이미 클리어된 상태면 게임 오버 처리 안함
        if (gameCleared) return;
        
        // GameManager에 게임 오버 상태 알림
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.EndGame();
            Debug.Log("GameManager에 게임 오버 상태 알림");
        }
        else
        {
            Debug.LogWarning("GameManager를 찾을 수 없습니다!");
        }
                
        // 최고 점수 최종 저장
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
        }
        
        // 게임 오버 UI에 현재 점수 표시
        UpdateGameOverScore();
        
        // 게임 오버 UI 표시
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Debug.Log("게임 오버 UI 활성화됨");
        }
        else
        {
            Debug.LogWarning("게임 오버 UI가 설정되지 않았습니다!");
        }
        
        // 게임 시간 정지
        Time.timeScale = 0f;
        
        Debug.Log($"게임 오버! 최종 점수: {currentScore}, 최고 점수: {bestScore}");
    }
    
    // 게임 클리어 처리
    public void GameClear()
    {
        gameCleared = true;
        
        // GameManager에 게임 클리어 상태 알림
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SetGameCleared();
        }
        
        // 클리어 보너스 점수 추가 (예: 1000점)
        int clearBonus = 1000;
        UpdateScore(clearBonus);
        
        // 최고 점수 최종 저장
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
        }
        
        // 게임 클리어 UI에 현재 점수 표시
        UpdateGameClearScore();
        
        // 게임 클리어 UI 표시
        if (gameClearUI != null)
        {
            gameClearUI.SetActive(true);
        }
        
        // 게임 시간 정지
        Time.timeScale = 0f;
        
        Debug.Log($"게임 클리어! 최종 점수: {currentScore} (클리어 보너스: +{clearBonus}), 최고 점수: {bestScore}");
    }
    
    // 게임 재시작 (게임 오버 UI에서 호출)
    public void RestartGame()
    {
        // 시간 정지 해제
        Time.timeScale = 1f;
        
        // 현재 점수 리셋
        currentScore = 0;
        UpdateScore(0);
        
        // 하트 복구
        currentHearts = maxHearts;
        UpdateHeartDisplay();
        
        // 게임 클리어 상태 리셋
        gameCleared = false;
        
        // 데미지 쿨다운 리셋
        lastDamageTime = 0f;
        
        // 게임 오버 UI 숨기기
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        // 게임 클리어 UI 숨기기
        if (gameClearUI != null)
        {
            gameClearUI.SetActive(false);
        }
        
        Debug.Log("게임 재시작!");
    }
    
    // 버튼용 재시작 메서드 (GameManager의 씬 재로드 방식)
    public void RestartGameButton()
    {
        Debug.Log("재시작 버튼 클릭됨!");
        
        // 게임 오버 UI 먼저 끄기
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        // 게임 클리어 UI 먼저 끄기
        if (gameClearUI != null)
        {
            gameClearUI.SetActive(false);
        }
        
        // GameManager를 안전하게 찾아서 재시작
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            // GameManager가 없으면 기존 방식으로 재시작
            RestartGame();
        }
    }
    
    // 버튼용 빠른 재시작 메서드 (씬 재로드 없이)
    public void RestartGameQuickButton()
    {
        Debug.Log("빠른 재시작 버튼 클릭됨!");
        
        // 게임 오버 UI 먼저 끄기
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        // 게임 클리어 UI 먼저 끄기
        if (gameClearUI != null)
        {
            gameClearUI.SetActive(false);
        }
        
        // GameManager를 안전하게 찾아서 빠른 재시작
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RestartGameWithoutReload();
        }
        else
        {
            // GameManager가 없으면 기존 방식으로 재시작
            RestartGame();
        }
    }
    
    // 게임 오버 UI에 현재 점수 업데이트
    void UpdateGameOverScore()
    {
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "점수 : " + currentScore.ToString();
            Debug.Log($"게임 오버 점수 UI 업데이트: {currentScore}점");
        }
        else
        {
            Debug.LogWarning("게임 오버 점수 텍스트가 설정되지 않았습니다!");
        }
    }
    
    // 게임 클리어 UI에 현재 점수 업데이트
    void UpdateGameClearScore()
    {
        if (gameClearScoreText != null)
        {
            gameClearScoreText.text = "점수 : " + currentScore.ToString();
            Debug.Log($"게임 클리어 점수 UI 업데이트: {currentScore}점");
        }
        else
        {
            Debug.LogWarning("게임 클리어 점수 텍스트가 설정되지 않았습니다!");
        }
    }
    
    // 게임 시간 리셋 메서드
    public void ResetGameTime()
    {
        gameTime = 0f;
        gameCleared = false;  // 게임 클리어 상태도 리셋
        lastDamageTime = 0f;  // 데미지 쿨다운도 리셋
        Debug.Log("게임 시간이 0으로 리셋되었습니다");
    }
}
