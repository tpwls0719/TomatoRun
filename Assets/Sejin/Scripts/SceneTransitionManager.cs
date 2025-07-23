using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;
    private bool isFading = false;
    private Color originalColor;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬 로드 이벤트 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        // 이벤트 해제
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // "FadeImage" 태그를 가진 오브젝트 찾기
        GameObject fadeObj = GameObject.FindGameObjectWithTag("FadeImage");
        if (fadeObj != null)
        {
            fadeImage = fadeObj.GetComponent<Image>();
        }
        else
        {
            Debug.LogError("FadeImage 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        if (fadeImage != null)
        {
            originalColor = fadeImage.color;
            fadeImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            StartCoroutine(FadeIn());
        }
    }

    // 사용법: SceneTransitionManager.Instance.FadeOutAndChangeScene(sceneIndex);
    public void FadeOutAndChangeScene(int sceneIndex)
    {
        if (!isFading)
            StartCoroutine(FadeOutAndLoadSceneCoroutine(sceneIndex));
    }


    // 페이드 없이 바로 게임 종료
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서만 실행
#else
        Application.Quit();  // 빌드된 게임에서는 이것이 실행
#endif
    }

    IEnumerator FadeOutAndLoadSceneCoroutine(int targetSceneIndex)
    {
        isFading = true;
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // 알파 1(불투명) - 페이드 아웃

        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeImage.color = endColor;
        isFading = false;

        // 전달받은 씬 이름으로 변경
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneIndex);
        yield return new WaitUntil(() => asyncLoad.isDone);
    }


    IEnumerator FadeIn()
    {
        Debug.Log("FadeIn called");
        isFading = true;
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 알파 0(투명) - 페이드 인

        while (elapsed < fadeInDuration)
        {
            float t = elapsed / fadeInDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeImage.color = endColor;
        isFading = false;
    }

}
