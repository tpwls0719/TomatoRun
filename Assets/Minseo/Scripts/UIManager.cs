using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("캐릭터 아이콘 슬라이더")]
    public RectTransform characterIcon;
    public RectTransform sliderBackground;
    
    [Header("설정")]
    public float stageDuration = 30f;
    public int maxStages = 5;
    
    private float gameTime = 0f;

    void Update()
    {
        // 슬라이더에 캐릭터 아이콘 위치 업데이트
        UpdateIconPosition();
    }
    
    void UpdateIconPosition()
    {
        if (characterIcon == null || sliderBackground == null) return;
        
        gameTime += Time.deltaTime;
        
        // 전체 게임 시간 기준으로 진행률 계산 (0~1)
        float totalTime = stageDuration * maxStages;
        float progress = Mathf.Clamp01(gameTime / totalTime);
        
        // 슬라이더 너비 기준으로 X 위치 계산
        float width = sliderBackground.rect.width;
        float x = (-width / 2f) + (progress * width);
        
        characterIcon.anchoredPosition = new Vector2(x, characterIcon.anchoredPosition.y);
    }
}
