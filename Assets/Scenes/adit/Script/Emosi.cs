using UnityEngine;
using UnityEngine.UI;

public class Emosi : MonoBehaviour
{
    public RectTransform ikonChef;
    public float speed = 10f;
    private float isiEmosi = 0f; // Pengganti fillAmount (0 sampai 1)

    void Update()
    {
        // --- KALIBRASI MANUAL ---
        // Naikkan angka ini kalau kurang ke KANAN (Merah)
        // Turunkan angka ini kalau kurang ke KIRI (Hijau)
        float batasKanan = 56f;
        float batasKiri = -56f;

        // Hitung posisi (Mencampur antara batas kiri dan kanan)
        float targetX = Mathf.Lerp(batasKiri, batasKanan, isiEmosi);

        // Gerak mulus
        float smoothX = Mathf.Lerp(ikonChef.anchoredPosition.x, targetX, Time.deltaTime * speed);
        ikonChef.anchoredPosition = new Vector2(smoothX, 0);

        // Kontrol
        if (Input.GetKeyDown(KeyCode.F)) isiEmosi = Mathf.Clamp(isiEmosi + 0.1f, 0, 1);
        if (Input.GetKeyDown(KeyCode.T)) isiEmosi = Mathf.Clamp(isiEmosi - 0.1f, 0, 1);
    }
}