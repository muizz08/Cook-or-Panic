using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit;

namespace CookOrPanic.TutorialManager
{
    using CookOrPanic.Panel;
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.SocketController;
    using CookOrPanic.Ingredient;
    using CookOrPanic.Food;

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

        public enum TutorialStep { Panduan, Resep, AmbilBahan, MasakMakanan, HidangkanMakanan, SimpanKenampan, SajikanMakanan}


        [Header("Current Progress")]
        public TutorialStep currentStep = TutorialStep.Panduan;

        [Header("UI & Visuals")]
        [SerializeField] private GameObject _arrowIndicatorBook;
        [SerializeField] private GameObject _arrowIndicatorIngredient;
        [SerializeField] private GameObject _arrowIndicatorCutFood;


        [Header("Instruksi UI")]
        public GameObject _instruksiPanduan; // Drag Image "Klik Panduan"
        public GameObject _instruksiResep;   // Drag Image "Klik Resep"
        public GameObject _panelLanjut;
        public GameObject _panelMasak;
        public GameObject _panelCheckList;
        public GameObject _infoTimer;
        public GameObject _instruksiHidangkan;
        public GameObject _panelNampan;
        public GameObject _instruksiMejaSaji;
        public GameObject _panelSelesaiFinal;
        [SerializeField] private SocketController _foodSocket;
        [SerializeField] private SocketController _oilSocket;
        [SerializeField] private SocketController _plateSocket;
        [SerializeField] private SocketController _scoreSocket;

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

