using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using UnityEngine.Events;

namespace CookOrPanic.CookingStation
{
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Recipe;
    using CookOrPanic.Food; 
    using CookOrPanic.Timer;
    using CookOrPanic.CanvasManager;
    using CookOrPanic.SocketController;

    public class CookingStation : MonoBehaviour
    {
        [Header("Tutorial Events")]
        public UnityEvent OnAdukBahanClicked;

        [Header("Station References")]
        [SerializeField] private List<Recipe> _availableRecipe;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Button _tombolAduk;
        [SerializeField] private Transform _gundukanSpawnPoint;

        [Header("Cooking References")]
        [SerializeField] private Button _tombolAngkat;
        [SerializeField] private ParticleSystem _foodFumesParticle;
        [SerializeField] private ParticleSystem _mixingParticle; // Drag particle tepung ke sini
        

        [Header("Station Socket Controllers")]
        [SerializeField] private SocketController _singleSocket; // Ganti tipe data
        [SerializeField] private SocketController _oilSocket;    // Ganti tipe data
        [SerializeField] private SocketController _drainSocket;  // Ganti tipe data

        [SerializeField] private Timer _cookingTimer;
        [SerializeField] private CanvasManager _canvasManager;
        private float _elapsedCookingTime;
        private bool _isStoveOn = false;

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
        [SerializeField] private CanvasGroup _warningCanvasGroup;


        private void Start()
        {
            if (_tombolAduk != null) _tombolAduk.gameObject.SetActive(false);
            if (_tombolAngkat != null) _tombolAngkat.gameObject.SetActive(false);
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            if(_mixingParticle != null) _mixingParticle.Stop();

            // Berlangganan ke event dari Controller
            if (_singleSocket != null)
                _singleSocket.OnIngredientEntered += OnIngredientEntered;

            if (_oilSocket != null)
            {
                _oilSocket.OnFoodEntered += OnFoodPlacedInOil;
            }
        }

        // --- LOGIKA ADUK BAHAN ---
        // --- LOGIKA ADUK BAHAN ---
        private void OnIngredientEntered(ProcessedIngredient ingredient)
        {
            // 1. TAMBAHKAN KE LIST DULU (PENTING!)
            _ingredientsInContainer.Add(new IngredientData
            {
                type = ingredient.GetIngredientType(),
                state = ingredient.GetState()
            });

            // 2. BARU CEK APAKAH SUDAH LENGKAP
            if (_isTutorialMode)
            {
                if (ValidateInternal(_tutorialRecipe))
                {
                    _tombolAduk.gameObject.SetActive(true);
                    _tombolAduk.transform.localScale = Vector3.zero;
                    _tombolAduk.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    Debug.Log("<color=cyan>Tutorial:</color> Bahan lengkap, tombol muncul!");
                }
            }
            else
            {
                _tombolAduk.gameObject.SetActive(true);
                _tombolAduk.transform.localScale = Vector3.zero;
                _tombolAduk.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }

            UpdateVisualIngredient();
            Destroy(ingredient.gameObject);
        }

        public void TryCook()
        {
            OnAdukBahanClicked?.Invoke();
            foreach (Recipe recipe in _availableRecipe)
            {
                if (ValidateInternal(recipe))
                {
                    StartCoroutine(MixingProcessRoutine(recipe));
                    return;
                }
            }
            Debug.LogError("[Gagal] Bahan tidak sesuai resep!");
        }

        //logika masak dengan minyak, bisa dipanggil dari tombol angkat

        public void ToggleStove()
        {
            // Membalikkan nilai _isStoveOn (jika true jadi false, jika false jadi true)
            _isStoveOn = !_isStoveOn;

            Debug.Log("Kompor sekarang: " + (_isStoveOn ? "NYALA" : "MATI"));

        }

        // --- LOGIKA MASAK (Sekarang menerima Food langsung) ---
        private void OnFoodPlacedInOil(Food foodScript, GameObject foodObject)
        {
            if (foodScript._foodState == FoodState.Raw)
            {
                Debug.Log("Makanan masuk, menjalankan CookingRoutine...");
                StartCoroutine(CookingRoutine(foodObject, foodScript));
            }
        }

