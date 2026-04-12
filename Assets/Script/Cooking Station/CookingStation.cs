using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System;



namespace CookOrPanic.CookingStation
{

    using CookOrPanic.AudioManager;
    using CookOrPanic.CanvasManager;
    using CookOrPanic.Food;
    using CookOrPanic.IngredientPasser;
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Ingredient;
    using CookOrPanic.Recipe;
    using CookOrPanic.SeasoningPour;
    using CookOrPanic.SocketController;
    using CookOrPanic.Timer;
    using CookOrPanic.TutorialManager;
    using CookOrPanic.UIAnimator;
    using Random = UnityEngine.Random;
    using CookOrPanic.IngredientRestockManager;


    [System.Serializable]
    public struct IngredientData
    {
        public IngredientType type;
        public FoodState state;
    }

    [System.Serializable]
    public struct IngredientVisual
    {
        public IngredientType _type; // Langsung pakai tipe bahan
        public GameObject _visualObject;
    }

    public class CookingStation : MonoBehaviour
    {

        [Header("Script References")]
        [SerializeField] private Timer _cookingTimer;
        [SerializeField] private CanvasManager _canvasManager;
        [SerializeField] private IngredientPasser _manualPasser;

        [Header("Station References")]
        [SerializeField] private List<Recipe> _availableRecipe;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Button _foodStirButton;
        [SerializeField] private Transform _foodMoundSpawn;
        [SerializeField] private Button _foodDrainButton;
        [SerializeField] private Transform _stoveKnob;

        [Header("Particle system References")]
        [SerializeField] private ParticleSystem _stoveFireParticle;
        [SerializeField] private ParticleSystem _foodFumesParticle;
        [SerializeField] private ParticleSystem _mixingParticle;

        [Header("Station Socket Controllers")]
        [SerializeField] private SocketController _compoundSocket;
        [SerializeField] private SocketController _oilSocket;
        [SerializeField] private SocketController _drainSocket;

    
        private float _elapsedCookingTime;
        private bool _isStoveOn = false;


        public static Action<IngredientType> OnSeasoningAdded;
        private List<IngredientData> _ingredientsInContainer = new List<IngredientData>();
        public Action<float> OnIngredientError;
        public Action<float> OrderTimeout;
        [SerializeField] private List<IngredientVisual> _ingredientVisuals;

    
        private void Start()
        {
            if (_manualPasser != null)
            {
                _manualPasser.currentStation = this;
                Debug.Log($"<color=green>[LINK FIXED]</color> {_manualPasser.name} berhasil dihubungkan via Inspector!");
            }
           
            if (_compoundSocket != null)
            {
                _compoundSocket.OnIngredientEntered += OnIngredientEntered;
            }

            // 2. Subscribe ke Socket Minyak (Oil)
           
            if (_oilSocket != null)
            {
                _oilSocket.OnFoodEntered += OnFoodPlacedInOil;
            }

            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            if (_mixingParticle != null) _mixingParticle.Stop();
            if (_stoveFireParticle != null) _stoveFireParticle.Stop();
            if(_stoveFireParticle != null) _stoveFireParticle.Stop();
        }


