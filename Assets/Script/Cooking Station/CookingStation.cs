using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;


namespace CookOrPanic.CookingStation
{
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Recipe;
    using CookOrPanic.Food;
    using CookOrPanic.Timer;
    using CookOrPanic.CanvasManager;
    using CookOrPanic.SocketController;
    using CookOrPanic.TutorialManager;
    using CookOrPanic.AudioManager;
    using CookOrPanic.UIAnimator;

    public class CookingStation : MonoBehaviour
    {


        [Header("Station References")]
        [SerializeField] private List<Recipe> _availableRecipe;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Button _tombolAduk;
        [SerializeField] private Transform _gundukanSpawnPoint;

        [Header("Cooking References")]
        [SerializeField] private Button _tombolAngkat;
        [SerializeField] private ParticleSystem _foodFumesParticle;
        [SerializeField] private ParticleSystem _mixingParticle;


        [Header("Station Socket Controllers")]
        [SerializeField] private SocketController _adonanSocket;
        [SerializeField] private SocketController _oilSocket;
        [SerializeField] private SocketController _drainSocket;

        [Header("Script References")]
        [SerializeField] private Timer _cookingTimer;
        [SerializeField] private CanvasManager _canvasManager;

        private float _elapsedCookingTime;
        private bool _isStoveOn = false;
        private bool _isAdukShown = false;



        private List<IngredientData> _ingredientsInContainer = new List<IngredientData>();


        public System.Action<float> OnIngredientError;

        [System.Serializable]
        public struct IngredientData
        {
            public Ingredient.IngredientType type;
            public FoodState state;
        }

        [System.Serializable]
        public struct IngredientVisual
        {
            public Ingredient.IngredientType _type; // Langsung pakai tipe bahan
            public GameObject _visualObject;
        }

        [SerializeField] private List<IngredientVisual> _ingredientVisuals;

        [Header("UI Peringatan (DOTween)")]
        [SerializeField] private CanvasGroup _warningOnStoveGas;

        private void Start()
        {

            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            if (_mixingParticle != null) _mixingParticle.Stop();

            // Berlangganan ke event dari Controller
            if (_adonanSocket != null)
                _adonanSocket.OnIngredientEntered += OnIngredientEntered;

            if (_oilSocket != null)
            {
                _oilSocket.OnFoodEntered += OnFoodPlacedInOil;
            }
        }


        // --- LOGIKA ADUK BAHAN ---
        // --- LOGIKA ADUK BAHAN ---
        private void OnIngredientEntered(ProcessedIngredient ingredient)
        {
            Ingredient.IngredientType incomingType = ingredient.GetIngredientType();
            FoodState incomingState = ingredient.GetState();

            // Ambil Target dari CanvasManager
            if (_canvasManager == null)
            {
                Debug.LogError("<color=red>[CookingStation]</color> CanvasManager tidak terpasang di Inspector!");
                return;
            }

            FoodType targetSekarang = _canvasManager.CurrentTargetFood;
            Debug.Log($"<color=cyan>[CHECK]</color> Bahan Masuk: <b>{incomingType} ({incomingState})</b> | Pesanan Aktif: <b>{targetSekarang}</b>");

            // Cari Resep yang sesuai dengan pesanan aktif saja
            Recipe resepAktif = _availableRecipe.Find(r => r._resultFoodType == targetSekarang);

            if (resepAktif == null)
            {
                Debug.LogWarning($"<color=yellow>[WARNING]</color> Tidak ada Recipe untuk <b>{targetSekarang}</b> di List _availableRecipe!");
                HandleWrongIngredient(ingredient);
                return;
            }

            string displayName = "";
            bool isIngredientValid = false;
            string reasonForFailure = "Bahan tidak ada di resep ini";

            // Cek di dalam resep yang aktif saja
            foreach (var req in resepAktif._requirements)
            {
                if (req._ingredientType == incomingType && req._requiredState == incomingState)
                {
                    // Hitung jumlah bahan sejenis yang sudah masuk
                    int currentCount = 0;
                    foreach (var input in _ingredientsInContainer)
                    {
                        if (input.type == incomingType) currentCount++;
                    }

                    if (currentCount < req._requiredAmount)
                    {
                        isIngredientValid = true;
                        displayName = req._ingredientName;
                        Debug.Log($"<color=green>[VALID]</color> {incomingType} diterima untuk resep {targetSekarang}. ({currentCount + 1}/{req._requiredAmount})");
                        break;
                    }
                    else
                    {
                        reasonForFailure = $"Jumlah {incomingType} sudah penuh ({req._requiredAmount})";
                    }
                }
            }

            if (isIngredientValid)
            {
                HandleCorrectIngredient(ingredient, displayName);
            }
            else
            {
                Debug.Log($"<color=red>[REJECTED]</color> {incomingType} ditolak karena: {reasonForFailure}");
                HandleWrongIngredient(ingredient);
            }
        }
        protected virtual void HandleCorrectIngredient(ProcessedIngredient ingredient, string nameToDisplay)
        {
            var type = ingredient.GetIngredientType();

            _ingredientsInContainer.Add(new IngredientData
            {
                type = type,
                state = ingredient.GetState()
            });

            bool isTutorial = TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode;


            // Animasi tombol aduk juga sebaiknya pakai DOKill agar tidak tumpang tindih
            if (_ingredientsInContainer.Count == 1 && !_isAdukShown)
            {
                if (_tombolAduk != null)
                {

                    // Gunakan UIAnimator untuk memberikan feedback visual (Pop up/Scale)
                    UIAnimator.Show(_tombolAduk.gameObject, UIAnimator.AnimationType.Scale);
                }
                else
                {
                    Debug.LogError("Variabel _tombolAduk belum di-assign di Inspector!");
                }
            }

            UpdateVisualIngredient();
            if (ingredient.TryGetComponent(out ReturnToSender.ReturnToSender returnScript))
            {
                // Jika ada, panggil fungsi teleport-nya, jangan di-Destroy
                returnScript.ReturnToInitialTransform();
                Debug.Log($"<color=cyan>Teleporting {ingredient.name} back to table.</color>");
            }
            else
            {
                // Jika bukan penyedap (tidak punya script ReturnToSender), baru di-Destroy
                Destroy(ingredient.gameObject);
            }
        }