        private IEnumerator CookingRoutine(GameObject foodObject, Food foodScript)
        {
            Debug.Log("CookingRoutine dimulai, menunggu kompor...");
            _tombolAngkat.gameObject.SetActive(true);

            // Loop ini akan terus berjalan selama objek ada di dalam panci
            while (foodScript != null && foodScript._foodState == FoodState.Raw)
            {
                if (!_isStoveOn)
                {
                    ShowWarningWithDOTween();
                    if (_foodFumesParticle != null && _foodFumesParticle.isPlaying)
                        _foodFumesParticle.Stop();
                }

                // PENTING: Tunggu kompor nyala DI SINI, sebelum timer apa pun diproses
                yield return new WaitUntil(() => _isStoveOn);
                HideWarningWithDOTween();

                // Kode di bawah ini HANYA akan dijalankan jika _isStoveOn == true
                Debug.Log("Kompor menyala, proses masak dimulai/dilanjutkan!");

                if (!_cookingTimer._isRunning)
                    _cookingTimer.StartTimer(999f);

                if (_foodFumesParticle != null && !_foodFumesParticle.isPlaying)
                    _foodFumesParticle.Play();

                _elapsedCookingTime += Time.deltaTime;
                _cookingTimer.UpdateTimer(Time.deltaTime);
                _canvasManager.UpdateUITimer(_cookingTimer.GetTimeLeft());

                yield return null; // Tunggu frame berikutnya
            }

            // Pastikan partikel berhenti saat keluar dari loop (makanan diangkat)
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
        }

        public void ActionAngkatMakanan()
        {
            StopAllCoroutines();
          
            if (_foodFumesParticle != null) _foodFumesParticle.Stop();
            _cookingTimer.StopTimer();
            _tombolAngkat.gameObject.SetActive(false);

            List<IXRSelectInteractable> selected = _oilSocket.interactablesSelected;
            if (selected.Count > 0)
            {
                IXRSelectInteractable foodInteractable = selected[0];
                Renderer[] renderers = foodInteractable.transform.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers) r.enabled = true;

                if (foodInteractable.transform.TryGetComponent(out Food foodScript))
                {
                    // 2. KIRIM DURASI KE FOOD
                    foodScript._timeSpentCooking = _elapsedCookingTime;

                    // 3. PERINTAHKAN FOOD UNTUK MENENTUKAN STATUSNYA SENDIRI
                    foodScript.UpdateStateBasedOnTime(_elapsedCookingTime);

                    Debug.Log($"[CookingStation] Makanan diangkat. Waktu: {_elapsedCookingTime}s");
                }

                // --- BAGIAN YANG DIUBAH ---
                // Kita gunakan _oilSocket.interactionManager untuk mengeluarkan benda
                // Tapi parameternya harus berupa XRSocketInteractor asli, yaitu _oilSocket.Socket
                _oilSocket.interactionManager.SelectExit(_oilSocket.Socket, foodInteractable);

                if (_drainSocket != null)
                {
                    // Masukkan ke socket penirisan (drain) menggunakan Socket aslinya
                    _drainSocket.interactionManager.SelectEnter(_drainSocket.Socket, foodInteractable);
                }

            }
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

            // Jika jumlah bahan belum pas, langsung false
            if (_ingredientsInContainer.Count != totalNeeded) return false;

            foreach (var req in recipe._requirements)
            {
                int matchCount = 0;
                foreach (var input in _ingredientsInContainer)
                {
                    if (input.type == req._ingredientType && input.state == req._requiredState)
                        matchCount++;
                }
                if (matchCount != req._requiredAmount) return false;
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
        private void ShowWarningWithDOTween()
        {
            if (_warningCanvasGroup == null) return;

            // Jika sudah aktif, jangan panggil lagi agar tidak tumpang tindih
            if (_warningCanvasGroup.gameObject.activeSelf) return;

            _warningCanvasGroup.gameObject.SetActive(true);

            // --- PERBAIKAN: JANGAN ubah localScale di sini ---
            // Biarkan ukurannya sesuai dengan yang kamu atur di Inspector

            _warningCanvasGroup.alpha = 0;

            _warningCanvasGroup.DOKill();

            // Animasi Alpha bergerak dari 0.2 ke 1 (Kedap-kedip)
            // Ini jauh lebih aman untuk VR karena tidak merubah fisik objek
            _warningCanvasGroup.DOFade(1f, 0.5f)
                .From(0.2f) // Mulai dari agak transparan
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void HideWarningWithDOTween()
        {
            if (_warningCanvasGroup == null || !_warningCanvasGroup.gameObject.activeSelf) return;

            _warningCanvasGroup.DOKill();
            _warningCanvasGroup.DOFade(0f, 0.3f).OnComplete(() => {
                _warningCanvasGroup.gameObject.SetActive(false);
            });
        }

        private IEnumerator MixingProcessRoutine(Recipe recipe)
        {
            float _mixingDuration = 2.0f;
            // 1. Matikan tombol aduk agar tidak diklik dua kali
            if (_tombolAduk != null) _tombolAduk.gameObject.SetActive(false);
            ClearStation();


            // 2. Munculkan Particle Efek Mengadon
            if (_mixingParticle != null)
            {
                _mixingParticle.Play();
            }

            // 3. Tunggu selama beberapa detik (durasi mengaduk)
            yield return new WaitForSeconds(_mixingDuration);

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

            // Hapus semua gundukan SEBELUM melakukan hal lain
            foreach (Transform child in _gundukanSpawnPoint)
            {
                Destroy(child.gameObject);
            }

            _tombolAduk.gameObject.SetActive(false);
        }
    }
}