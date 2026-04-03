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
        [SerializeField] private HeadChef _headChef; 

        private float _elapsedCookingTime;
        private bool _isStoveOn = false;

        // Tambahkan di bagian Header References
        [Header("UI Checklist & Feedback")]
        [SerializeField] private GameObject _checklistPrefab;
        [SerializeField] private Transform _checklistParent;
        [SerializeField] private ScrollRect _checklistScrollRect;
        [SerializeField] private CanvasGroup _wrongIngredientPopup; // Popup "Bahan Salah"

        [Header("Feedback Visual")]
        [SerializeField] private Heart.Heart _heartEffect; // Drag & drop objek Heart di Inspector

        [Header("Panic Settings")]
        [SerializeField] private float _panicThreshold = 0.8f; // 0.8 berarti 80% emosi
        private bool _isPanicActive = false;

        private Dictionary<Ingredient.IngredientType, GameObject> _activeChecklistUI = new Dictionary<Ingredient.IngredientType, GameObject>();

        private List<IngredientData> _ingredientsInContainer = new List<IngredientData>();

        [Header("Tutorial Configuration")]
        [SerializeField] private bool _isTutorialMode = false;
        [SerializeField] private Recipe _tutorialRecipe;

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
        [SerializeField] private CanvasGroup _warningOffStoveGas;


        private void Start()
        {
            if (_tombolAduk != null) _tombolAduk.gameObject.SetActive(false);
            if (_tombolAngkat != null) _tombolAngkat.gameObject.SetActive(false);
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            if(_mixingParticle != null) _mixingParticle.Stop();

            // Berlangganan ke event dari Controller
            if (_adonanSocket != null)
                _adonanSocket.OnIngredientEntered += OnIngredientEntered;

            if (_oilSocket != null)
            {
                _oilSocket.OnFoodEntered += OnFoodPlacedInOil;
            }
        }
        private void Update()
        {
            CheckChefEmotionPanic();
        }

        // --- LOGIKA ADUK BAHAN ---
        // --- LOGIKA ADUK BAHAN ---
        private void OnIngredientEntered(ProcessedIngredient ingredient)
        {
            Ingredient.IngredientType incomingType = ingredient.GetIngredientType();
            FoodState incomingState = ingredient.GetState();

            string displayName = "";
            bool isIngredientValid = false;

            // 1. VALIDASI KETAT: Cek apakah bahan ini ada di resep yang tersedia
            foreach (Recipe recipe in _availableRecipe)
            {
                foreach (var req in recipe._requirements)
                {
                    // Cek apakah Tipe dan State sesuai dengan kebutuhan resep
                    if (req._ingredientType == incomingType && req._requiredState == incomingState)
                    {
                        // TAMBAHAN: Cek apakah bahan ini sudah dimasukkan melebihi jumlah yang dibutuhkan?
                        // Ini opsional, tapi bagus agar player tidak memasukkan 10 garam jika butuh 1.
                        int currentCount = 0;
                        foreach (var input in _ingredientsInContainer)
                        {
                            if (input.type == incomingType) currentCount++;
                        }

                        if (currentCount < req._requiredAmount)
                        {
                            isIngredientValid = true;
                            displayName = req._ingredientName;
                            break;
                        }
                    }
                }
                if (isIngredientValid) break;
            }

            // 2. EKSEKUSI
            if (isIngredientValid)
            {
                // Jika valid, baru masukkan ke list dan proses visual
                HandleCorrectIngredient(ingredient, displayName);
            }
            else
            {
                // Jika salah (tidak ada di resep atau jumlah berlebih), 
                // langsung destroy dan munculkan popup tanpa masuk ke _ingredientsInContainer
                HandleWrongIngredient(ingredient);
            }
        }   

        private void HandleCorrectIngredient(ProcessedIngredient ingredient, string nameToDisplay)
        {
            var type = ingredient.GetIngredientType();

            _ingredientsInContainer.Add(new IngredientData
            {
                type = type,
                state = ingredient.GetState()
            });

            if (!_activeChecklistUI.ContainsKey(type))
            {
                GameObject newChecklist = Instantiate(_checklistPrefab, _checklistParent);

                // 1. Set text dulu
                var textMesh = newChecklist.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textMesh != null) textMesh.text = nameToDisplay;

                // 2. Reset Scale ke 0 sebelum aktif agar tidak "flash" ukuran penuh
                newChecklist.transform.localScale = Vector3.zero;

                // 3. PAKSA Layout Group menghitung posisi detik ini juga
                // Gunakan LayoutRebuilder pada parent-nya
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_checklistParent.GetComponent<RectTransform>());

                // 4. Baru jalankan animasi DOTween
                newChecklist.transform.DOKill();
                newChecklist.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

                _activeChecklistUI.Add(type, newChecklist);
            }

            // Animasi tombol aduk juga sebaiknya pakai DOKill agar tidak tumpang tindih
            if (!_isTutorialMode && _ingredientsInContainer.Count >= 1)
            {
                _tombolAduk.gameObject.SetActive(true);
                _tombolAduk.transform.DOKill();
                _tombolAduk.transform.localScale = Vector3.zero;
                _tombolAduk.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }

            UpdateVisualIngredient();
            if (ingredient.TryGetComponent(out CookOrPanic.ReturnToSender.ReturnToSender returnScript))
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
            Debug.Log("<color=red>Bahan Salah! Emosi Chef Meningkat!</color>");

            StartCoroutine(TriggerHeartEffect(2f));

            if (_headChef != null)
            {
                _headChef.TambahEmosi(0.2f);
            }

            // ANIMASI POPUP SALAH
            if (_wrongIngredientPopup != null)
            {
                _wrongIngredientPopup.gameObject.SetActive(true);
                _wrongIngredientPopup.DOKill();
                _wrongIngredientPopup.alpha = 0;
                _wrongIngredientPopup.transform.localScale = Vector3.one * 0.7f;

                Sequence s = DOTween.Sequence();
                s.Append(_wrongIngredientPopup.DOFade(1f, 0.2f));
                s.Join(_wrongIngredientPopup.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
                s.AppendInterval(1.5f); // Tahan sebentar
                s.Append(_wrongIngredientPopup.DOFade(0f, 0.3f));
                s.OnComplete(() => _wrongIngredientPopup.gameObject.SetActive(false));
            }

            AudioManager.Instance.PlaySFX("ChefAngry"); // Pastikan ada Sound ini
            Destroy(ingredient.gameObject);
        }

        private void CheckChefEmotionPanic()
        {
            if (_headChef == null || _heartEffect == null) return;

            // Ambil nilai emosi dari HeadChef
            float currentEmosi = _headChef.GetIsiEmosi();

            if (currentEmosi >= _panicThreshold)
            {
                // Jika sudah masuk zona panik dan efek belum aktif, aktifkan
                if (!_isPanicActive)
                {
                    _isPanicActive = true;
                    _heartEffect.gameObject.SetActive(true);
                    Debug.Log("<color=orange>Chef hampir meledak! Efek jantung aktif terus.</color>");
                }
            }
            else
            {
                // Jika emosi turun di bawah threshold (misal ada mekanik pendingin), matikan
                if (_isPanicActive)
                {
                    _isPanicActive = false;
                    _heartEffect.gameObject.SetActive(false);
                }
            }
        }

        // Modifikasi Coroutine lama agar tidak mematikan jantung jika sedang panik
        private IEnumerator TriggerHeartEffect(float duration)
        {
            if (_heartEffect != null)
            {
                _heartEffect.gameObject.SetActive(true);

                yield return new WaitForSeconds(duration);

                // HANYA matikan jika emosi Chef masih di bawah batas panik
                if (_headChef != null && _headChef.GetIsiEmosi() < _panicThreshold)
                {
                    _heartEffect.gameObject.SetActive(false);
                }
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
                TutorialManager.Instance.OnStoveTurnedOn();
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
                TutorialManager.Instance.OnStoveTurnedOff();
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
                _canvasManager.UpdateUITimer(_elapsedCookingTime);

                Debug.Log($"Makanan masuk kembali. Melanjutkan dari: {_elapsedCookingTime} detik");
                StartCoroutine(CookingRoutine(foodObject, foodScript));
            }
        }

        private IEnumerator CookingRoutine(GameObject foodObject, Food foodScript)
        {
            _tombolAngkat.gameObject.SetActive(true);

            while (foodScript != null && foodScript._foodState == FoodState.Raw)
            {
                // 1. CEK KONDISI KOMPOR
                if (!_isStoveOn)
                {
                    // Kompor MATI: Matikan semua efek
                    ShowWarningWithDOTween(_warningOnStoveGas);
                    if (_foodFumesParticle.isPlaying) _foodFumesParticle.Stop();
                    AudioManager.Instance.StopSFX("MasakAudio");

                    // TUNGGU sampai dinyalakan kembali
                    yield return new WaitUntil(() => _isStoveOn);
                }

                // 2. JIKA SUDAH NYALA (Atau baru dinyalakan kembali)
                HideWarningWithDOTween(_warningOnStoveGas);

                // Mainkan suara hanya jika belum bunyi (agar tidak pecah/berulang dari awal)
                if (!AudioManager.Instance.IsPlaying("MasakAudio"))
                {
                    AudioManager.Instance.PlaySFX("MasakAudio");
                }

                if (!_foodFumesParticle.isPlaying) _foodFumesParticle.Play();

                // 3. PROSES TIMER
                if (!_cookingTimer._isRunning) _cookingTimer.StartTimer(999f);

                _elapsedCookingTime += Time.deltaTime;
                _cookingTimer.UpdateTimer(Time.deltaTime);
                _canvasManager.UpdateUITimer(_cookingTimer.GetTimeLeft());

                yield return null;
            }

            // Keluar dari Loop (Makanan matang/diangkat)
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            AudioManager.Instance.StopSFX("MasakAudio");
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
                    _canvasManager.UpdateUITimer(_elapsedCookingTime);

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
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            _cookingTimer.StopTimer(); // Pastikan StopTimer di script Timer-mu tidak memaksa UI ke 0
            _tombolAngkat.gameObject.SetActive(false);
            HideWarningWithDOTween(_warningOnStoveGas);

            // JANGAN RESET _elapsedCookingTime = 0 di sini! 
            // Biarkan dia menyimpan angka terakhir sampai ada makanan baru yang masuk.
        }

        // --- UTILITIES ---

        private void UpdateVisualIngredient()
        {
            float _sebaranRadius = 0.08f;
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

        //animasi
        // Tambahkan parameter CanvasGroup pada fungsi yang sudah ada
        private void ShowWarningWithDOTween(CanvasGroup targetCG)
        {
            if (targetCG == null) return;
            if (targetCG.gameObject.activeSelf) return;

            targetCG.gameObject.SetActive(true);
            targetCG.alpha = 0;
            targetCG.DOKill();

            targetCG.DOFade(1f, 0.5f)
                .From(0.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void HideWarningWithDOTween(CanvasGroup targetCG)
        {
            if (targetCG == null || !targetCG.gameObject.activeSelf) return;

            targetCG.DOKill();
            targetCG.DOFade(0f, 0.3f).OnComplete(() => {
                targetCG.gameObject.SetActive(false);
            });
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



        public void ClearStation()
{
    _ingredientsInContainer.Clear();

    // Hapus semua UI Checklist
    foreach (var ui in _activeChecklistUI.Values)
    {
        Destroy(ui);
    }
    _activeChecklistUI.Clear();

    // Hapus visual gundukan
    foreach (Transform child in _gundukanSpawnPoint)
    {
        Destroy(child.gameObject);
    }

    _tombolAduk.gameObject.SetActive(false);
}
    }
}