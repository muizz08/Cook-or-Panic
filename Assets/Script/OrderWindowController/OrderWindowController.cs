using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


namespace CookOrPanic.OrderWindowController
{

    using System.Collections.Generic;
    using CookOrPanic.CanvasManager;
    using CookOrPanic.Plate;
    using CookOrPanic.ProcessingMechanic;
    using CookOrPanic.Score;
    using CookOrPanic.SocketController;
    using CookOrPanic.TutorialManager;

    public class OrderWindowController : MonoBehaviour
    {
        [Header("Sockets")]
        [SerializeField] private List<SocketController> _scoringSockets = new List<SocketController>();
        [SerializeField] private List<SocketController> _nampanSockets = new List<SocketController>();
        [SerializeField] private SocketController _windowSocket;
        [SerializeField] private Transform _trayReturnPoint;

        [Header("NPC Settings")]
        [SerializeField] private GameObject _npcObject; // Drag NPC kamu ke sini
        [SerializeField] private Transform _npcHandPoint; // Tarik tulang tangan kanan/kiri NPC ke sini
        [SerializeField] private Animator _npcAnimator;
        [SerializeField] private Transform _npcSpawnPoint;
        [SerializeField] private Transform _npcTargetWindow;
        [SerializeField] private float _npcWalkSpeed = 2f;

        private bool _isReturningTray = false;

        [Header("Prefabs & References")]
        [SerializeField] private ProcessingMechanic _mechanicManager;

        private void Start()
        {
            if (_windowSocket != null)
                _windowSocket.OnObjectEntered += OnTrayEnteredWindow;

            foreach (SocketController socket in _scoringSockets)
            {
                if (socket != null)
                {
                    socket.OnObjectEntered += OnScoringSocketEntered;
                }
            }

            foreach (SocketController socket in _nampanSockets)
            {
                if (socket != null)
                {
                    socket.OnObjectEntered += Nampan;
                }
            }

        }


        private void OnScoringSocketEntered(GameObject plateObject)
        {
            XRSocketInteractor socketOnTray = plateObject.GetComponentInChildren<XRSocketInteractor>();

            // 2. Lock Food to Plate
            Plate plate = plateObject.GetComponent<Plate>();
            if (plate != null) plate.LockFoodToPlate();

            // 3. Ubah Layer agar tidak bisa diambil lagi (Opsional)
            if (plateObject.TryGetComponent(out XRGrabInteractable grab))
            {
                grab.interactionLayers = InteractionLayerMask.GetMask("SocketOnly");
            }
        }

        private void Nampan(GameObject trayObject)
        {

            XRSocketInteractor socketOnTray = trayObject.GetComponentInChildren<XRSocketInteractor>();
            if (socketOnTray != null && socketOnTray.hasSelection)
            {
                socketOnTray.enabled = false; // Kunci piring di nampan
            }

            Plate plate = trayObject.GetComponent<Plate>();
            if (plate != null) plate.LockPlateToTray();
        }

        public void OnTrayEnteredWindow(GameObject trayObject)
        {
            // ❗ CEK DULU
            if (_isReturningTray)
            {
                Debug.Log("Nampan balik - skip update score");
                _isReturningTray = false; // reset
                return;
            }

            Debug.Log("WINDOW TERPANGGIL"); 
        }

        public void PushBell()
        {

            Debug.Log("<color=white>Bell:</color> Tombol Bel ditekan.");

            // Gunakan interactor dari socket secara langsung
            XRSocketInteractor windowSocket = _windowSocket.Socket;
           

            // Coba ambil objek tertua yang sedang berinteraksi
            IXRSelectInteractable trayInteractable = windowSocket.GetOldestInteractableSelected();

            if (trayInteractable != null) // Jika ada objek yang terdeteksi
            {
                GameObject trayObject = trayInteractable.transform.gameObject;
                Debug.Log($"<color=green>Bell:</color> Mendeteksi objek: {trayObject.name}");

                // Cari piring
                XRSocketInteractor socketOnTray = trayObject.GetComponentInChildren<XRSocketInteractor>();
                GameObject plateObject = (socketOnTray != null && socketOnTray.hasSelection)
                    ? socketOnTray.GetOldestInteractableSelected().transform.gameObject
                    : null;

                // Lepas nampan
                windowSocket.interactionManager.SelectExit(windowSocket, trayInteractable);

                CanvasManager canvas = FindObjectOfType<CanvasManager>();
                // Ambil score terbaru
                int currentScore = Score.Instance.GetCurrentScore();

                if (canvas != null)
                {
                    canvas.UpdateScoreUI(currentScore);
                }
                StartCoroutine(DelayedNewOrder(canvas, plateObject, trayObject));
            }
            else
            {
                // DEBUG TAMBAHAN: Jika gagal, kita cek manual pakai OverlapSphere (opsional)
                Debug.LogWarning("<color=red>Bell:</color> Socket kosong! Cek Interaction Layer Mask di Inspector.");
            }
        }

