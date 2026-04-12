using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


namespace CookOrPanic.ProcessingMechanic
{
    using System.Collections;
    using System.Collections.Generic;
    using CookOrPanic.Ingredient;
    using CookOrPanic.Plate;
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.SocketController;
    using CookOrPanic.AudioManager;

    public class ProcessingMechanic : MonoBehaviour
    {
        private static int _nextSlotIndex = 0;
        private static int _piringDiWastafel = 0;
        private List<GameObject> _piringDiWastafelList = new List<GameObject>();


        [Header("Food Processor")]
        [SerializeField] private GameObject _ikanGilingPrefab; // Prefab hasil gilingan
        [SerializeField] private GameObject _ayamGilingPrefab;
        [SerializeField] private Transform _spawnPoint;        // Lokasi di atas mangkok
        [SerializeField] private SocketController _socket;   // Socket di lubang gilingan
        private GameObject _dagingMentah;

        [Header("Dirty Plate")]
        [SerializeField] private GameObject _cleanButtonCanvas;
        [SerializeField] private List<Transform> _plateRackSlots = new List<Transform>();
        [SerializeField] private Transform _wastafelSpawnPoint;
        [SerializeField] private ParticleSystem _foam;

        [Header("Dirty Plate")]
        [SerializeField] private bool _isTutorialMode = false; // Centang ini khusus di scene tutorial
        [SerializeField] private int _mainSceneCapacity = 6;  // Kapasitas untuk main scene
        [SerializeField] private int _tutorialCapacity = 1;
        private int _kapasitasWastafel;


        [Header("Grinder Settings")]
        private bool _isGrinding = false; // Status mesin

        private void Start()
        {
            _foam.Stop();
            // Tentukan kapasitas berdasarkan mode
            _kapasitasWastafel = _isTutorialMode ? _tutorialCapacity : _mainSceneCapacity;

            // Reset static variables (PENTING: karena static tidak reset otomatis saat ganti scene)
            _piringDiWastafel = 0;
            _nextSlotIndex = 0;


            if (_socket != null)
            {
                _socket.OnObjectEntered += HandleObjectEntered;
                _socket.OnObjectRemoved += HandleObjectRemoved;
            }

        }
        private void OnDestroy()
        {
            // Unsubscribe untuk keamanan memory
            if (_socket != null)
            {
                _socket.OnObjectEntered -= HandleObjectEntered;
                _socket.OnObjectRemoved -= HandleObjectRemoved;
            }
        }

        private void HandleObjectEntered(GameObject obj)
        {
            _dagingMentah = obj;
            Debug.Log($"<color=cyan>Processor:</color> {obj.name} siap diproses.");
        }

        // Fungsi ini dipanggil otomatis oleh SocketController saat benda keluar
        private void HandleObjectRemoved()
        {
            _dagingMentah = null;
        }

        // FUNGSI INI DIHUBUNGKAN KE TOMBOL MERAH
        public void ToggleGrinding()
        {
            _isGrinding = !_isGrinding; // Balikkan status (On/Off)

            if (_isGrinding)
            {
                StartCoroutine(GrindingProcess());


            }
            else
            {
                StopGrindingSequence();
            }
        }

        private IEnumerator GrindingProcess()
        {
            // 1. Suara Pemantik/Start (Sekali bunyi)
            AudioManager.Instance.PlaySFX("OnGrinding");

            // 2. Tunggu sebentar lalu jalankan suara putaran mesin (Looping)
            yield return new WaitForSeconds(0.5f);

            if (_isGrinding) // Cek lagi siapa tahu pemain keburu mematikan
            {
                AudioManager.Instance.PlaySFX("GrinderLoop"); // Pastikan di Inspector SFX ini dicentang Loop
            }

            // 3. Selama mesin menyala, tunggu 2 detik untuk memproses daging
            while (_isGrinding)
            {
                if (_dagingMentah != null)
                {
                    // Tunggu proses giling 2 detik
                    yield return new WaitForSeconds(3.0f);

                    // Cek lagi setelah 2 detik, apakah daging masih ada dan mesin masih nyala
                    if (_isGrinding && _dagingMentah != null)
                    {
                        ProcessDaging(); // Pindah logika spawn ke fungsi terpisah agar rapi
                    }
                }
                yield return null; // Tunggu frame berikutnya jika tidak ada daging
            }
        }

        private void StopGrindingSequence()
        {
            _isGrinding = false;

            // Matikan suara mesin yang sedang looping
            AudioManager.Instance.StopSFX("OnGrinding");

            // Bunyikan suara mesin berhenti (Klak/Mati)
            AudioManager.Instance.PlaySFX("OffGrinding");

            if (TutorialManager.TutorialManager.Instance != null)
            {
                TutorialManager.TutorialManager.Instance.OnGrinderTurnedOff();
            }

        }

