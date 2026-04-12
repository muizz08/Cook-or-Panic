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
        [SerializeField] private float fadeDuration = 0.5f;

        private Coroutine currentFade;

        [Header("Scenes")]
        public SceneReference mainMenu;
        public SceneReference Tutorial;
        public SceneReference gameplay;
        public SceneReference gameOver;
        public SceneReference winner;

        private void Awake()
        {

            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);

                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 🔥 Saat game pertama kali mulai → fade dari hitam ke clear
            if (faderGroup != null)
            {
                faderGroup.alpha = 1;
                currentFade = StartCoroutine(Fade(0));
            }
        }

        // ================= LOAD SCENE =================
        public void LoadScene(SceneReference scene)
        {
            if (scene == null)
            {
                Debug.LogError("SceneReference kosong!");
                return;
            }

            StartCoroutine(FadeOutAndLoad(scene.SceneName));
        }

        public void LoadMainMenu() => LoadScene(mainMenu);
        public void LoadTutorial() => LoadScene(Tutorial);
        public void LoadGameplay() => LoadScene(gameplay);
        public void LoadGameOver() => LoadScene(gameOver);
        public void LoadGameWinner() => LoadScene(winner);

        // ================= ON SCENE LOADED =================
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

            // 🔥 Fade dari hitam ke normal
            if (currentFade != null)
                StopCoroutine(currentFade);

            currentFade = StartCoroutine(Fade(0));
        }

        // ================= FADE SYSTEM =================
        private IEnumerator FadeOutAndLoad(string sceneName)
        {
            yield return StartCoroutine(Fade(1));

            // 🔥 MATIKAN SEMUA TWEEN
            DG.Tweening.DOTween.KillAll();

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (faderGroup == null)
                yield break;

            float startAlpha = faderGroup.alpha;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;

                faderGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            faderGroup.alpha = targetAlpha;

            // Block raycast kalau masih hitam
            faderGroup.blocksRaycasts = targetAlpha > 0.9f;
            faderGroup.interactable = targetAlpha > 0.9f;
        }

        // ================= QUIT =================
        public void QuitGame()
        {
            Debug.Log("Game dihentikan...");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (mainMenu != null) mainMenu.UpdateSceneName();
            if (Tutorial != null) Tutorial.UpdateSceneName();
            if (gameplay != null) gameplay.UpdateSceneName();
            if (gameOver != null) gameOver.UpdateSceneName();
            if (winner != null) winner.UpdateSceneName();
        }
#endif
    }
}