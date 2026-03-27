using UnityEngine;
using UnityEngine.UI; // PENTING: Harus ada ini biar Image terbaca

public class Darah : MonoBehaviour
{ // Harus ada kurung kurawal pembuka di sini
    public Image barMerah;
    public float darahMaksimal = 100f;
    float darahSekarang;

    void Start()
    {
        darahSekarang = darahMaksimal;
    }

    public void KurangiHP(float jumlah)
    {
        darahSekarang -= jumlah;
        darahSekarang = Mathf.Clamp(darahSekarang, 0, darahMaksimal);
        barMerah.fillAmount = darahSekarang / darahMaksimal;
    }

    public void TambahHP(float jumlah)
    {
        darahSekarang += jumlah;
        // Penting: Biar darah nggak luber lebih dari maksimal
        darahSekarang = Mathf.Clamp(darahSekarang, 0, darahMaksimal);

        // Update tampilan bar
        barMerah.fillAmount = darahSekarang / darahMaksimal;
    }
}
