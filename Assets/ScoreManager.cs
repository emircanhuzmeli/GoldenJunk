using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI Elemanları")]
    public TextMeshProUGUI scoreText; 

    [Header("4 Sabit Alan (0:Blue, 1:Red, 2:Yellow, 3:Green)")]
    public List<GameObject> fixedAreas = new List<GameObject>(); 

    [Header("Renk Materyalleri")]
    public Material blueMat;
    public Material yellowMat;
    public Material greenMat;
    public Material redMat;

    [Header("Oyun Ayarları")]
    public int score = 0;
    public int activeAreaCount = 2; 
    public float currentGlobalSpeed = 3.5f;
    public float speedIncreasePerLevel = 0.5f; // Hız artışı biraz daha belirgin

    void Awake() 
    { 
        if (instance == null) instance = this; 
    }

    void Start()
    {
        score = 0;
        activeAreaCount = 2; 
        currentGlobalSpeed = 3.5f;
        UpdateUI();
        
        // Başlangıç gecikmesini biraz artırdım ki sahneler tam yüklensin
        Invoke("SetupInitialState", 0.2f); 
    }

    void SetupInitialState()
    {
        if (fixedAreas.Count < 4) return;
        
        // Senin dizilimine göre (0-3 Mavi, 1-2 Sarı)
        ApplySettings(fixedAreas[0], "Blue", blueMat);
        ApplySettings(fixedAreas[3], "Blue", blueMat);
        
        ApplySettings(fixedAreas[1], "Yellow", yellowMat);
        ApplySettings(fixedAreas[2], "Yellow", yellowMat);
    }

    public void AddScore(int amount)
    {
        score += amount;
        CheckLevelProgress();
        UpdateUI();
    }

    void CheckLevelProgress()
    {
        // 1. AŞAMA: 300 PUAN -> KIRMIZI (Zorunlu Güncelleme)
        if (score >= 300 && activeAreaCount == 2)
        {
            activeAreaCount = 3;
            currentGlobalSpeed += speedIncreasePerLevel;
            ApplySettings(fixedAreas[1], "Red", redMat);
            Debug.Log("<color=red>DİKKAT: KIRMIZI ALAN AKTİF!</color>");
        }

        // 2. AŞAMA: 500+ PUAN -> KAOS MODU (Hızlanma ve Karıştırma)
        if (score >= 500 && score % 100 == 0) 
        {
            currentGlobalSpeed += 0.3f;
            ShuffleAreas();
        }

        // 3. AŞAMA: 800 PUAN -> YEŞİL
        if (score >= 800 && activeAreaCount == 3)
        {
            activeAreaCount = 4;
            currentGlobalSpeed += speedIncreasePerLevel;
            ApplySettings(fixedAreas[3], "Green", greenMat);
            Debug.Log("<color=green>DİKKAT: YEŞİL ALAN AKTİF!</color>");
        }
    }

    public void ApplySettings(GameObject area, string color, Material mat)
    {
        if (area == null) return;
        
        // GetComponent yerine GetComponentInChildren kullanarak dedektörü garantiye alıyoruz
        ScoreDetector det = area.GetComponentInChildren<ScoreDetector>();
        MeshRenderer rend = area.GetComponent<MeshRenderer>();
        BinJuice bin = area.GetComponentInChildren<BinJuice>();

        if (det != null) 
        {
            det.targetColor = color; 
            Debug.Log(area.name + " ayarlandı: " + color);
        }

        if (rend != null && mat != null) rend.material = mat;
        if (bin != null) bin.binType = color;
    }

    public void ShuffleAreas()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var area in fixedAreas) {
            if(area != null) positions.Add(area.transform.position);
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 temp = positions[i];
            int randomIndex = Random.Range(i, positions.Count);
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }

        for (int i = 0; i < fixedAreas.Count; i++)
        {
            if(fixedAreas[i] != null) fixedAreas[i].transform.position = positions[i];
        }
    }

    public void WrongDrop()
    {
        score -= 5;
        if (score < 0) score = 0;
        UpdateUI();
        if (TrashLauncher.instance != null) TrashLauncher.instance.AddMistake();
    }

    void UpdateUI() { if (scoreText != null) scoreText.text = "SKOR: " + score; }
}