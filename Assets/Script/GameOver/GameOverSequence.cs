using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace CookOrPanic.GameOverSequence
{
    public class GameOverSequence : MonoBehaviour
    {
        public float delayBeforePanic = 3f; // Waktu tunggu sebelum mulai getar
        public float shakeDuration = 2f;    // Berapa lama getarnya
        public float shakeIntensity = 0.1f; // Kekuatan getar
        public float flyForce = 10f;        // Kekuatan terbang

        private List<Rigidbody> _kitchenItems = new List<Rigidbody>();

        void Start()
        {
            // Cari semua objek dengan Rigidbody di scene (kecuali Player/Chef)
            Rigidbody[] allRbs = FindObjectsOfType<Rigidbody>();
            foreach (Rigidbody rb in allRbs)
            {
                if (rb.CompareTag("Untagged") || rb.CompareTag("Prop")) // Pastikan kasih tag ke barang dapur
                {
                    rb.useGravity = false; // Matikan gravitasi dulu supaya bisa melayang nanti
                    _kitchenItems.Add(rb);
                }
            }

            StartCoroutine(ExecuteGameOver());
        }

        IEnumerator ExecuteGameOver()
        {
            // 1. Diam sejenak (Player vs Headchef)
            yield return new WaitForSeconds(delayBeforePanic);

            // 2. Efek Getar (Semua barang mulai panik)
            float elapsed = 0;
            while (elapsed < shakeDuration)
            {
                foreach (Rigidbody rb in _kitchenItems)
                {
                    rb.transform.position += (Vector3)Random.insideUnitCircle * shakeIntensity;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 3. TERBANG! (Berikan gaya ke atas dan acak)
            foreach (Rigidbody rb in _kitchenItems)
            {
                rb.useGravity = false; // Pastikan tetap mati gravitasi atau kasih nilai kecil
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f));
                rb.AddForce(randomDirection * flyForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * flyForce, ForceMode.Impulse); // Biar muter-muter
            }

            Debug.Log("Barang terbang! Headchef sangat marah!");
        }
    }
}
