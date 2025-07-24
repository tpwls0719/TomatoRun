using UnityEngine;

public class Stage : MonoBehaviour
{
    public int stagePoint;
    public int stageIndex;
    public GameObject[] Stages;

    private float timer = 0f;
    public float stageDuration = 34f;
    private int totalPoint = 0;

    public PlatformSpawner platformSpawner; // 에디터에서 할당하거나 자동 연결

    public void NextStage()
    {
        // 페이드 효과 없이 바로 스테이지 전환
        TransitionStage();
    }

    void TransitionStage()
    {
        // 스테이지 비활성화 & 인덱스 증가
        if (stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
        }

        totalPoint += stagePoint;
        stagePoint = 0;
    }

    void Start()
    {
        for (int i = 0; i < Stages.Length; i++)
        {
            Stages[i].SetActive(i == stageIndex);
        }
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
