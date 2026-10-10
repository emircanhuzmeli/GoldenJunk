using UnityEngine;
using System.Collections;

public class BinJuice : MonoBehaviour
{
    private Vector3 originalPos;
    private Vector3 originalScale;
    private bool isShaking = false;
    private bool isBouncing = false;

    [Header("Ayarlar")]
    // ScoreManager bunu oyun içinde günceller (Blue, Yellow, Red, Green)
    public string binType; 
    public float shakeAmount = 0.06f; 
    public float wrongShakeMultiplier = 4.0f; 

    void Start()
    {
        originalPos = transform.localPosition;
        originalScale = transform.localScale;
    }

    // NOT: Eğer ScoreDetector puan veriyorsa, buradaki OnTriggerEnter sadece EFEKT içindir.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("trash") || other.CompareTag("Trash"))
        {
            TrashMovement trash = other.GetComponent<TrashMovement>();
            if (trash == null) trash = other.GetComponentInParent<TrashMovement>();

            if (trash != null)
            {
                string tColor = trash.trashColor.Trim().ToLower();
                string bColor = binType.Trim().ToLower();

                if (tColor == bColor)
                {
                    // DOĞRU: Zıpla (Puanı ScoreDetector verecek, biz sadece zıplıyoruz)
                    PlayBounce(); 
                }
                else
                {
                    // YANLIŞ: Salla (Hatayı ScoreDetector işleyecek, biz sadece sallıyoruz)
                    PlayWrongShake();
                }
                
                // Torba yok etme işlemini ScoreDetector yapmıyorsa burası yapmalı:
                // Destroy(trash.gameObject);
            }
        }
    }

    // --- EFEKT FONKSİYONLARI (Kusursuz Çalışır) ---
    public void PlayBounce() 
    { 
        if (!isBouncing && gameObject.activeInHierarchy) StartCoroutine(BounceRoutine()); 
    }

    public void PlayWrongShake() 
    { 
        if (!isShaking && gameObject.activeInHierarchy) StartCoroutine(ShakeRoutine(shakeAmount * wrongShakeMultiplier)); 
    }

    private IEnumerator BounceRoutine()
    {
        isBouncing = true;
        transform.localScale = originalScale * 1.15f; // Biraz daha belirgin zıplama
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
        isBouncing = false;
    }

    private IEnumerator ShakeRoutine(float amount)
    {
        isShaking = true;
        float timer = 0;
        while (timer < 0.25f)
        {
            timer += Time.deltaTime;
            transform.localPosition = originalPos + Random.insideUnitSphere * amount;
            yield return null;
        }
        transform.localPosition = originalPos;
        isShaking = false;
    }

    public void StopEverything()
    {
        StopAllCoroutines();
        transform.localPosition = originalPos;
        transform.localScale = originalScale;
        this.enabled = false;
    }
}