using UnityEngine;

public class ScoreDetector : MonoBehaviour
{
    [Header("Dinamik Ayarlar")]
    // ScoreManager bu değeri anlık günceller.
    public string targetColor; 

    private void OnTriggerStay(Collider other)
    {
        // 1. Sadece "trash" etiketli objeleri kontrol et
        if (other.CompareTag("trash") || other.CompareTag("Trash"))
        {
            TrashMovement trash = other.GetComponent<TrashMovement>();
            if (trash == null) trash = other.GetComponentInParent<TrashMovement>();

            if (trash != null)
            {
                // --- ÇOK KRİTİK: ELİNDE TUTUYORSAN ASLA YUTMA ---
                // Bu sayede torbayı kutunun üstünde gezdirirken elinden "çat" diye çekmez.
                if (trash.isGrabbed) return; 

                // 2. Renkleri temizle ve karşılaştır (Küçük harfe zorla)
                string incomingColor = trash.trashColor.Trim().ToLower();
                string expectedColor = targetColor.Trim().ToLower();

                // Konsolda kimin neyi beklediğini net görelim (Debug için hayat kurtarır)
                Debug.Log($"<color=white>DEDEKTÖR [{gameObject.name}]:</color> Gelen: {incomingColor} | Beklenen: {expectedColor}");

                if (incomingColor == expectedColor)
                {
                    // --- DOĞRU EŞLEŞME ---
                    if (ScoreManager.instance != null) 
                        ScoreManager.instance.AddScore(10);
                    
                    // Görsel zıplama efekti (BinJuice scripti varsa)
                    BinJuice juice = GetComponent<BinJuice>();
                    if(juice == null) juice = GetComponentInParent<BinJuice>();
                    if(juice != null) juice.PlayBounce();
                }
                else
                {
                    // --- YANLIŞ EŞLEŞME ---
                    if (ScoreManager.instance != null) 
                        ScoreManager.instance.WrongDrop();

                    // Görsel hata efekti (BinJuice scripti varsa)
                    BinJuice juice = GetComponent<BinJuice>();
                    if(juice == null) juice = GetComponentInParent<BinJuice>();
                    if(juice != null) juice.PlayWrongShake();
                }

                // 3. İşlem bitti, torbayı yok et
                Destroy(trash.gameObject);
            }
        }
    }
}