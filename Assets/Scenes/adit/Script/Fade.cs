using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeIn : MonoBehaviour
{
    public CanvasGroup faderGroup;

    void Start()
    {
        // Memaksa alpha jadi 1 dulu, baru dipudarkan ke 0
        faderGroup.alpha = 1;
        StartCoroutine(Fade(0));
    }

    public void ToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(1)); // Tutup layar
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator Fade(float targetAlpha)
    {
        float speed = 1.5f; // Atur kecepatan di sini
        while (!Mathf.Approximately(faderGroup.alpha, targetAlpha))
        {
            faderGroup.alpha = Mathf.MoveTowards(faderGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
    }
}