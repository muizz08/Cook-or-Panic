using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.UI;

namespace CookOrPanic.Score
{
    using CookOrPanic.CanvasManager;
    using CookOrPanic.Food;
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Plate;
    using CookOrPanic.LevelController;
    public class Score : MonoBehaviour
    {
        [SerializeField] private XRSocketInteractor _scoringSocket;
        [SerializeField] private XRSocketInteractor _UpdateUISocket;
        [SerializeField] private XRSocketInteractor _finishedSocket;
        [SerializeField] private Transform _trayReturnPoint; // Titik lokasi nampan kembali
        private int _currentScore;

        [Header("Nampan")]
        [SerializeField] private GameObject _nampanPrefab;


        private void Start()
        {
            if (_scoringSocket != null)
            {
                _scoringSocket.selectEntered.AddListener(OnScoringSocketEntered);

            }


            if (_UpdateUISocket != null)
            {
                _UpdateUISocket.selectEntered.AddListener(UpdateUI);
            }

            if (_finishedSocket != null)
                _finishedSocket.selectEntered.AddListener(OnFinishedSocketEntered);
        }

        private void OnDestroy()
        {
            if (_scoringSocket != null)
            {
                _scoringSocket.selectEntered.RemoveListener(OnScoringSocketEntered);
            }
        }

        // Fungsi KHUSUS untuk _scoringSocket
        // Di dalam fungsi OnScoringSocketEntered pada Score.cs:

        private void OnScoringSocketEntered(SelectEnterEventArgs args)
        {
            if (args.interactorObject.transform != _scoringSocket.transform) return;

            GameObject plateObject = args.interactableObject.transform.gameObject;

            // 1. Hitung Skor (Logika Score)
            Food food = plateObject.GetComponentInChildren<Food>();
            if (food != null)
            {
                AddScoreFromFood(food._recipeOrigin._cookedScore, food._foodState);
            }


            // 2. Panggil Logika Piring (Logika Plate)
            Plate plate = plateObject.GetComponent<Plate>();
            if (plate != null)
            {
                plate.LockFoodToPlate();
            }

            // 3. Kunci Piring ke Socket (Logika Interaction)
            XRGrabInteractable grab = plateObject.GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                grab.interactionLayers = InteractionLayerMask.GetMask("FoodOnly");
            }
        }

        private void OnFinishedSocketEntered(SelectEnterEventArgs args)
        {

            // Cegah proses jika socket sedang mati (sedang memproses nampan sebelumnya)
            if (!_finishedSocket.enabled) return;

            GameObject trayObject = args.interactableObject.transform.gameObject;
            XRSocketInteractor socketOnTray = trayObject.GetComponentInChildren<XRSocketInteractor>();

            if (socketOnTray != null && socketOnTray.hasSelection)
            {
                _finishedSocket.enabled = false;

                CanvasManager canvas = FindObjectOfType<CanvasManager>();
                GameObject plateObject = socketOnTray.GetOldestInteractableSelected().transform.gameObject;

                StartCoroutine(DelayedNewOrder(canvas, plateObject, trayObject));
            }
        }