        private void ProcessDaging()
        {
            GameObject foodToProcess = _dagingMentah;
            _dagingMentah = null;

            Ingredient ingredient = foodToProcess.GetComponent<Ingredient>();
            if (ingredient == null) return;

            GameObject resultPrefab = (ingredient.GetIngredientType() == IngredientType.IkanMentah)
                ? _ikanGilingPrefab : _ayamGilingPrefab;

            if (resultPrefab != null)
            {
                Destroy(foodToProcess);
                Instantiate(resultPrefab, _spawnPoint.position, _spawnPoint.rotation);

                // Trigger tutorial jika ada
                if (TutorialManager.TutorialManager.Instance != null)
                {
                    // Panggil fungsi ini untuk memberitahu tutorial bahwa hasil sudah keluar
                    // dan sekarang saatnya memunculkan instruksi MATIKAN mesin.
                    TutorialManager.TutorialManager.Instance.OnGrinderResultSpawned();
                }
            }
        }

        public void SpawnDirtyPlate(GameObject nampanObj)
        {
            if (_wastafelSpawnPoint == null) return;

            XRSocketInteractor socketNampan = nampanObj.GetComponentInChildren<XRSocketInteractor>(true);

            if (socketNampan != null && socketNampan.hasSelection)
            {
                IXRSelectInteractable plateInteractable = socketNampan.GetOldestInteractableSelected();
                GameObject plateObj = plateInteractable.transform.gameObject;
                Rigidbody plateRb = plateObj.GetComponent<Rigidbody>();

                // 1. CARI FOOD DI SOCKET PIRING SECARA MANUAL (Metode paling aman)
                XRSocketInteractor socketPiring = plateObj.GetComponentInChildren<XRSocketInteractor>(true);
                if (socketPiring != null && socketPiring.hasSelection)
                {
                    // Ambil makanannya
                    GameObject foodObj = socketPiring.GetOldestInteractableSelected().transform.gameObject;

                    // Hapus paksa SEKARANG juga
                    Destroy(foodObj);
                    Debug.Log("<color=red>Mechanic:</color> Food dipaksa hancur sebelum teleport.");
                }
                else
                {
                    // Jika socket manual gak ketemu, baru panggil script Plate
                    var plateScript = plateObj.GetComponent<Plate>();
                    if (plateScript != null) plateScript.DestroyFoodInSocket();
                }

                // 2. Lepaskan piring dari nampan
                socketNampan.interactionManager.SelectExit(socketNampan, plateInteractable);
                plateObj.transform.SetParent(null);

                // 3. TELEPORTASI
                // JANGAN langsung SetActive(false). Kita pindahkan posisinya dulu.
                plateObj.transform.position = _wastafelSpawnPoint.position;
                plateObj.transform.rotation = _wastafelSpawnPoint.rotation;

                if (!_piringDiWastafelList.Contains(plateObj))
                {
                    _piringDiWastafelList.Add(plateObj);
                    _piringDiWastafel++; // Counter nambah di sini supaya akurat
                }

                // 4. Reset Physics agar tidak mental
                
                if (plateRb != null)
                {
                    plateRb.isKinematic = false;
                    plateRb.velocity = Vector3.zero;
                    plateRb.angularVelocity = Vector3.zero;
                }

                // 5. Update Interaction Layer
                if (plateObj.TryGetComponent(out XRGrabInteractable grab))
                {
                    grab.interactionLayers = InteractionLayerMask.GetMask("Default");
                }

                Debug.Log("<color=green>Mechanic:</color> Piring pindah ke wastafel.");
            }

            // 6. Cek Kapasitas
            if (_piringDiWastafel >= _kapasitasWastafel)
            {
                _cleanButtonCanvas.SetActive(true);
            }
        }

        //piring
        public void CleanAndReturnToRack()
        {
            if (_piringDiWastafelList.Count > 0 && _nextSlotIndex < _plateRackSlots.Count)
            {
                StartCoroutine(WashingSequence());


            }
        }

        private IEnumerator WashingSequence()
        {
            _foam.Play();
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("MencuciAudio");
            }
            yield return new WaitForSeconds(2.0f);

            _foam.Stop();
            if (_piringDiWastafelList.Count > 0)
            {
                GameObject plateToTeleport = _piringDiWastafelList[0];
                Transform targetSlot = _plateRackSlots[_nextSlotIndex];

                TeleportSpecificPlate(plateToTeleport, targetSlot);

                _piringDiWastafelList.RemoveAt(0);
                _nextSlotIndex++;

                if (_piringDiWastafelList.Count == 0)
                {
                    _cleanButtonCanvas.SetActive(false);
                }

                if (TutorialManager.TutorialManager.Instance != null)
                {
                    TutorialManager.TutorialManager.Instance.OnDishWashed();
                }
            }
        }
        private void TeleportSpecificPlate(GameObject plate, Transform slot)
        {
            plate.SetActive(false);

            plate.transform.position = slot.position;
            plate.transform.rotation = slot.rotation;

            plate.SetActive(true);

            Rigidbody rb = plate.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

        }
    }

}
