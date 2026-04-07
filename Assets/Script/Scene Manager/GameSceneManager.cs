using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace CookOrPanic.GameSceneManager
{
    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance;

        [Header("Fade Settings")]
        [SerializeField] private CanvasGroup faderGroup;
        [SerializeField] private float fadeSpeed = 1.5f;

        [Header("Scenes")]
        public SceneReference mainMenu;
        public SceneReference Tutorial;
        public SceneReference gameplay;
        public SceneReference gameOver;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                SceneManager.sceneLoaded += OnSceneLoaded; // 🔥 TAMBAH INI
            }
            else
            {
                Destroy(gameObject);
            }
        }


        public void LoadScene(SceneReference scene)
        {
            if (scene == null)
            {
                Debug.LogError("SceneReference kosong!");
                return;
            }

            StartCoroutine(FadeOutAndLoad(scene.SceneName));
        }

        public void LoadMainMenu()
        {
            LoadScene(mainMenu);
        }

        public void LoadTutorial()
        {
            LoadScene(Tutorial);
        }

        public void LoadGameplay()
        {
            LoadScene(gameplay);
        }

        public void LoadGameOver()
        {
            LoadScene(gameOver);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 🔥 RE-GET fader kalau hilang
            if (faderGroup == null)
            {
                faderGroup = FindObjectOfType<CanvasGroup>();
            }

            if (faderGroup == null)
            {
                Debug.LogError("FaderGroup tidak ditemukan di scene!");
                return;
            }

            faderGroup.alpha = 1;
            faderGroup.blocksRaycasts = true;

            StartCoroutine(Fade(0));
            Debug.Log("Fader: " + (faderGroup == null ? "NULL" : "ADA"));
        }

        // ================= FADE SYSTEM =================
        private IEnumerator FadeOutAndLoad(string sceneName)
        {
            yield return StartCoroutine(Fade(1));

            // 🔥 FIX ERROR DOTWEEN
            DG.Tweening.DOTween.KillAll();

            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (faderGroup == null) yield break;

            float timer = 0f;

            while (!Mathf.Approximately(faderGroup.alpha, targetAlpha))
            {
                faderGroup.alpha = Mathf.MoveTowards(
                    faderGroup.alpha,
                    targetAlpha,
                    fadeSpeed * Time.deltaTime
                );

                timer += Time.deltaTime;

                if (timer > 5f) // ⛑️ anti stuck
                {
                    Debug.LogWarning("Fade timeout!");
                    break;
                }

                yield return null;
            }

            faderGroup.alpha = targetAlpha;

            if (targetAlpha <= 0)
            {
                faderGroup.blocksRaycasts = false;
                faderGroup.interactable = false;
            }
            else
            {
                faderGroup.blocksRaycasts = true;
                faderGroup.interactable = true;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
    {
        if (mainMenu != null) mainMenu.UpdateSceneName();
        if (Tutorial != null) Tutorial.UpdateSceneName();
        if (gameplay != null) gameplay.UpdateSceneName();
        if (gameOver != null) gameOver.UpdateSceneName();
    }
#endif
    }
}