        private IEnumerator DelayedNewOrder(CanvasManager canvas, GameObject plateObject, GameObject trayObject)
        {
            // 1. Persiapan Teleportasi Bersamaan
            yield return new WaitForSeconds(1f); // Beri waktu pemain melihat piring di nampan sejenak

            if (trayObject != null && _trayReturnPoint != null)
            {
             
               
                Rigidbody trayRb = trayObject.GetComponent<Rigidbody>();
                if (trayRb != null) trayRb.isKinematic = true;

                // 2. TELEPORTASI (Parent dan Child pindah sekaligus)
                trayObject.transform.position = _trayReturnPoint.position;
                trayObject.transform.rotation = _trayReturnPoint.rotation;

                // Tunggu satu frame fisika agar posisi sinkron
                yield return new WaitForFixedUpdate();

                yield return new WaitForSeconds(0.5f); // Efek visual agar piring terlihat sampai di return point dulu
                //if (plateObject != null)
                //{
                //    Destroy(plateObject);
                //}
            }

            yield return new WaitForSeconds(0.5f); // Tambahan delay agar nampan tidak langsung muncul saat piring masih terlihat di return point

            // Ganti bagian spawn nampan dengan ini agar lebih aman:
            if (_nampanPrefab != null && _finishedSocket != null)
            {
                // Gunakan posisi socket itu sendiri, bukan attachTransform jika ragu
                Vector3 spawnPos = _finishedSocket.transform.position;
                Quaternion spawnRot = _finishedSocket.transform.rotation;

                GameObject newTray = Instantiate(_nampanPrefab, spawnPos, spawnRot);

                // Paksa masuk ke socket agar nempel di depan pemain
                var interactable = newTray.GetComponent<IXRSelectInteractable>();
                if (interactable != null)
                {
                    _finishedSocket.interactionManager.SelectEnter(_finishedSocket, interactable);
                }
            }

            if (canvas != null) canvas.ShowRandomOrder();

            yield return new WaitForSeconds(0.2f);
            _finishedSocket.enabled = true;
        }

        public void AddScore(Food food)
        {
            int baseScore = food._recipeOrigin._cookedScore;
            FoodState state = food._foodState;
            AddScoreFromFood(baseScore, state);
        }

        public void AddScoreFromFood(int _cookedScore, FoodState state)
        {
            int pointToAdd = 0;

            switch (state)
            {
                case FoodState.Cooked:
                    pointToAdd = _cookedScore;
                    break;
                case FoodState.Raw:
                    pointToAdd = Mathf.Max(9, _cookedScore - 5);
                    break;
                case FoodState.OverCooked:
                    pointToAdd = Mathf.Max(5, _cookedScore - 3);
                    break;
                default:
                    pointToAdd = 0;
                    break;
            }

            // Di Score.cs -> fungsi AddScoreFromFood
            _currentScore += pointToAdd;
            Debug.Log("Skor Saat Ini: " + _currentScore);

            if (LevelController.Instance != null)
            {
                // Cek apakah skor sudah mencapai atau melewati target level saat ini
                if (_currentScore >= LevelController.Instance.GetTargetScore())
                {
                    LevelController.Instance.NexTLevel();
                }
            } 
        }

        private void UpdateUI(SelectEnterEventArgs args)
        {
            // 1. Objek yang masuk ke UpdateUI adalah Nampan (Tray)
            GameObject trayObject = args.interactableObject.transform.gameObject;
            CanvasManager canvas = FindObjectOfType<CanvasManager>();

            // 2. Cari socket yang ada di atas Nampan (tempat piring menempel)
            XRSocketInteractor socketOnTray = trayObject.GetComponentInChildren<XRSocketInteractor>();

            if (socketOnTray != null && socketOnTray.hasSelection)
            {
                // Ambil objek Piring yang sedang nempel di socket Nampan
                GameObject plateObject = socketOnTray.GetOldestInteractableSelected().transform.gameObject;

                if (canvas != null)
                {
                    canvas.UpdateScoreUI(_currentScore);
                }

                // Matikan socket nampan agar tidak bisa ditaruh piring baru selama proses delay
                socketOnTray.enabled = false;
            }
            else
            {
                Debug.LogWarning("Nampan masuk UpdateUI tapi tidak ada piring di atasnya!");
            }

            Plate plate = trayObject.GetComponent<Plate>();

            // 2. Jika komponen ditemukan, panggil fungsinya
            if (plate != null)
            {
                plate.LockPlateToTray();
            }
            else
            {
                Debug.LogWarning("Script Plate tidak ditemukan pada objek: " + trayObject.name);
            }
        }


        public void ResetScore()
        {
            _currentScore = 0;
            CanvasManager canvas = FindObjectOfType<CanvasManager>();
            if (canvas != null) canvas.UpdateScoreUI(0);
        }

    }
}