using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private Renderer rend;
    
    [Header("Ayarlar")]
    // Torba hızıyla bantın deseni arasındaki uyumu buradan ince ayar yaparsın.
    public float textureMultiplier = 0.28f; 
    
    private string textureProperty = "_BaseMap"; // URP/Lit Shader için
    private float currentOffset = 0f;

    void Start()
    {
        rend = GetComponent<Renderer>();

        // Shader tipine göre doğru ismi bul (URP veya Standart)
        if (rend != null && rend.material != null)
        {
            if (rend.material.HasProperty("_BaseMap")) textureProperty = "_BaseMap";
            else if (rend.material.HasProperty("_MainTex")) textureProperty = "_MainTex";
        }
    }

    void Update()
    {
        // 1. GÜVENLİK: Oyun durduysa veya Manager yoksa akışı durdur
        if (Time.timeScale == 0f || rend == null || ScoreManager.instance == null) return;

        // 2. SENKRONİZASYON: ScoreManager'daki güncel hızı çekiyoruz
        float currentSpeed = ScoreManager.instance.currentGlobalSpeed;

        // 3. HESAPLAMA: Zamanla hızı çarparak offset oluşturuyoruz
        // Başına EKSİ koyduk ki torbalar oyuncuya gelirken bant da oyuncuya aksın
        currentOffset += Time.deltaTime * currentSpeed * -textureMultiplier;
        
        // 4. UYGULAMA: Texture'ı kaydır
        rend.material.SetTextureOffset(textureProperty, new Vector2(0, currentOffset));
    }
}