        private void HandleWrongIngredient(ProcessedIngredient ingredient)
        {
            Debug.Log("EVENT SALAH BAHAN TERPANGGIL");
            OnIngredientError?.Invoke(0.2f);

            // Logika visual (Popup & Audio) tetap di sini karena spesifik di alat ini
            ShowWrongVisualFeedback();
            Destroy(ingredient.gameObject);
        }

        protected virtual void ShowWrongVisualFeedback()
        {
            // 1. Play Error Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ErrorBahan"); // Make sure "ErrorBahan" exists in your AudioManager
            }
        }

        public void TryCook()
        {

            foreach (Recipe recipe in _availableRecipe)
            {
                if (ValidateInternal(recipe))
                {
                    StartCoroutine(MixingProcessRoutine(recipe));
                    return;
                }
            }

            // Contoh pemanggilan di script lain
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnFoodLifted();
            }
            Debug.LogError("[Gagal] Bahan tidak sesuai resep!");
        }

        //logika masak dengan minyak, bisa dipanggil dari tombol angkat

        public void ToggleStove()
        {
            _isStoveOn = !_isStoveOn;

            if (_isStoveOn)
            {

                if (TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode)
                {
                    TutorialManager.Instance.OnStoveTurnedOn();
                }

                AudioManager.Instance.PlaySFX("StoveIgnite");

                // HANYA mainkan jika belum bunyi
                if (!AudioManager.Instance.IsPlaying("StoveLoop"))
                {
                    AudioManager.Instance.PlaySFX("StoveLoop");
                }
            }
            else
            {
                AudioManager.Instance.StopSFX("StoveLoop");
                AudioManager.Instance.StopSFX("MasakAudio");

                if (TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode)
                {
                    TutorialManager.Instance.OnStoveTurnedOff();
                }

            }
            Debug.Log("Kompor sekarang: " + (_isStoveOn ? "NYALA" : "MATI"));

        }

        // --- LOGIKA MASAK (Sekarang menerima Food langsung) ---
        private void OnFoodPlacedInOil(Food foodScript, GameObject foodObject)
        {
            if (foodScript._foodState == FoodState.Raw)
            {
                _elapsedCookingTime = foodScript._timeSpentCooking;

                // 2. Langsung update UI agar tidak menampilkan angka 0 atau angka bekas masakan sebelumnya
                _canvasManager.UpdateUITimerCooking(_elapsedCookingTime);

                Debug.Log($"Makanan masuk kembali. Melanjutkan dari: {_elapsedCookingTime} detik");
                StartCoroutine(CookingRoutine(foodObject, foodScript));
            }
        }

        private IEnumerator CookingRoutine(GameObject foodObject, Food foodScript)
        {
            _tombolAngkat.gameObject.SetActive(true);

            // Variabel pembantu agar UI tidak dipanggil terus-menerus tiap frame
            bool wasStoveOnLastFrame = !_isStoveOn;

            while (foodScript != null && foodScript._foodState == FoodState.Raw)
            {
                if (!_isStoveOn)
                {
                    // Jika baru saja mati (panggil UI hanya sekali)
                    if (wasStoveOnLastFrame)
                    {
                        UIAnimator.Show(_warningOnStoveGas.gameObject, UIAnimator.AnimationType.PulseFade);
                        if (_foodFumesParticle.isPlaying) _foodFumesParticle.Stop();
                        AudioManager.Instance.StopSFX("MasakAudio");
                        wasStoveOnLastFrame = false;
                    }

                    yield return new WaitUntil(() => _isStoveOn);
                }

                // --- JIKA NYALA ---
                // Jika baru saja dinyalakan kembali (panggil UI hanya sekali)
                if (!wasStoveOnLastFrame)
                {
                    // SEHARUSNYA DI SINI ADALAH HIDE, BUKAN SHOW
                    UIAnimator.Hide(_warningOnStoveGas.gameObject, UIAnimator.AnimationType.Fade);

                    if (!AudioManager.Instance.IsPlaying("MasakAudio"))
                        AudioManager.Instance.PlaySFX("MasakAudio");

                    if (!_foodFumesParticle.isPlaying) _foodFumesParticle.Play();

                    wasStoveOnLastFrame = true;
                }

                // 3. PROSES TIMER (Tetap berjalan tiap frame)
                if (!_cookingTimer._isRunning) _cookingTimer.StartTimer(999f);

                _elapsedCookingTime += Time.deltaTime;
                _cookingTimer.TimerCooking(Time.deltaTime);
                _canvasManager.UpdateUITimerCooking(_elapsedCookingTime);

                yield return null;
            }

            // Cleanup saat selesai
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            AudioManager.Instance.StopSFX("MasakAudio");
            // Pastikan warning hilang saat makanan diangkat
            UIAnimator.Hide(_warningOnStoveGas.gameObject, UIAnimator.AnimationType.Fade);
        }


        public void ActionAngkatMakanan()
        {
            // 1. Ambil referensi makanan yang ada di socket saat ini
            List<IXRSelectInteractable> selected = _oilSocket.interactablesSelected;

            if (selected.Count > 0)
            {
                IXRSelectInteractable foodInteractable = selected[0];

                if (foodInteractable.transform.TryGetComponent(out Food foodScript))
                {
                    // 2. SIMPAN WAKTU: Masukkan waktu yang sudah berjalan ke dalam script Food
                    foodScript._timeSpentCooking = _elapsedCookingTime;

                    // 3. UPDATE STATUS: Beri tahu food untuk update visualnya
                    foodScript.UpdateStateBasedOnTime(_elapsedCookingTime);

                    // 4. PAKSA UI tetap menampilkan waktu terakhir (agar tidak kedip jadi 0)
                    _canvasManager.UpdateUITimerCooking(_elapsedCookingTime);

                    Debug.Log($"[Angkat] Waktu tersimpan di {foodScript.name}: {foodScript._timeSpentCooking}s");
                }

                // 5. Keluarkan dari socket
                _oilSocket.interactionManager.SelectExit(_oilSocket.Socket, foodInteractable);

                if (_drainSocket != null)
                {
                    _drainSocket.interactionManager.SelectEnter(_drainSocket.Socket, foodInteractable);
                }
            }

            // 6. MATIKAN EFEK & TIMER (Setelah data aman disimpan)
            StopAllCoroutines();
            _foodFumesParticle.Stop();
            _cookingTimer.StopTimer();
            UIAnimator.Hide(_tombolAngkat.gameObject, UIAnimator.AnimationType.Scale);
            UIAnimator.Show(_warningOnStoveGas.gameObject, UIAnimator.AnimationType.WarningAnimation);

        }

        // --- UTILITIES ---

        private void UpdateVisualIngredient()
        {
            float _sebaranRadius = 0.15f;
            if (_ingredientsInContainer.Count == 0) return;

            var lastType = _ingredientsInContainer[_ingredientsInContainer.Count - 1].type;
            IngredientVisual match = _ingredientVisuals.Find(x => x._type == lastType);

            if (match._visualObject != null)
            {
                // 1. Ambil posisi dasar panci (spawn point)
                Vector3 basePos = _gundukanSpawnPoint.position;

                // 2. Tentukan posisi acak di sekitar titik tengah (sebar ke samping)
                // Semakin banyak bahan, radiusnya bisa sedikit membesar atau tetap di area itu
                Vector2 randomCircle = Random.insideUnitCircle * _sebaranRadius;
                Vector3 spawnPos = new Vector3(basePos.x + randomCircle.x, basePos.y, basePos.z + randomCircle.y);

                // 3. Instantiate gundukan baru di posisi menyebar
                GameObject gundukanBaru = Instantiate(match._visualObject, spawnPos, match._visualObject.transform.rotation); // Menggunakan rotasi asli dari Prefab

                // Ini akan membuat gundukanBaru menjadi anak dari _gundukanSpawnPoint
                gundukanBaru.transform.SetParent(_gundukanSpawnPoint);

                Debug.Log("Gundukan baru muncul menyebar!");
            }
        }

        // Modifikasi fungsi yang sudah ada agar lebih fleksibel
        private bool ValidateInternal(Recipe recipe)
        {
            if (recipe == null) return false;

            int totalNeeded = 0;
            foreach (var req in recipe._requirements) totalNeeded += req._requiredAmount;

            // DEBUG 1: Cek jumlah
            if (_ingredientsInContainer.Count != totalNeeded)
            {
                Debug.Log($"<color=yellow>Gagal:</color> Jumlah bahan baru {_ingredientsInContainer.Count}/{totalNeeded}");
                return false;
            }

            foreach (var req in recipe._requirements)
            {
                int matchCount = 0;
                foreach (var input in _ingredientsInContainer)
                {
                    if (input.type == req._ingredientType && input.state == req._requiredState)
                        matchCount++;
                }

                // DEBUG 2: Cek jenis bahan yang salah
                if (matchCount != req._requiredAmount)
                {
                    Debug.Log($"<color=red>Gagal:</color> Bahan {req._ingredientType} kurang. Butuh {req._requiredAmount}, ada {matchCount}");
                    return false;
                }
            }
            return true;
        }

        private void SpawnFood(Recipe recipe)
        {
            foreach (GameObject prefab in recipe._resultPrefabs)
            {
                if (prefab != null)
                {
                    GameObject spawnedFood = Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
                    if (spawnedFood.TryGetComponent(out Food foodScript))
                    {
                        foodScript.Initialze(recipe._resultFoodType, FoodState.Raw, recipe);
                    }
                }
            }
        }

        private IEnumerator MixingProcessRoutine(Recipe recipe)
        {
            float _mixingDuration = 2.0f;
            // 1. Matikan tombol aduk agar tidak diklik dua kali    
            if (_tombolAduk != null) _tombolAduk.gameObject.SetActive(false);
            ClearStation();


            AudioManager.Instance.PlaySFX("AdonAudio");


            // 2. Munculkan Particle Efek Mengadon
            if (_mixingParticle != null)
            {
                _mixingParticle.Play();
            }

            // 3. Tunggu selama beberapa detik (durasi mengaduk)
            yield return new WaitForSeconds(_mixingDuration);

            // 2. Matikan Suara dan Particle
            AudioManager.Instance.StopSFX("AdonAudio");
            if (_mixingParticle != null) _mixingParticle.Stop();

            // 4. Matikan Particle (opsional, tergantung setting looping particle-mu)
            if (_mixingParticle != null)
            {
                _mixingParticle.Stop();
            }

            // 5. Baru munculkan Prefab Makanan dan bersihkan station
            SpawnFood(recipe);


            Debug.Log("<color=green>CookingStation:</color> Selesai mengaduk, makanan muncul!");
        }

        // Di CookingStation.cs
        public void FailOrderDueToTime()
        {
            // 1. Kosongkan data bahan di list internal
            _ingredientsInContainer.Clear();

            // 2. Hancurkan semua visual bahan (gundukan) yang ada di panci
            foreach (Transform child in _gundukanSpawnPoint)
            {
                // Opsional: Tambahkan partikel asap kecil atau bunyi 'poof'
                Destroy(child.gameObject);
            }

            // 3. Reset status tombol aduk agar tidak bisa diklik
            if (_tombolAduk != null) _tombolAduk.gameObject.SetActive(false);
            _isAdukShown = false;

            // 4. Feedback ke pemain bahwa mereka gagal
            OnIngredientError?.Invoke(0.5f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ErrorBahan");
            }
        }


        protected virtual void ClearStation()
        {
            _ingredientsInContainer.Clear();
            _isAdukShown = false;

            // Hapus UI Checklist
            //foreach (var ui in _activeChecklistUI.Values) { Destroy(ui); }
            //_activeChecklistUI.Clear();

            // Hapus visual gundukan
            foreach (Transform child in _gundukanSpawnPoint) { Destroy(child.gameObject); }

            _tombolAduk.gameObject.SetActive(false);

            //// RESET LOCK DI SINI
            //if (_tombolKlikResep != null) _tombolKlikResep.ResetPanelLock();
            //if (_isiPanelResep != null) _isiPanelResep.ResetPanelLock();

            //_isRecipeOpened = false;
            Debug.Log("<color=green>Station Cleared: Tombol resep bisa ditekan lagi.</color>");
        }

    }
}