        public IEnumerator DelayedNewOrder(CanvasManager canvas, GameObject plateObject, GameObject trayObject)
        {
            yield return new WaitForSeconds(1.5f);

            // --- TAHAP 1: NPC DATANG ---
            _npcObject.SetActive(true);
            _npcObject.transform.position = _npcSpawnPoint.position;
            _npcAnimator.SetBool("isWalking", true);

            while (Vector3.Distance(_npcObject.transform.position, _npcTargetWindow.position) > 0.1f)
            {
                _npcObject.transform.position = Vector3.MoveTowards(_npcObject.transform.position, _npcTargetWindow.position, _npcWalkSpeed * Time.deltaTime);
                _npcObject.transform.LookAt(_npcTargetWindow);
                yield return null;
            }
            _npcAnimator.SetBool("isWalking", false);
            yield return new WaitForSeconds(1.0f);

            // --- TAHAP 2: NAMPAN MENEMPEL KE TANGAN NPC (PENTING!) ---
            _windowSocket.enabled = false;
            Rigidbody trayRb = trayObject.GetComponent<Rigidbody>();
            XRGrabInteractable interactable = trayObject.GetComponent<XRGrabInteractable>();

            if (interactable != null && interactable.isSelected)
            {
                interactable.interactionManager.SelectExit(interactable.firstInteractorSelecting, interactable);
                interactable.enabled = false;
            }

            if (trayRb != null) trayRb.isKinematic = true;

            // --- TAHAP 2: NAMPAN MENEMPEL KE TANGAN NPC ---
            if (trayObject != null && _npcHandPoint != null)
            {
                trayObject.transform.SetParent(_npcHandPoint);

                // Reset total
                trayObject.transform.localPosition = Vector3.zero;

                // Gunakan rotasi lokal identitas (mengikuti arah tulang tangan)
                // Jika nampan miring, ganti Quaternion.identity dengan Euler yang pas
                trayObject.transform.localRotation = Quaternion.identity;

                // PENTING: Matikan interpolasi Rigidbody agar tidak melawan gerakan parent
                if (trayRb != null)
                {
                    trayRb.isKinematic = true;
                    trayRb.interpolation = RigidbodyInterpolation.None;
                }
            }

            yield return new WaitForSeconds(0.5f);

            // --- TAHAP 3: NPC JALAN KE BELAKANG (Nampan ikut karena sudah jadi Child) ---
            _npcAnimator.SetBool("isWalking", true);
            while (Vector3.Distance(_npcObject.transform.position, _npcSpawnPoint.position) > 0.5f)
            {
                _npcObject.transform.position = Vector3.MoveTowards(_npcObject.transform.position, _npcSpawnPoint.position, _npcWalkSpeed * Time.deltaTime);
                _npcObject.transform.LookAt(_npcSpawnPoint); // Nampan ikut berputar di sini
                yield return null;
            }

            _npcAnimator.SetBool("isWalking", false);

            // Saat sampai di belakang, proses piring (opsional: sembunyikan nampan atau tetap pegang)
            if (trayObject != null && _mechanicManager != null)
            {
                _mechanicManager.SpawnDirtyPlate(trayObject);
            }

            yield return new WaitForSeconds(2.0f);

            // --- TAHAP 4: NPC KEMBALI KE JENDELA ---
            _npcAnimator.SetBool("isWalking", true);
            while (Vector3.Distance(_npcObject.transform.position, _npcTargetWindow.position) > 0.1f)
            {
                _npcObject.transform.position = Vector3.MoveTowards(_npcObject.transform.position, _npcTargetWindow.position, _npcWalkSpeed * Time.deltaTime);
                _npcObject.transform.LookAt(_npcTargetWindow);
                yield return null;
            }
            _npcAnimator.SetBool("isWalking", false);

            // --- TAHAP 5: LEPAS NAMPAN KE WINDOW ---
            if (trayObject != null)
            {
                _isReturningTray = true;
                
                trayObject.transform.position = _windowSocket.transform.position;
                trayObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

                yield return new WaitForSeconds(0.5f);

                if (trayRb != null) trayRb.isKinematic = false;
                if (interactable != null) interactable.enabled = true;

                Debug.Log("Nampan dikembalikan ke jendela.");
            }

            // --- TAHAP 6: NPC PERGI (TANGAN KOSONG) ---
            yield return new WaitForSeconds(1.0f);
            // --- TAHAP 3: NPC JALAN KE BELAKANG ---
            _npcAnimator.SetBool("isWalking", true);
            while (Vector3.Distance(_npcObject.transform.position, _npcSpawnPoint.position) > 0.5f)
            {
                // Hitung arah ke tujuan
                Vector3 direction = (_npcSpawnPoint.position - _npcObject.transform.position).normalized;

                // Rotasikan NPC ke arah tujuan (Hanya sumbu Y agar tidak nungging)
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    _npcObject.transform.rotation = Quaternion.Slerp(_npcObject.transform.rotation, targetRotation, 10f * Time.deltaTime);
                }

                _npcObject.transform.position = Vector3.MoveTowards(_npcObject.transform.position, _npcSpawnPoint.position, _npcWalkSpeed * Time.deltaTime);
                yield return null;
            }
            _npcObject.SetActive(false);

            _windowSocket.enabled = true;
            if (canvas != null) canvas.ShowRandomOrder();
        }

    }
}

