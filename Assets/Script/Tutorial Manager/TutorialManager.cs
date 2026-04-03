using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CookOrPanic.TutorialManager
{
    using CookOrPanic.Food;
    using CookOrPanic.Ingredient;
    using CookOrPanic.Panel;
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.SocketController;
    using CookOrPanic.ReturnToSender;
 

    [Serializable]
    public class IngredientUI
    {
        // Menggunakan Enum dari namespace CookOrPanic.Ingredient
        public IngredientType ingredientType;
        public GameObject checkmark;
        [HideInInspector] public bool isCollected = false;
    }

    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;

        public enum TutorialStep { Panduan, Resep, AmbilBahan,NyalakanKompor, MasakMakanan, MatikanKompor, AmbilPiring, HidangkanMakanan, SimpanKenampan, SajikanMakanan, TekanBel, SimpanNampanBalik, AmbilIkan, Penggilingan, MatikanPenggiling, CuciPiring}
         

        [Header("Current Progress")]
        public TutorialStep currentStep = TutorialStep.Panduan;

        [Header("UI & Visuals")]
        [SerializeField] private GameObject _arrowIndicatorBook;
        [SerializeField] private GameObject _arrowIndicatorIngredient;
        [SerializeField] private GameObject _arrowIndicatorCutFood;
        [SerializeField] private GameObject _arrowIndicatorPlate;


        [Header("Instruksi UI")]
        [SerializeField] private GameObject _instruksiPanduan; // Drag Image "Klik Panduan"
        [SerializeField] private GameObject _instruksiResep;   // Drag Image "Klik Resep"
        [SerializeField] private GameObject _panelLanjut;
        [SerializeField] private GameObject _panelMasak;
        [SerializeField] private GameObject _panelCheckList;
        [SerializeField] private GameObject _infoTimer;
        [SerializeField] private GameObject _PanelNyalakanKompor;
        [SerializeField] private GameObject _instruksiNyalakanKompor;
        [SerializeField] private GameObject _instruksiMatikanKompor; 
        [SerializeField] private GameObject _instruksiAmbilPiring;
        [SerializeField] private GameObject _instruksiHidangkan;
        [SerializeField] private GameObject _panelNampan;
        [SerializeField] private GameObject _instruksiMejaSaji;
        [SerializeField] private GameObject _instruksiTekanBel; // Drag objek instruksi bel di Inspector
        [SerializeField] private GameObject _instruksiKembalikanNampan;
        [SerializeField] private GameObject _instruksiAmbilIkan;
        [SerializeField] private GameObject _instruksiPenggilingan;
        [SerializeField] private GameObject _instruksiTekanTombolGiling;
        [SerializeField] private GameObject _instruksiMatikanPenggiling;
        [SerializeField] private GameObject _instruksiCuciPiring;
        [SerializeField] private GameObject _panelSelesaiFinal;
       

        [Header("Socket")]
        [SerializeField] private SocketController _foodSocket;
        [SerializeField] private SocketController _oilSocket;
        [SerializeField] private List<SocketController> _plateSocket = new List<SocketController>();
        [SerializeField] private SocketController _nampanSocket;
        [SerializeField] private SocketController _WindowSocket;
        [SerializeField] private SocketController _kembaliNampan;
        [SerializeField] private SocketController _grinderSocket;


        [Header("Checklist System")]
        // Ini adalah LIST yang kamu minta
        public List<IngredientUI> ingredientChecklist = new List<IngredientUI>();


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {

            // Panggil UpdateStepUI di awal agar tampilan sinkron dengan step 'Panduan'
            UpdateStepUI();
            if (_foodSocket != null)
            {
                _foodSocket.OnIngredientEntered += HandleFoodPlaced;
                Debug.Log("<color=green>TutorialManager:</color> Berhasil berlangganan ke Socket!");
            }
            else
            {
                Debug.LogError("<color=red>TutorialManager:</color> _foodSocket KOSONG di Inspector!");
            }
            // Matikan semua centang di awal secara otomatis agar tidak ada kesalahan manual
            foreach (IngredientUI item in ingredientChecklist)
            {
                if (item.checkmark != null) item.checkmark.SetActive(false);
                item.isCollected = false;
            }

            // Subscribe Socket Minyak
            if (_oilSocket != null)
            {
                _oilSocket.OnFoodEntered += HandleOilPlaced; // Menggunakan OnFoodEntered sesuai script CookingStation kamu
            }

            foreach (SocketController socket in _plateSocket)
            {
                if (socket != null)
                {
                    socket.OnFoodEntered += HandleFoodPlated;
                }
            }
            // Subscribe Socket Nampan (Piring masuk ke Nampan)
            if (_nampanSocket != null)
            {
                _nampanSocket.OnObjectEntered += HandlePlateInTray;
            }

            if (_kembaliNampan != null)
            {
                _kembaliNampan.OnObjectEntered += OnNampanKembali;
                // Matikan socket di awal tutorial
                _kembaliNampan.enabled = false;
            }


            if (_grinderSocket != null)
            {
                // Gunakan OnObjectEntered karena ikan biasanya adalah GameObject biasa sebelum diproses
                _grinderSocket.OnObjectEntered += HandleFishInGrinder;
            }

            if (_WindowSocket != null)
            {
                _WindowSocket.OnObjectEntered += HandleNampanDiSaji;
            }

        }

        // Fungsi yang dipanggil saat buku/panel dibuka
        public void OnBookOpened(Panel.PanelType type)
        {
            if (currentStep == TutorialStep.Panduan && type == Panel.PanelType.PanelPanduan)
            {
                currentStep = TutorialStep.Resep;
                UpdateStepUI(); // <--- PENTING: Update tampilan setelah pindah step
            }
            else if (currentStep == TutorialStep.Resep && type == Panel.PanelType.PanelResep)
            {
                currentStep = TutorialStep.AmbilBahan;
                UpdateStepUI(); // <--- PENTING: Update tampilan setelah pindah step
                Debug.Log("Lanjut ke step selanjutnya: Ambil Bahan!");
            }
        }

        // --- LOGIKA AMBIL MAKANAN (Pemicu Arrow Cut Food) ---
        public void OnFoodGrabbed(SelectEnterEventArgs args)
        {
            if (currentStep == TutorialStep.AmbilBahan)
            {
                // Arrow bahan mati, Arrow talenan (CutFood) NYALA
                HideUI(_arrowIndicatorIngredient);
                _arrowIndicatorCutFood.SetActive(true);
                _panelCheckList.SetActive(true); 
            }
        }

        // --- LOGIKA TOMBOL (Hanya untuk menutup UI) ---
        public void OnButtonLanjutClicked()
        {
            if (currentStep == TutorialStep.AmbilBahan)
            {
                if (_panelLanjut != null)
                {
                    // Mengambil script Panel dengan tipe data eksplisit
                    Panel pScript = _panelLanjut.GetComponent<Panel>();

                    if (pScript != null) pScript.HidePanel();
                    else HideUI(_panelLanjut);
                }
            }
            HideUI(_instruksiResep);
            HideUI(_arrowIndicatorBook);
            HideUI(_arrowIndicatorIngredient);

        }
        // --- FUNGSI TRIGGER UTAMA: Dipanggil saat bahan masuk ke Socket ---
        private void HandleFoodPlaced(ProcessedIngredient ingredient)
        {
            if (currentStep != TutorialStep.AmbilBahan) return;

            IngredientType incomingType = ingredient.ingredientType;

            // 1. Reset status lengkap ke true sebelum pengecekan
            bool allIngredientsNowDone = true;

            // 2. Loop Checklist
            foreach (IngredientUI item in ingredientChecklist)
            {
                if (item.ingredientType == incomingType)
                {
                    item.isCollected = true;
                    if (item.checkmark != null)
                    {
                        item.checkmark.SetActive(true);
                        item.checkmark.transform.localScale = Vector3.zero;
                        item.checkmark.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    }
                   
                }

                // Jika ada satu saja yang belum terkumpul, tandai belum lengkap
                if (item.isCollected == false)
                {
                    allIngredientsNowDone = false;
                }
                
            }

            // 3. LOGIKA UTAMA: Matikan Panel Checklist HANYA jika semua sudah terkumpul
            // Ini akan sinkron dengan munculnya tombol di CookingStation
            if (allIngredientsNowDone)
            {
                if (_panelCheckList != null)
                {
                   
                    // Opsi 2: Hilang halus dengan DOTween (Jika ada CanvasGroup)
                    CanvasGroup cg = _panelCheckList.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.DOFade(0f, 0.5f).OnComplete(() => _panelCheckList.SetActive(false));
                    }
                    else
                    {
                        HideUI(_panelCheckList);
                    }
                }
                HideUI(_arrowIndicatorCutFood);

                Debug.Log("<color=cyan>Tutorial:</color> Checklist selesai dan disembunyikan.");
            }
        }

        // Tambahkan fungsi ini di dalam class TutorialManager
        public void FinishAmbilBahanStep()
        {
            // Cek apakah pemain baru saja menyelesaikan pengumpulan bahan
            if (currentStep == TutorialStep.AmbilBahan)
            {
                // 1. Pindah ke step Nyalakan Kompor (atau MasakMakanan sesuai case di UpdateStepUI)
                currentStep = TutorialStep.NyalakanKompor;

                // 2. Bersihkan UI tahap sebelumnya
                if (_panelCheckList != null) HideUI(_panelCheckList);
                if (_panelLanjut != null) HideUI(_panelLanjut);
                HideUI(_arrowIndicatorCutFood);

                // 3. Update UI untuk memunculkan instruksi kompor
                UpdateStepUI();

                Debug.Log("<color=yellow>Tutorial:</color> Bahan selesai diaduk, lanjut Nyalakan Kompor!");
            }
        }

        // Fungsi Baru untuk Menghilangkan Panel Masak
        private void HandleOilPlaced(Food food, GameObject obj)
        {
            // Hanya jalan jika sedang di tahap Masak
            if (currentStep == TutorialStep.MasakMakanan && _panelMasak != null)
            {
                CanvasGroup cg = _panelMasak.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    // Animasi Fade Out
                    cg.DOFade(0f, 0.5f).OnComplete(() =>
                    {
                        HideUI(_panelMasak);
                    });
                }
                else
                {
                    // Animasi Scale Down jika tidak ada CanvasGroup
                    _panelMasak.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack).OnComplete(() =>
                    {
                        HideUI(_panelMasak);
                    });
                }

               
            }
        }
        // Panggil fungsi ini melalui Event di tombol kompor (On Click atau On Select)
        public void OnStoveTurnedOn()
        {
            if (currentStep == TutorialStep.NyalakanKompor)
            {
              
                HideUI(_instruksiNyalakanKompor);
                HideUI(_PanelNyalakanKompor);
                

                // 2. Pindah ke step memasak
                currentStep = TutorialStep.MasakMakanan;
                UpdateStepUI();

                Debug.Log("<color=green>Tutorial:</color> Kompor menyala! Sekarang masukkan bahan ke penggorengan.");
            }
        }

        public void OnFoodLifted()
        {
            if (currentStep == TutorialStep.MasakMakanan)
            {
                currentStep = TutorialStep.MatikanKompor;

                UpdateStepUI();
                Debug.Log("<color=yellow>Tutorial:</color> Makanan diangkat! Sekarang matikan kompor.");
            }
        }

        public void OnStoveTurnedOff()
        {
            // Hanya lanjut jika step-nya sedang menunggu kompor dimatikan
            if (currentStep == TutorialStep.MatikanKompor)
            {
                // Matikan UI instruksi matikan kompor
                if (_instruksiMatikanKompor != null)
                {
                    HideUI(_instruksiMatikanKompor);
                }

                HideUI(_infoTimer);

                // Pindah ke step ambil piring
                currentStep = TutorialStep.AmbilPiring;
                UpdateStepUI();

                Debug.Log("<color=green>Tutorial:</color> Kompor mati! Lanjut ambil piring.");
            }
        }

        // Panggil fungsi ini via Event di XR Grab Interactable Piring (On Select Entered)
        public void OnPlateGrabbed(SelectEnterEventArgs args)
        {
            if (currentStep == TutorialStep.AmbilPiring)
            {
                // Pindah ke step menghidangkan (menaruh makanan ke piring)
                currentStep = TutorialStep.HidangkanMakanan;

                UpdateStepUI();
                Debug.Log("<color=green>Tutorial:</color> Piring diambil! Taruh makanan ke atasnya.");
            }
        }
        private void HandleFoodPlated(Food food, GameObject obj)
        {
            if (currentStep == TutorialStep.HidangkanMakanan)
            {
                // Pindah ke step "Simpan Ke Nampan"
                currentStep = TutorialStep.SimpanKenampan;

                if (_instruksiHidangkan != null) HideUI(_instruksiHidangkan);

                // Munculkan panel/instruksi agar pemain tahu harus menaruh piring ke nampan
                if (_panelNampan != null)
                {
                    _panelNampan.SetActive(true);
                    _panelNampan.transform.localScale = Vector3.zero;
                    _panelNampan.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutElastic);
                }

                UpdateStepUI();
                Debug.Log("<color=yellow>Tutorial:</color> Piring siap, sekarang taruh ke Nampan!");
            }
        }
        private void HandlePlateInTray(GameObject obj)
        {
            Debug.Log("Detector: Objek masuk ke nampan!"); // Cek apakah terpanggil

            if (currentStep == TutorialStep.SimpanKenampan)
            {
                currentStep = TutorialStep.SajikanMakanan;

                if (_panelNampan != null)
                {
                    _panelNampan.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => _panelNampan.SetActive(false));
                }

                UpdateStepUI(); // Ini yang akan memunculkan _instruksiMejaSaji
                Debug.Log("<color=green>Tutorial:</color> Step berubah ke Sajikan Makanan!");
            }
        }

        // Pastikan WindowSocket sudah di-subscribe di Start()
        // _WindowSocket.OnObjectEntered += HandleNampanDiSaji;

        private void HandleNampanDiSaji(GameObject obj)
        {
            if (currentStep == TutorialStep.SajikanMakanan)
            {
                // Matikan instruksi taruh nampan
                if (_instruksiMejaSaji != null) HideUI(_instruksiMejaSaji);

                // Pindah ke step Tekan Bel
                currentStep = TutorialStep.TekanBel;
                UpdateStepUI();

                Debug.Log("<color=yellow>Tutorial:</color> Nampan sampai! Sekarang tekan bel.");
            }
        }


        // Tambahkan fungsi ini di dalam class TutorialManager
        public void OnBellPressedDuringTutorial()
        {
            // Cek apakah pemain sedang di tahap menekan bel
            if (currentStep == TutorialStep.TekanBel)
            {
                // 1. Matikan instruksi bel
                if (_instruksiTekanBel != null)
                {
                    _instruksiTekanBel.transform.DOScale(Vector3.zero, 0.3f)
                        .OnComplete(() => _instruksiTekanBel.SetActive(false));
                }

                // 2. Aktifkan socket pengembalian
                if (_kembaliNampan != null)
                {
                    _kembaliNampan.enabled = true;
                }

                // 3. Pindah ke step kembalikan nampan
                currentStep = TutorialStep.SimpanNampanBalik;
                UpdateStepUI();
            }
        }


        public void OnNampanKembali(GameObject obj)
        {
            // Pastikan stepnya pas
            if (currentStep == TutorialStep.SimpanNampanBalik)
            {
                Debug.Log("<color=green>Tutorial:</color> Nampan terdeteksi di Rak Pengembalian!");

                // Matikan instruksi kembalikan nampan
                if (_instruksiKembalikanNampan != null)
                {
                    _instruksiKembalikanNampan.transform.DOScale(Vector3.zero, 0.3f)
                        .OnComplete(() => _instruksiKembalikanNampan.SetActive(false));
                }

                // LANJUT KE STEP AMBIL IKAN
                currentStep = TutorialStep.AmbilIkan;
                UpdateStepUI();

                // Nonaktifkan socket ini sementara agar tidak trigger berkali-kali selama tutorial
                if (_kembaliNampan != null) _kembaliNampan.enabled = false;
            }
        }

        // Panggil fungsi ini melalui Event di XR Grab Interactable Ikan
        public void OnFishGrabbed(SelectEnterEventArgs args)
        {
            // Cek apakah pemain memang sedang di tahap Ambil Ikan
            if (currentStep == TutorialStep.AmbilIkan)
            {
                // 1. Matikan indikator ikan yang lama
                if (_instruksiAmbilIkan != null) HideUI(_instruksiAmbilIkan);
               
                // 2. Langsung pindah ke step Penggilingan
                currentStep = TutorialStep.Penggilingan;

                // 3. Update UI untuk memunculkan instruksi giling
                UpdateStepUI();

                Debug.Log("<color=yellow>Tutorial:</color> Ikan diambil! Sekarang lanjut ke Penggilingan.");
            }
        }

        private void HandleFishInGrinder(GameObject obj)
        {
            // Cek apakah ikan yang masuk sesuai (opsional, bisa pakai Tag atau Script)
            if (currentStep == TutorialStep.Penggilingan)
            {
                // 1. Matikan instruksi "Bawa ke Penggilingan"
                if (_instruksiPenggilingan != null)
                {
                    HideUI(_instruksiPenggilingan);
                }

                // 2. Munculkan instruksi "Tekan Tombol"
                if (_instruksiTekanTombolGiling != null)
                {
                    _instruksiTekanTombolGiling.SetActive(true);
                    CanvasGroup targetCG = _instruksiTekanTombolGiling.GetComponent<CanvasGroup>();
                    if (targetCG == null) targetCG = _instruksiTekanTombolGiling.AddComponent<CanvasGroup>();

                    // Berhentikan animasi sebelumnya agar tidak bertumpuk (Stacking)
                    targetCG.DOKill();

                    // Jalankan animasi kedip (Yoyo)
                    targetCG.DOFade(1f, 0.5f)
                        .From(0.2f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);


                }

                Debug.Log("<color=cyan>Tutorial:</color> Ikan di penggilingan, instruksi tombol muncul.");
            }
        }

        public void OnGrinderButtonPressed()
        {
            // Step ini dipanggil saat pemain menekan tombol NYALAKAN
            if (currentStep == TutorialStep.Penggilingan)
            {
                if (_instruksiTekanTombolGiling != null)
                {
                    _instruksiTekanTombolGiling.transform.DOKill(true);
                    _instruksiTekanTombolGiling.SetActive(false);
                }

                // JANGAN pindah step di sini jika ingin menunggu prefab muncul dulu
                // Atau biarkan saja jika kamu ingin instruksi "Tunggu Proses" muncul
            }
        }

        // Tambahkan fungsi baru untuk dipanggil saat prefab muncul
        public void OnGrinderResultSpawned()
        {
            if (currentStep == TutorialStep.Penggilingan)
            {
                currentStep = TutorialStep.MatikanPenggiling;
                UpdateStepUI();
                Debug.Log("<color=cyan>Tutorial:</color> Prefab muncul! Instruksi MATIKAN diaktifkan.");
            }
        }
        public void OnGrinderTurnedOff()
        {
            if (currentStep == TutorialStep.MatikanPenggiling)
            {
                // Pindah ke step cuci piring setelah mesin mati
                currentStep = TutorialStep.CuciPiring;
                UpdateStepUI();
                Debug.Log("<color=green>Tutorial:</color> Mesin mati! Sekarang waktunya cuci piring.");
            }
        }


        // Panggil fungsi ini melalui event/script saat piring selesai dicuci
        public void OnDishWashed()
        {
            if (currentStep == TutorialStep.CuciPiring)
            {
                HideUI(_instruksiCuciPiring);

                // 2. Aktifkan Panel Selesai Final
                if (_panelSelesaiFinal != null)
                {
                    _panelSelesaiFinal.SetActive(true);
                    _panelSelesaiFinal.transform.localScale = Vector3.zero;
                    _panelSelesaiFinal.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                }

                Debug.Log("<color=green>Tutorial:</color> Cuci piring selesai, Tutorial Tamat!");
            }
        }

        // Jika ada socket lagi di Meja Saji, kamu bisa buat fungsi serupa:
        public void FinishTutorial()
        {
            if (currentStep == TutorialStep.SajikanMakanan)
            {
                HideUI(_instruksiMejaSaji);
                _panelSelesaiFinal.SetActive(true);
                    
                Debug.Log("<color=green>Tutorial:</color> Selesai!");
            }
        }

        private void UpdateStepUI()
        {
            
            switch (currentStep)
            {
                case TutorialStep.Panduan:
                    _instruksiPanduan.SetActive(true);
                    ShowUI(_arrowIndicatorBook);
                    break;

                case TutorialStep.Resep:
                    _instruksiResep.SetActive(true);
                    ShowUI(_arrowIndicatorBook);
                    break;

                case TutorialStep.AmbilBahan:
                    ShowUI(_arrowIndicatorIngredient);
                    ShowUI(_panelLanjut);
                    HideUI(_arrowIndicatorBook);
                    break;

                case TutorialStep.NyalakanKompor:   
                    if (_instruksiNyalakanKompor != null)
                    {
                        _instruksiNyalakanKompor.SetActive(true);
                        // Ambil atau tambahkan CanvasGroup secara otomatis
                        CanvasGroup targetCG = _instruksiNyalakanKompor.GetComponent<CanvasGroup>();
                        if (targetCG == null) targetCG = _instruksiNyalakanKompor.AddComponent<CanvasGroup>();

                        // Berhentikan animasi sebelumnya agar tidak bertumpuk (Stacking)
                        targetCG.DOKill();

                        // Jalankan animasi kedip (Yoyo)
                        targetCG.DOFade(1f, 0.5f)
                            .From(0.2f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                    }
                    ShowUI(_PanelNyalakanKompor);
                    break;

                case TutorialStep.MasakMakanan:
                    // Pastikan panel instruksi sebelumnya mati
                    HideUI(_instruksiResep);
                    HideUI(_panelLanjut);
                    ShowUI(_panelMasak);
                    ShowUI(_infoTimer);
                    break;

                case TutorialStep.MatikanKompor:
                    if (_instruksiMatikanKompor != null)
                    {
                        _instruksiMatikanKompor.SetActive(true);
                        // Ambil atau tambahkan CanvasGroup secara otomatis
                        CanvasGroup targetCG = _instruksiMatikanKompor.GetComponent<CanvasGroup>();
                        if (targetCG == null) targetCG = _instruksiMatikanKompor.AddComponent<CanvasGroup>();

                        // Berhentikan animasi sebelumnya agar tidak bertumpuk (Stacking)
                        targetCG.DOKill();

                        // Jalankan animasi kedip (Yoyo)
                        targetCG.DOFade(1f, 0.5f)
                            .From(0.2f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);

                    }
                    break;


                case TutorialStep.AmbilPiring:
                    _arrowIndicatorPlate.SetActive(true); 
                    if (_instruksiAmbilPiring != null)
                    {
                        ShowUI(_instruksiAmbilPiring);
                        
                    }
                    break;

                case TutorialStep.HidangkanMakanan:
                    HideUI(_instruksiAmbilPiring);
                    HideUI(_arrowIndicatorPlate);
                    ShowUI(_instruksiHidangkan);
                    break;

                case TutorialStep.SimpanKenampan:
                    ShowUI(_panelNampan);
                    break;


                case TutorialStep.SajikanMakanan:
                    ShowUI(_instruksiMejaSaji);
                    break;

                case TutorialStep.TekanBel:
                    if (_instruksiTekanBel != null)
                    {
                        ShowUI(_instruksiTekanBel);
                        
                    }
                    break;

                case TutorialStep.SimpanNampanBalik:
                    if (_instruksiKembalikanNampan != null)
                    {
                        ShowUI(_instruksiKembalikanNampan);
                       
                    }
                    break;

                case TutorialStep.AmbilIkan:
                    if (_instruksiAmbilIkan != null)
                    {
                        ShowUI(_instruksiAmbilIkan);
                        
                    }

                    // Pastikan panah bahan sebelumnya mati
                     HideUI(_arrowIndicatorIngredient);
                    break;

                case TutorialStep.Penggilingan:
                    if (_instruksiPenggilingan != null)
                    {
                        ShowUI(_instruksiPenggilingan);
                    }
                    break;

                case TutorialStep.MatikanPenggiling:
                    _instruksiMatikanPenggiling.SetActive(true);
                    // Ambil atau tambahkan CanvasGroup secara otomatis
                    CanvasGroup target = _instruksiMatikanPenggiling.GetComponent<CanvasGroup>();
                    if (target == null) target = _instruksiMatikanPenggiling.AddComponent<CanvasGroup>();

                    // Berhentikan animasi sebelumnya agar tidak bertumpuk (Stacking)
                    target.DOKill();

                    // Jalankan animasi kedip (Yoyo)
                    target.DOFade(1f, 0.5f)
                        .From(0.2f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                    break;

                case TutorialStep.CuciPiring:
                    // Matikan instruksi penggilingan jika masih ada
                    HideUI(_instruksiPenggilingan);
                    HideUI(_instruksiMatikanPenggiling);
                    // Nyalakan instruksi cuci piring
                    if (_instruksiCuciPiring != null)
                    {
                        ShowUI(_instruksiCuciPiring);
             
                    }
                    break;
            }
        }

        // ================== UNIVERSAL UI ANIMATION ==================

        private void ShowUI(GameObject ui)
        {
            if (ui == null) return;

            ui.SetActive(true);

            CanvasGroup cg = ui.GetComponent<CanvasGroup>();
            if (cg == null) cg = ui.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            ui.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            cg.DOKill();
            ui.transform.DOKill();

            cg.DOFade(1f, 0.25f);
            ui.transform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic);
        }

        private void HideUI(GameObject ui)
        {
            if (ui == null) return;

            CanvasGroup cg = ui.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                ui.SetActive(false);
                return;
            }

            cg.DOKill();
            ui.transform.DOKill();

            cg.DOFade(0f, 0.2f);
            ui.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InCubic)
                .OnComplete(() => ui.SetActive(false));
        }
    }
}
