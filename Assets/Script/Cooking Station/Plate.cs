 using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


namespace CookOrPanic.Plate
{
    using CookOrPanic.Food;
    using CookOrPanic.Score;
    public class Plate : MonoBehaviour
    {

        private XRSocketInteractor _socket;

        private void Awake()
        {
            _socket = GetComponentInChildren<XRSocketInteractor>();
        }

        private void OnEnable()
        {
            // Berlangganan event saat makanan masuk ke piring
            _socket.selectEntered.AddListener(OnFoodPlaced);
        }

        private void OnDisable()
        {
            _socket.selectEntered.RemoveListener(OnFoodPlaced);
        }

        private void OnFoodPlaced(SelectEnterEventArgs args)
        {
            GameObject foodObject = args.interactableObject.transform.gameObject;
            Debug.Log($"Makanan {foodObject.name} masuk ke piring!");

            Food food = foodObject.GetComponent<Food>();

            if (food != null)
            {
                Score.Instance.AddScore(food);
                Debug.Log("Score sekarang: " + Score.Instance.GetCurrentScore());
            }
        }

        // Di dalam Plate.cs
        public void DestroyFoodInSocket()
        {
            // 1. Cek lewat Socket dulu
            if (_socket != null && _socket.hasSelection)
            {
                IXRSelectInteractable foodInteractable = _socket.GetOldestInteractableSelected();
                GameObject foodObj = foodInteractable.transform.gameObject;

                _socket.interactionManager.SelectExit(_socket, foodInteractable);
                Destroy(foodObj);
                Debug.Log("<color=red>Plate:</color> Makanan dihancurkan via Socket.");
            }
            else
            {
                // 2. BACKUP: Jika socket meleset (karena SetParent), kita cari manual di child
                // Cari semua objek yang punya script Food di bawah piring ini
                Food foodInChild = GetComponentInChildren<Food>();
                if (foodInChild != null)
                {
                    Destroy(foodInChild.gameObject);
                    Debug.Log("<color=red>Plate:</color> Makanan dihancurkan via Child Search (Fallback).");
                }
            }
        }

        // Fungsi ini akan dipanggil oleh Score.cs saat piring masuk socket scoring
        public void LockFoodToPlate()
        {
            // Cari semua komponen Food yang ada di dalam piring (sebagai child)
            Food[] foodsInPlate = GetComponentsInChildren<Food>();

            foreach (Food food in foodsInPlate)
            {
                // 1. Matikan fisika agar tidak goyang/jatuh saat piring dibawa
                Rigidbody rb = food.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true; // Matikan fisika agar makanan tidak terpengaruh oleh gaya apapun
                    rb.useGravity = false;
                }

                // 2. Matikan kemampuan Grab pada makanan agar pemain tidak bisa mencuri isinya
                XRGrabInteractable foodGrab = food.GetComponent<XRGrabInteractable>();
                if (foodGrab != null)
                {
                    foodGrab.enabled = false;
                }

                food.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

                Debug.Log($"Makanan {food.name} telah dikunci ke piring.");

                Rigidbody plateRb = GetComponent<Rigidbody>();
                if (plateRb != null)
                {
                    plateRb.isKinematic = true;
                }

                Debug.Log("Piring dikunci tanpa konflik hirarki.");
            }
        }

        public void LockPlateToTray()
        {
            float radius = 1.5f; // 🔥 perbesar radius biar pasti kena food

            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider hit in hits)
            {
                // 🔥 ambil Food dari parent juga (biar aman kalau script ada di child)
                Food food = hit.GetComponentInParent<Food>();

                if (food != null)
                {
                    Debug.Log("Food terdeteksi: " + food.name);

                    // 🔥 jadikan child ke plate
                    food.transform.SetParent(this.transform);

                    // 🔥 matikan physics
                    Rigidbody rb = food.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.useGravity = false;
                    }

                    // 🔥 matikan XR grab
                    XRGrabInteractable grab = food.GetComponent<XRGrabInteractable>();
                    if (grab != null)
                    {
                        grab.enabled = false;
                    }
                }
            }

            // 🔥 FIX PENTING: plate HARUS kinematic
            Rigidbody plateRb = GetComponent<Rigidbody>();
            if (plateRb != null)
            {
                plateRb.isKinematic = false;
                plateRb.useGravity = false;
            }

            Debug.Log("Plate & semua food sudah terkunci ke tray");
        }
    }
}