        // --- LOGIKA ADUK BAHAN ---
        // --- LOGIKA ADUK BAHAN ---
        private void OnIngredientEntered(ProcessedIngredient ingredient)
        {
            IngredientType incomingType = ingredient.GetIngredientType();
            FoodState incomingState = ingredient.GetState();

            if (_canvasManager == null)
            {
                Debug.LogError("<color=red>[CookingStation]</color> CanvasManager tidak terpasang!");
                return;
            }

            FoodType targetSekarang = _canvasManager.CurrentTargetFood;
            Debug.Log($"<color=cyan>[SOCKET ENTER]</color> Objek: <b>{ingredient.name}</b> | Tipe: {incomingType} | State: {incomingState}");

            // --- LINKING PASSER DEBUG ---
            if (_manualPasser != null)
            {
                _manualPasser.currentStation = this;
                Debug.Log($"<color=green>[LINK FIXED]</color> {_manualPasser.name} berhasil dihubungkan via Inspector!");
            }

            Recipe resepAktif = _availableRecipe.Find(r => r._resultFoodType == targetSekarang);

            if (resepAktif == null)
            {
                Debug.LogWarning($"<color=yellow>[WARNING]</color> Tidak ada Recipe untuk <b>{targetSekarang}</b>!");
                HandleWrongIngredient(ingredient);
                return;
            }

            string displayName = "";
            bool isIngredientValid = false;
            string reasonForFailure = "Bahan tidak ada di resep ini";

            foreach (var req in resepAktif._requirements)
            {
                if (req._ingredientType == incomingType && req._requiredState == incomingState)
                {
                    int currentCount = _ingredientsInContainer.FindAll(i => i.type == incomingType).Count;

                    if (currentCount < req._requiredAmount)
                    {
                        isIngredientValid = true;
                        displayName = req._ingredientName;
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
                Debug.Log($"<color=red>[REJECTED]</color> {incomingType} ditolak: {reasonForFailure}");
                HandleWrongIngredient(ingredient);
            }

        }
        public void OnParticleCollision(GameObject other)
        {

            // Debug level 1: Apakah ada partikel yang menabrak sama sekali?
            Debug.Log($"<color=white>[PARTICLE HIT]</color> Tabrakan terdeteksi di {gameObject.name} dari {other.name}");

            SeasoningPour seasoning = other.GetComponentInParent<SeasoningPour>();
            if (seasoning == null)
            {
                Debug.LogWarning($"<color=gray>[PARTICLE IGNORE]</color> {other.name} bukan botol bumbu (Script SeasoningPour tidak ditemukan).");
                return;
            }

            if (!seasoning.IsPouring()) return;

            IngredientType incomingType = seasoning.MyType;
            Debug.Log($"<color=orange>[SEASONING DETECTED]</color> Mencoba memasukkan bumbu: <b>{incomingType}</b>");

            HandleSeasoning(incomingType);
        }

        private void HandleSeasoning(IngredientType type)
        {
            FoodType targetSekarang = _canvasManager.CurrentTargetFood;
            Recipe resepAktif = _availableRecipe.Find(r => r._resultFoodType == targetSekarang);

            if (TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode)
            {
                TutorialManager.Instance.HandleSeasoningPlaced(IngredientType.PenyedapRasa);
            }

            if (resepAktif == null)
            {
                Debug.LogWarning("<color=red>[SEASONING FAIL]</color> Tidak bisa menuang bumbu karena resep tidak ditemukan.");
                return;
            }

            foreach (var req in resepAktif._requirements)
            {
                if (req._ingredientType == type)
                {
                    int count = _ingredientsInContainer.FindAll(i => i.type == type).Count;

                    if (count < req._requiredAmount)
                    {
                        Debug.Log($"<color=green>[SEASONING SUCCESS]</color> Bumbu {type} masuk ke dalam wadah ({count + 1}/{req._requiredAmount})");
                        HandleCorrectIngredient(null, req._ingredientName, type);
                        return;
                    }
                    else
                    {
                        Debug.LogWarning($"<color=yellow>[SEASONING FULL]</color> Bumbu {type} sudah mencukupi resep.");
                    }
                }
            }
        }


        // Tambahkan parameter pType dengan default value (0 biasanya index pertama enum kamu)
        protected virtual void HandleCorrectIngredient(ProcessedIngredient ingredient, string nameToDisplay, IngredientType pType = default)
        {
           
            // 1. Tentukan Tipe dan State
            IngredientType type = (ingredient != null) ? ingredient.GetIngredientType() : pType;
            FoodState state = (ingredient != null) ? ingredient.GetState() : FoodState.Raw;

            // 2. Tambahkan ke List Internal
            _ingredientsInContainer.Add(new IngredientData
            {
                type = type,
                state = state
            });

            // 3. MUNCULKAN VISUAL GUNDUKAN (Panggil fungsi yang sudah ada)
            UpdateVisualIngredient();

            // 4. CEK APAKAH RESEP SUDAH LENGKAP UNTUK MUNCULKAN TOMBOL ADUK
            //FoodType target = _canvasManager.CurrentTargetFood;
            //Recipe resepAktif = _availableRecipe.Find(r => r._resultFoodType == target);

            if (_ingredientsInContainer.Count >= 1 && _foodStirButton != null)
            {
                if (!_foodStirButton.gameObject.activeSelf)
                {
                    _foodStirButton.gameObject.SetActive(true);

                    // Opsional: Tambahkan animasi sedikit agar munculnya halus
                    _foodStirButton.transform.localScale = Vector3.zero;
                    _foodStirButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            }

            if (ingredient != null)
            {
                //Ingredient.IngredientType type = ingredient.GetIngredientType();

                // CEK KHUSUS: Jika Ikan atau Ayam, langsung Destroy
                if (type == IngredientType.IkanGiling || type == IngredientType.AyamGiling)
                {
                    Destroy(ingredient.gameObject);
                }
                else if (IngredientRestockManager.Instance != null)
                {
                    // Selain itu, panggil Restock Manager
                    IngredientRestockManager.Instance.HandleIngredientUsed(ingredient.gameObject, type);
                }
                else
                {
                    Destroy(ingredient.gameObject);
                }
            }
        }

        private void HandleWrongIngredient(ProcessedIngredient ingredient)
        {
            Debug.Log("<color=red>[SALAH BAHAN]</color> Mengembalikan " + ingredient.name + " ke wadah.");

            // 1. Feedback Visual/Audio
            OnIngredientError?.Invoke(0.2f);
            ShowWrongVisualFeedback();

            IngredientType type = ingredient.GetIngredientType();

            // --- PERBAIKAN DI SINI ---
            // 2. Ambil XRGrabInteractable dari bahan tersebut
            if (ingredient.TryGetComponent(out XRGrabInteractable interactable))
            {
                IXRSelectInteractor socketInteractor = interactable.firstInteractorSelecting;

                if (socketInteractor != null)
                {
                    // Ambil interactionManager dari socket itu sendiri
                    var manager = _compoundSocket.Socket.interactionManager;
                    manager.SelectExit(socketInteractor, (IXRSelectInteractable)interactable);
                }
            }
            // -------------------------

            // 3. Logika Penghancuran atau Pengembalian
            if (type == IngredientType.IkanGiling || type == IngredientType.AyamGiling)
            {
                Destroy(ingredient.gameObject);
                return;
            }

            if (IngredientRestockManager.Instance != null)
            {
                // Sekarang manager bisa memindahkan objek karena sudah bebas dari socket
                IngredientRestockManager.Instance.HandleIngredientUsed(ingredient.gameObject, type);
            }
            else
            {
                Destroy(ingredient.gameObject);
            }
        }

        protected virtual void ShowWrongVisualFeedback()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ErrorBahan"); // Make sure "ErrorBahan" exists in your AudioManager
            }
        }

        public void StirTheIngredient()
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
            Vector3 targetRotation = _isStoveOn ? new Vector3(90, 0, 0) : new Vector3(90, 90, 0);
            float duration = 0.3f;

            if (_isStoveOn)
            {
                if (_stoveKnob != null)
                    _stoveKnob.DORotate(targetRotation, duration).SetEase(Ease.OutBack);
                // NYALAKAN PARTIKEL API KOMPOR
                if (_stoveFireParticle != null && !_stoveFireParticle.isPlaying)
                {
                    _stoveFireParticle.Play();
                }

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
                if (_stoveFireParticle != null && _stoveFireParticle.isPlaying)
                {
                    _stoveFireParticle.Stop();
                }
                AudioManager.Instance.StopSFX("StoveLoop");
                AudioManager.Instance.StopSFX("MasakAudio");

                if (_stoveKnob != null)
                    _stoveKnob.DORotate(targetRotation, duration).SetEase(Ease.InSine);

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
            _foodDrainButton.gameObject.SetActive(true);

            // Variabel pembantu agar UI tidak dipanggil terus-menerus tiap frame
            bool wasStoveOnLastFrame = !_isStoveOn;

            while (foodScript != null && foodScript._foodState == FoodState.Raw)
            {
                if (!_isStoveOn)
                {
                    // Jika baru saja mati (panggil UI hanya sekali)
                    if (wasStoveOnLastFrame)
                    {
                        ShowStoveWarning();
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
                    HideStoveWarning();

                    if (!AudioManager.Instance.IsPlaying("MasakAudio"))
                        AudioManager.Instance.PlaySFX("MasakAudio");

                    if (!_foodFumesParticle.isPlaying) _foodFumesParticle.Play();

                    wasStoveOnLastFrame = true;
                }

                // --- PROSES TIMER MASAK ---

                // 1. Cek apakah timer sudah jalan, jika belum, nyalakan SATU KALI saja
                // Kita gunakan status khusus masak: _isCookRunning
                if (!_cookingTimer.IsCookRunning())
                {
                    _cookingTimer.StartCookingTimer();
                }

                // 2. Update UI
                // Ambil waktu langsung dari script timer agar sinkron
                _elapsedCookingTime = _cookingTimer.GetCookTime();
                _canvasManager.UpdateUITimerCooking(_elapsedCookingTime);

                yield return null;
            }

            // Cleanup saat selesai
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            AudioManager.Instance.StopSFX("MasakAudio");
            // Pastikan warning hilang saat makanan diangkat
            HideStoveWarning();
        }


        public void DrainTheFood()
        {
            // 1. Validasi awal: Pastikan referensi dari Inspector tidak kosong
            if (_oilSocket == null || _drainSocket == null)
            {
                Debug.LogError("<color=red>[CookingStation]</color> Socket Minyak atau Penirisan belum di-assign di Inspector!");
                return;
            }

            // 2. Ambil XRSocketInteractor dari wrapper SocketController kita
            XRSocketInteractor oilInteractor = _oilSocket.Socket;
            XRSocketInteractor drainInteractor = _drainSocket.Socket;

            // 3. Ambil list benda yang sedang menempel di interactor minyak
            var selected = oilInteractor.interactablesSelected;

            if (selected.Count > 0)
            {
                IXRSelectInteractable foodInteractable = selected[0];

                // --- LOGIKA GAMEPLAY ---
                if (foodInteractable.transform.TryGetComponent(out Food foodScript))
                {
                    // Simpan progres waktu masak terakhir ke script makanan
                    foodScript._timeSpentCooking = _elapsedCookingTime;

                    // Beri tahu makanan untuk update visual (Raw -> Cooked -> Burnt)
                    foodScript.UpdateStateBasedOnTime(_elapsedCookingTime);

                    // Update UI agar tetap menunjukkan waktu terakhir saat diangkat
                    if (_canvasManager != null)
                        _canvasManager.UpdateUITimerCooking(_elapsedCookingTime);

                    Debug.Log($"[Angkat] {foodScript.name} dipindahkan. Waktu tersimpan: {foodScript._timeSpentCooking}s");
                }

                // --- LOGIKA PEMINDAHAN (Socket to Socket) ---
                XRInteractionManager interactionManager = oilInteractor.interactionManager;

                // A. Paksa keluar dari Socket Minyak
                // Gunakan interactor-nya langsung (oilInteractor), bukan wrapper-nya (_oilSocket)
                interactionManager.SelectExit(oilInteractor, foodInteractable);

                // B. Teleport posisi secara instan agar tidak melayang (Optional but recommended)
                foodInteractable.transform.position = drainInteractor.attachTransform.position;
                foodInteractable.transform.rotation = drainInteractor.attachTransform.rotation;

                // C. Paksa masuk ke Socket Penirisan
                interactionManager.SelectEnter(drainInteractor, foodInteractable);

                Debug.Log($"[Drain] Berhasil memindahkan {foodInteractable.transform.name} ke penirisan.");
            }

            // 4. MATIKAN EFEK & TIMER (Setelah data aman disimpan)
            StopAllCoroutines();
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            if (_cookingTimer != null) _cookingTimer.StopCookingTimer();

            UIAnimator.Hide(_foodDrainButton.gameObject, UIAnimator.AnimationType.Scale);
        }

        // --- UTILITIES ---

        private void UpdateVisualIngredient()
        {
            AudioManager.Instance.PlaySFX("MoundSound");
            float _sebaranRadius = 0.07f;
            if (_ingredientsInContainer.Count == 0) return;

            var lastType = _ingredientsInContainer[_ingredientsInContainer.Count - 1].type;
            IngredientVisual match = _ingredientVisuals.Find(x => x._type == lastType);

            if (match._visualObject != null)
            {
                // 1. Ambil posisi dasar panci (spawn point)
                Vector3 basePos = _foodMoundSpawn.position;

                // 2. Tentukan posisi acak di sekitar titik tengah (sebar ke samping)
                // Semakin banyak bahan, radiusnya bisa sedikit membesar atau tetap di area itu
                Vector2 randomCircle = Random.insideUnitCircle * _sebaranRadius;
                Vector3 spawnPos = new Vector3(basePos.x + randomCircle.x, basePos.y, basePos.z + randomCircle.y);

                // 3. Instantiate gundukan baru di posisi menyebar
                GameObject gundukanBaru = Instantiate(match._visualObject, spawnPos, match._visualObject.transform.rotation); // Menggunakan rotasi asli dari Prefab

                // Ini akan membuat gundukanBaru menjadi anak dari _gundukanSpawnPoint
                gundukanBaru.transform.SetParent(_foodMoundSpawn);
                

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
            if (_foodStirButton != null) _foodStirButton.gameObject.SetActive(false);
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
            foreach (Transform child in _foodMoundSpawn)
            {
                // Opsional: Tambahkan partikel asap kecil atau bunyi 'poof'
                Destroy(child.gameObject);
            }

            OrderTimeout?.Invoke(0.34f);

            // 3. Reset status tombol aduk agar tidak bisa diklik
            if (_foodStirButton != null) _foodStirButton.gameObject.SetActive(false);
            //_isAdukShown = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("HeadChefEmosi");
            }

        }

        protected virtual void ShowStoveWarning() { }
        protected virtual void HideStoveWarning() { }

        protected virtual void ClearStation()
        {
            _ingredientsInContainer.Clear();

            // Hapus visual gundukan
            foreach (Transform child in _foodMoundSpawn) { Destroy(child.gameObject); }

            _foodStirButton.gameObject.SetActive(false);

            Debug.Log("<color=green>Station Cleared: Tombol resep bisa ditekan lagi.</color>");
        }

       
    }
}