            if (_plateSocket != null)
            {
                _plateSocket.OnFoodEntered += HandleFoodPlated;
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
                if (_arrowIndicatorIngredient != null) _arrowIndicatorIngredient.SetActive(false);
                if (_arrowIndicatorCutFood != null) _arrowIndicatorCutFood.SetActive(true);
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
                    else _panelLanjut.SetActive(false);
                }
            }
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
                    // Opsi 1: Langsung hilang
                    // _panelCheckList.SetActive(false); 

                    // Opsi 2: Hilang halus dengan DOTween (Jika ada CanvasGroup)
                    CanvasGroup cg = _panelCheckList.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.DOFade(0f, 0.5f).OnComplete(() => _panelCheckList.SetActive(false));
                    }
                    else
                    {
                        _panelCheckList.SetActive(false);
                    }
                }

              
                Debug.Log("<color=cyan>Tutorial:</color> Checklist selesai dan disembunyikan.");
            }
        }

        // Tambahkan fungsi ini di dalam class TutorialManager
        public void FinishAmbilBahanStep()
        {
            if (currentStep == TutorialStep.AmbilBahan)
            {
                // 1. Pindah Step
                currentStep = TutorialStep.MasakMakanan;

                // 2. UI Cleaning: Matikan panel checklist atau panel lanjut jika masih ada
                if (_instruksiResep != null) _instruksiResep.SetActive(false);
                if (_panelLanjut != null) _panelLanjut.SetActive(false);

                // 3. Update instruksi dan nyalakan panel masak via UpdateStepUI
                UpdateStepUI();

                Debug.Log("<color=yellow>Tutorial:</color> Tombol aduk diklik, masuk ke tahap Masak!");
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
                    cg.DOFade(0f, 0.5f).OnComplete(() => {
                        _panelMasak.SetActive(false);
                    });
                }
                else
                {
                    // Animasi Scale Down jika tidak ada CanvasGroup
                    _panelMasak.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack).OnComplete(() => {
                        _panelMasak.SetActive(false);
                    });
                }

                if (_infoTimer != null)
                {
                    // Pastikan objek aktif
                    _infoTimer.SetActive(true);

                    // Ambil CanvasGroup untuk animasi Fade (jika ada)
                    CanvasGroup timerCG = _infoTimer.GetComponent<CanvasGroup>();

                    // Reset keadaan awal untuk animasi
                    _infoTimer.transform.localScale = Vector3.zero; // Mulai dari tidak terlihat
                    if (timerCG != null) timerCG.alpha = 0f;

                    // Jalankan Animasi Muncul (Pop Up)
                    _infoTimer.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

                    if (timerCG != null)
                    {
                        timerCG.DOFade(1f, 0.5f);
                    }

                    Debug.Log("<color=green>Tutorial:</color> Info Timer Muncul!");
                }
            }
        }

        public void FinishMasakStep()
        {
            if (currentStep == TutorialStep.MasakMakanan)
            {
                currentStep = TutorialStep.HidangkanMakanan;

                // 1. Hilangkan UI yang berhubungan dengan memasak
                if (_infoTimer != null)
                {
                    _infoTimer.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => _infoTimer.SetActive(false));
                }

                // 2. Update panah dan instruksi
                UpdateStepUI();

                Debug.Log("<color=orange>Tutorial:</color> Makanan ditiriskan, sekarang hidangkan!");
            }
        }
        private void HandleFoodPlated(Food food, GameObject obj)
        {
            // Hanya jalan jika memang sudah di tahap menghidangkan
            if (currentStep == TutorialStep.HidangkanMakanan)
            {
                currentStep = TutorialStep.SimpanKenampan;

                // 1. Matikan instruksi "Hidangkan"
                if (_instruksiHidangkan != null) _instruksiHidangkan.SetActive(false);

                // 2. Munculkan Panel Selesai
                if (_panelNampan != null)
                {
                    _panelNampan.SetActive(true);

                    // Animasi Pop Up agar keren
                    _panelNampan.transform.localScale = Vector3.zero;
                    _panelNampan.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutElastic);

                    // Opsional: Fade In background jika ada CanvasGroup
                    CanvasGroup cg = _panelNampan.GetComponent<CanvasGroup>();
                    if (cg != null) { cg.alpha = 0; cg.DOFade(1f, 0.4f); }
                }

                Debug.Log("<color=green>Tutorial Selesai!</color> Makanan sudah di piring.");
            }
        }

        private void HandleTrayEnteredScore(Food food, GameObject obj)
        {
            if (currentStep == TutorialStep.SimpanKenampan)
            {
                currentStep = TutorialStep.SajikanMakanan;

                // Matikan panel nampan
                if (_panelNampan != null) _panelNampan.SetActive(false);

                UpdateStepUI();
                Debug.Log("<color=cyan>Tutorial:</color> Nampan siap, sekarang bawa ke Meja Saji!");
            }
        }

        // Jika ada socket lagi di Meja Saji, kamu bisa buat fungsi serupa:
        public void FinishTutorial()
        {
            if (currentStep == TutorialStep.SajikanMakanan)
            {
                if (_instruksiMejaSaji != null) _instruksiMejaSaji.SetActive(false);
                if (_panelSelesaiFinal != null) _panelSelesaiFinal.SetActive(true);

                Debug.Log("<color=green>Tutorial:</color> Selesai!");
            }
        }

        private void UpdateStepUI()
        {
            // Reset semua indikator agar bersih
            if (_instruksiPanduan != null) _instruksiPanduan.SetActive(false);
            if (_instruksiResep != null) _instruksiResep.SetActive(false);
            if (_arrowIndicatorBook != null) _arrowIndicatorBook.SetActive(false);
            if (_arrowIndicatorIngredient != null) _arrowIndicatorIngredient.SetActive(false);
            if (_arrowIndicatorCutFood != null) _arrowIndicatorCutFood.SetActive(false);

            switch (currentStep)
            {
                case TutorialStep.Panduan:
                    if (_instruksiPanduan != null) _instruksiPanduan.SetActive(true);
                    if (_arrowIndicatorBook != null) _arrowIndicatorBook.SetActive(true);
                    break;

                case TutorialStep.Resep:
                    if (_instruksiResep != null) _instruksiResep.SetActive(true);
                    if (_arrowIndicatorBook != null) _arrowIndicatorBook.SetActive(true);
                    break;

                case TutorialStep.AmbilBahan:
                    if (_arrowIndicatorIngredient != null) _arrowIndicatorIngredient.SetActive(true);
                    if (_panelLanjut != null) _panelLanjut.SetActive(true);
                    break;

                case TutorialStep.MasakMakanan:
                    // Pastikan panel instruksi sebelumnya mati
                    if (_instruksiResep != null) _instruksiResep.SetActive(false);
                    if (_panelLanjut != null) _panelLanjut.SetActive(false);
                    if (_panelMasak != null) _panelMasak.SetActive(true);
                    break;

                case TutorialStep.HidangkanMakanan:
                    if (_instruksiHidangkan != null) _instruksiHidangkan.SetActive(true);
                    break;

                case TutorialStep.SimpanKenampan:
                   if (_panelNampan != null) _panelNampan.SetActive(true);  
                    break;

                case TutorialStep.SajikanMakanan:
                    if (_instruksiMejaSaji != null)
                    {
                        _instruksiMejaSaji.SetActive(true);
                        // Animasi kecil agar pemain sadar ada instruksi baru
                        _instruksiMejaSaji.transform.localScale = Vector3.zero;
                        _instruksiMejaSaji.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                    }
                    break;

            }
        }
    }
}
