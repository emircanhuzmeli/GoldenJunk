using UnityEngine;
using TMPro;
using System.Collections;

public class TrashSpawner : MonoBehaviour
{
    public static TrashSpawner instance;

    [Header("UI Elemanları")]
    public TextMeshProUGUI infoText; 

    [Header("Çöp Havuzu (0:Blue, 1:Yellow, 2:Red, 3:Green)")]
    public GameObject[] trashPrefabs; 

    [Header("Dinamik Ayarlar")]
    public bool canSpawn = true;
    private bool isWarmingUp = true; 
    private int spawnedCount = 0;    

    void Awake() 
    { 
        if (instance == null) instance = this; 
    }

    void Start() 
    { 
        if(infoText != null) infoText.gameObject.SetActive(false);
        // Spawning döngüsünü başlatıyoruz
        StartCoroutine(SpawnRoutine());
    }

    // --- AKICI (SMOOTH) FIRLATMA DÖNGÜSÜ ---
    IEnumerator SpawnRoutine()
    {
        // İlk başta kısa bir bekleme
        yield return new WaitForSeconds(1.0f);

        while (canSpawn)
        {
            if (Time.timeScale > 0)
            {
                Spawn();
            }

            // Bir sonraki torba ne zaman gelsin? (Hesaplama)
            float waitTime = CalculateNextWaitTime();
            yield return new WaitForSeconds(waitTime);
        }
    }

    float CalculateNextWaitTime()
    {
        if (isWarmingUp) return 2.2f; // Başlangıçta sakin gelsinler

        float currentSpeed = 3.5f;
        if (ScoreManager.instance != null) 
            currentSpeed = ScoreManager.instance.currentGlobalSpeed;

        // Hız arttıkça fırlatma sıklığı artar (Zorluk dengesi)
        float speedModifier = currentSpeed / 3.5f;
        float baseTime = Random.Range(1.3f, 1.9f) / speedModifier;

        // Arada bir "hızlı ikili" fırlatma şansı
        if (Random.value < 0.15f) baseTime *= 0.5f;

        return Mathf.Max(baseTime, 0.55f); // Çok aşırı hızlanıp laptopu boğmasın
    }

    void Spawn() 
    {
        if (trashPrefabs == null || trashPrefabs.Length == 0) return;

        // ScoreManager'daki aktif renk sayısına göre havuzu kısıtlıyoruz
        int maxIndex = (ScoreManager.instance != null) ? ScoreManager.instance.activeAreaCount : 2;
        maxIndex = Mathf.Clamp(maxIndex, 1, trashPrefabs.Length);

        int randomIndex = Random.Range(0, maxIndex);
        
        if (trashPrefabs[randomIndex] != null)
        {
            // Torbayı oluştur
            GameObject newTrash = Instantiate(trashPrefabs[randomIndex], transform.position, Quaternion.identity);
            
            // Hızını anında set et
            TrashMovement move = newTrash.GetComponent<TrashMovement>();
            if(move == null) move = newTrash.GetComponentInParent<TrashMovement>();
            
            if (move != null && ScoreManager.instance != null)
            {
                move.currentSpeed = ScoreManager.instance.currentGlobalSpeed;
            }

            // SMOOTH ÇIKIŞ: Torbaların birbirine çarpıp fırlamasını önlemek için küçük bir fizik itişi
            Rigidbody rb = newTrash.GetComponent<Rigidbody>();
            if(rb != null) 
            {
                rb.velocity = Vector3.zero; // Önceki hızları sıfırla
                rb.AddForce(Vector3.back * 1.5f, ForceMode.Impulse); 
            }
        }
        
        spawnedCount++;

        if (isWarmingUp && spawnedCount >= 3)
        {
            StartCoroutine(TransitionToRealGame());
        }
    }

    IEnumerator TransitionToRealGame()
    {
        isWarmingUp = false;
        if(infoText != null) 
        {
            infoText.gameObject.SetActive(true);
            infoText.text = "READY?";
            yield return new WaitForSeconds(0.8f);
            infoText.text = "GO!";
            yield return new WaitForSeconds(0.8f);
            infoText.gameObject.SetActive(false);
        }
    }
}