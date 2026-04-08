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
    using CookOrPanic.UIAnimator;


    [Serializable]
    public class ArrowStepData
    {
        public TutorialManager.TutorialStep step; // Di step mana panah ini muncul
        public GameObject arrowObject;            // Objek panahnya
        public UIAnimator.AnimationType animation = UIAnimator.AnimationType.NavAnimation;
    }

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
        public bool _isTutorialMode = true;
       

        public enum TutorialStep { Panduan, Resep, AmbilBahan, NyalakanKompor, MasakMakanan, MatikanKompor, AmbilPiring, HidangkanMakanan, SimpanKenampan, SajikanMakanan, TekanBel, SimpanNampanBalik, AmbilIkan, Penggilingan, MatikanPenggiling, CuciPiring}
         

        [Header("Current Progress")]
        public TutorialStep currentStep = TutorialStep.Panduan;

        

        [Header("Arrow System")]
        [SerializeField] private List<ArrowStepData> arrowSettings = new List<ArrowStepData>();

        [Header("Instruksi UI")]
        [SerializeField] private GameObject _instruksiPanduan; 
        [SerializeField] private GameObject _instruksiResep;  
        [SerializeField] private GameObject _panelLanjut;
        [SerializeField] private GameObject _instruksiAmbilBahan; 
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
        [SerializeField] private GameObject _instruksiTekanBel;
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
                UIAnimator.Show(_panelCheckList, UIAnimator.AnimationType.Scale);
                UIAnimator.Show(_instruksiAmbilBahan, UIAnimator.AnimationType.Scale);
            }
        }

        // --- LOGIKA TOMBOL (Hanya untuk menutup UI) ---
        public void OnButtonLanjutClicked()
        {
            if (currentStep == TutorialStep.AmbilBahan)
            {
                if (_panelLanjut != null)
                { 
                    UIAnimator.Hide(_panelLanjut, UIAnimator.AnimationType.Scale);
                }
            }
            UIAnimator.Hide(_instruksiResep, UIAnimator.AnimationType.Scale);
         
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
                    UIAnimator.Hide(_panelCheckList, UIAnimator.AnimationType.Scale);
                }
              

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
                UIAnimator.Hide(_panelCheckList, UIAnimator.AnimationType.Scale);
                UIAnimator.Hide(_panelLanjut, UIAnimator.AnimationType.Scale);
               
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
                UIAnimator.Hide(_panelMasak, UIAnimator.AnimationType.Scale);
             
            }
        }
        // Panggil fungsi ini melalui Event di tombol kompor (On Click atau On Select)
        public void OnStoveTurnedOn()
        {
            if (currentStep == TutorialStep.NyalakanKompor)
            {
              
                UIAnimator.Hide(_instruksiNyalakanKompor, UIAnimator.AnimationType.Scale);
                UIAnimator.Hide(_PanelNyalakanKompor, UIAnimator.AnimationType.Scale);

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
                    UIAnimator.Hide(_instruksiMatikanKompor, UIAnimator.AnimationType.Scale);
                }

                UIAnimator.Hide(_infoTimer, UIAnimator.AnimationType.Scale);

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

                if (_instruksiHidangkan != null) UIAnimator.Hide(_instruksiHidangkan, UIAnimator.AnimationType.Scale);

                // Munculkan panel/instruksi agar pemain tahu harus menaruh piring ke nampan
                if (_panelNampan != null)
                {
                    UIAnimator.Show(_panelNampan, UIAnimator.AnimationType.Scale);
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

      
        private void HandleNampanDiSaji(GameObject obj)
        {
            if (currentStep == TutorialStep.SajikanMakanan)
            {
                // Matikan instruksi taruh nampan
                if (_instruksiMejaSaji != null) UIAnimator.Hide(_instruksiMejaSaji, UIAnimator.AnimationType.Scale);

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
                if (_instruksiAmbilIkan != null) UIAnimator.Hide(_instruksiAmbilIkan, UIAnimator.AnimationType.Scale);
               
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
                    UIAnimator.Hide(_instruksiPenggilingan, UIAnimator.AnimationType.Scale);
                }

                // 2. Munculkan instruksi "Tekan Tombol"
                if (_instruksiTekanTombolGiling != null)
                {
                    UIAnimator.Show(_instruksiTekanTombolGiling, UIAnimator.AnimationType.PulseFade);

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
                UIAnimator.Hide(_instruksiCuciPiring, UIAnimator.AnimationType.Scale);

                // 2. Aktifkan Panel Selesai Final
                if (_panelSelesaiFinal != null)
                {
                    UIAnimator.Show(_panelSelesaiFinal, UIAnimator.AnimationType.Scale);
                   
                }

                Debug.Log("<color=green>Tutorial:</color> Cuci piring selesai, Tutorial Tamat!");
            }
        }

        // Jika ada socket lagi di Meja Saji, kamu bisa buat fungsi serupa:
        public void FinishTutorial()
        {
            if (currentStep == TutorialStep.SajikanMakanan)
            {
                UIAnimator.Hide(_instruksiMejaSaji, UIAnimator.AnimationType.Scale);
                _panelSelesaiFinal.SetActive(true);
                    
                Debug.Log("<color=green>Tutorial:</color> Selesai!");
            }
        }

        private void HideAllArrows()
        {
            foreach (var data in arrowSettings)
            {
                if (data.arrowObject != null && data.arrowObject.activeSelf)
                {
                    UIAnimator.Hide(data.arrowObject, data.animation);
                }
            }
        }

        // Fungsi helper untuk menyalakan panah berdasarkan step
        private void ShowArrowForStep(TutorialStep step)
        {
            HideAllArrows(); // Bersihkan dulu agar tidak tumpang tindih

            foreach (var data in arrowSettings)
            {
                if (data.step == step && data.arrowObject != null)
                {
                    UIAnimator.Show(data.arrowObject, data.animation);
                }
            }
        }

        private void UpdateStepUI()
        {
            ShowArrowForStep(currentStep);

            switch (currentStep)
            {
                case TutorialStep.Panduan:
                    _instruksiPanduan.SetActive(true);
                    break;

                case TutorialStep.Resep:
                    _instruksiResep.SetActive(true);
                    break;

                case TutorialStep.AmbilBahan:
                    UIAnimator.Show(_panelLanjut, UIAnimator.AnimationType.Scale);
                    UIAnimator.Show(_panelCheckList, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.NyalakanKompor:
                    UIAnimator.Show(_instruksiNyalakanKompor, UIAnimator.AnimationType.PulseFade);
                    UIAnimator.Show(_PanelNyalakanKompor, UIAnimator.AnimationType.Scale);
                    UIAnimator.Hide(_instruksiAmbilBahan, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.MasakMakanan:
                    UIAnimator.Hide(_instruksiResep, UIAnimator.AnimationType.Scale);
                    UIAnimator.Hide(_panelLanjut, UIAnimator.AnimationType.Scale);
                    UIAnimator.Show(_panelMasak, UIAnimator.AnimationType.Scale);
                    UIAnimator.Show(_infoTimer, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.MatikanKompor:
                    UIAnimator.Show(_instruksiMatikanKompor, UIAnimator.AnimationType.PulseFade);
                    break;


                case TutorialStep.AmbilPiring:
                    UIAnimator.Show(_instruksiAmbilPiring, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.HidangkanMakanan:
                    UIAnimator.Hide(_instruksiAmbilPiring, UIAnimator.AnimationType.Scale);
                    UIAnimator.Show(_instruksiHidangkan, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.SimpanKenampan:
                    UIAnimator.Show(_panelNampan, UIAnimator.AnimationType.Scale);
                    break;


                case TutorialStep.SajikanMakanan:
                    UIAnimator.Show(_instruksiMejaSaji, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.TekanBel:
                    UIAnimator.Show(_instruksiTekanBel, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.SimpanNampanBalik:
                    UIAnimator.Show(_instruksiKembalikanNampan, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.AmbilIkan:
                    UIAnimator.Show(_instruksiAmbilIkan, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.Penggilingan:
                    UIAnimator.Show(_instruksiPenggilingan, UIAnimator.AnimationType.Scale);
                    break;

                case TutorialStep.MatikanPenggiling:
                    UIAnimator.Show(_instruksiMatikanPenggiling, UIAnimator.AnimationType.PulseFade);
                    break;

                case TutorialStep.CuciPiring:
                    UIAnimator.Hide(_instruksiPenggilingan, UIAnimator.AnimationType.Scale);
                    UIAnimator.Hide(_instruksiMatikanPenggiling, UIAnimator.AnimationType.Scale);
                    UIAnimator.Show(_instruksiCuciPiring, UIAnimator.AnimationType.Scale);
                    break;
            }
        }

        // ================== UNIVERSAL UI ANIMATION ==================

       
      
    }
}
