 using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


namespace CookOrPanic.Plate
{
    using CookOrPanic.Food;
    public class Plate : MonoBehaviour
    {
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

                Debug.Log($"Makanan {food.name} telah dikunci ke piring.");
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

                    // 🔥 reset transform biar nempel
                    food.transform.localPosition = Vector3.zero;
                    food.transform.localRotation = Quaternion.identity;

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