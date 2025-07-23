using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Stage : MonoBehaviour
{
    public int stagePoint;
    public int stageIndex;
    public GameObject[] Stages;

    private float timer = 0f;
    private float stageDuration = 35f;
    private int totalPoint = 0;

    // 🔽 추가: 페이드용 오브젝트
    public CanvasGroup fadePanel;

    public void NextStage()
    {
        // 코루틴 실행으로 스테이지 전환 + 페이드 연출
        StartCoroutine(TransitionStage());
    }

    IEnumerator TransitionStage()
    {
        // 어두워지기
        yield return StartCoroutine(FadeIn());

        // 스테이지 비활성화 & 인덱스 증가
        if (stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
        }

        totalPoint += stagePoint;
        stagePoint = 0;

        // 밝아지기
        yield return StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f;
            fadePanel.alpha = t;
            yield return null;
        }
        fadePanel.alpha = 1f;
        yield return new WaitForSeconds(1f); // 어두운 화면 유지 시간
    }

    IEnumerator FadeOut()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime / 0.5f;
            fadePanel.alpha = t;
            yield return null;
        }
        fadePanel.alpha = 0f;
    }

    void Start()
    {
        for (int i = 0; i < Stages.Length; i++)
        {
            Stages[i].SetActive(i == stageIndex);
        }

        // 초기에 페이드 꺼짐 상태로
        if (fadePanel != null)
            fadePanel.alpha = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= stageDuration)
        {
            timer = 0f;
            NextStage();
        }
    }
}
