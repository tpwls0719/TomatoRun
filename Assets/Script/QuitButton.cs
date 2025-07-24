using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneTransitionManager.Instance.QuitGame();
        });
    }
}
