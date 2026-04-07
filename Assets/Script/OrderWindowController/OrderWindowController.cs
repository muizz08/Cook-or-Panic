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
        [SerializeField] private SocketController _nampanSocket;
        [SerializeField] private SocketController _windowSocket;
        [SerializeField] private Transform _trayReturnPoint;
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

            if (_nampanSocket != null) _nampanSocket.OnObjectEntered -= Nampan;

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
            var socket = _windowSocket.Socket;

            // Coba ambil objek tertua yang sedang berinteraksi
            IXRSelectInteractable trayInteractable = socket.GetOldestInteractableSelected();

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
                socket.interactionManager.SelectExit(socket, trayInteractable);

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
            // 1. Matikan socket agar tidak mendeteksi objek saat proses pindah
            _windowSocket.enabled = false;

            XRGrabInteractable interactable = trayObject.GetComponent<XRGrabInteractable>();
            // Ambil Rigidbody nampan sekali di awal
            Rigidbody trayRb = trayObject.GetComponent<Rigidbody>();

            if (interactable != null && interactable.isSelected)
            {
                interactable.interactionManager.SelectExit(interactable.firstInteractorSelecting, interactable);
                 interactable.enabled = false;
            }

       
            // 2. PREPARASI PHYSICS (Mencegah Glitch)
            if (trayRb != null)
            {
                trayRb.isKinematic = true;
                trayRb.velocity = Vector3.zero;          // Reset kecepatan linear
                trayRb.angularVelocity = Vector3.zero;   // Reset kecepatan rotasi
                trayRb.interpolation = RigidbodyInterpolation.None; // Matikan interpolasi
            }


            if (trayRb != null)
            {
                trayRb.isKinematic = true;
                trayRb.velocity = Vector3.zero;
                trayRb.angularVelocity = Vector3.zero;
            }


            yield return new WaitForEndOfFrame();

          
            // 2. PROSES IKAT PIRING KE NAMPAN (PENTING!)
            if (plateObject != null)
            {
                // Matikan physics piring agar tidak berontak saat pindah
                Rigidbody plateRb = plateObject.GetComponent<Rigidbody>();
                if (plateRb != null) plateRb.isKinematic = true;
            }

            // 2. TELEPORT KE RAK PENGEMBALIAN (Nampan + Piring)
            if (trayObject != null && _trayReturnPoint != null)
            {
                //trayObject.transform.SetParent(null);
                trayObject.transform.SetPositionAndRotation(_trayReturnPoint.position, _trayReturnPoint.rotation);
                Debug.Log("Teleport nampan bersih tanpa glitch");
            }
            // 3. JEDA WAKTU (Simulasi nampan sedang "antre" atau diproses di belakang)
            // Kamu bisa ubah angka 3f ini sesuai keinginan (misal 5 detik)
            yield return new WaitForSeconds(1.5f);

            if (interactable != null)
            {
                interactable.enabled = true;
            }


            if (trayObject != null && _mechanicManager != null)
            {
                // Fungsi ini akan otomatis menghapus makanan dan kirim piring ke wastafel
                // karena piring masih ada di atas nampan (sebagai child)
                _mechanicManager.SpawnDirtyPlate(trayObject);
            }

            // 4. TELEPORT KEMBALI KE WINDOW SOCKET
            if (trayObject != null)
            {
                _isReturningTray = true;
                trayObject.SetActive(false);

                // Kembalikan ke posisi socket jendela
                trayObject.transform.position = _windowSocket.transform.position;
                trayObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

                yield return new WaitForSeconds(1.5f);
                    
                trayObject.SetActive(true);

                if (trayRb != null)
                {
                    trayRb.isKinematic = false; // Aktifkan lagi agar bisa diambil pemain
                }

                if (TutorialManager.Instance != null)
                {
                    // Kita panggil fungsi yang memicu step "SimpanNampanBalik"
                    TutorialManager.Instance.OnBellPressedDuringTutorial();
                }

                Debug.Log("<color=green>Loop:</color> Nampan kembali ke Window Socket!");
            }


            // 5. AKTIFKAN KEMBALI SISTEM
            _windowSocket.enabled = true;
            if (canvas != null) canvas.ShowRandomOrder();
        }

    }
}

