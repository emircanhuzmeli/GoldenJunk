using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TrashLauncher : MonoBehaviour
{
    public static TrashLauncher instance; 

    [Header("Tutma ve Fırlatma Ayarları")]
    public float sensitivity = 35f; 
    public float throwForce = 18f;  
    private bool isDragging = false;
    private Rigidbody currentRb;    
    private float yOffset = 1.3f; 
    private int originalLayer; 

    [Header("UI Elemanları")]
    public TextMeshProUGUI scoreText;         
    public TextMeshProUGUI finalScoreText;    
    public TextMeshProUGUI moneyEarnedText;   
    public GameObject gameOverPanel; 
    public GameObject[] hearts; 

    [Header("Elenme Ayarları")]
    public int mistakeCount = 0; 
    public int maxMistakes = 4;
    private float nextMistakeTime = 0f;
    private float mistakeCooldown = 0.5f; 

    void Awake() { 
        if (instance == null) instance = this; 
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        
        HandleInput();

        if (scoreText != null && ScoreManager.instance != null) 
            scoreText.text = "SKOR: " + ScoreManager.instance.score;

        if (mistakeCount >= maxMistakes) GameOver();
    }

    void HandleInput()
    {
        // 1. TUTMA BAŞLANGICI
        if (Input.GetMouseButtonDown(0) && !isDragging) // Eğer zaten bir şey taşımıyorsak
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Sadece "trash" katmanındaki veya etiketindeki ilk objeyi yakala
            if (Physics.Raycast(ray, out hit, 100f)) 
            {
                TrashMovement moveScript = hit.collider.GetComponent<TrashMovement>();
                if(moveScript == null) moveScript = hit.collider.GetComponentInParent<TrashMovement>();

                if (moveScript != null)
                {
                    isDragging = true;
                    currentRb = moveScript.GetComponent<Rigidbody>();
                    
                    if (currentRb != null)
                    {
                        moveScript.isGrabbed = true;
                        currentRb.isKinematic = true; // Fizik motorundan ayır (Mıknatıs etkisini önler)
                        currentRb.useGravity = false;
                        
                        originalLayer = currentRb.gameObject.layer;
                        currentRb.gameObject.layer = 2; // Ignore Raycast (Elindeyken tıklamayı engelle)
                    }
                }
            }
        }

        // 2. SÜRÜKLEME
        if (isDragging && currentRb != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; 
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(mousePos);
            targetPos.y = yOffset; 
            
            // Yumuşak takip (Lerp) - Laptop kasıyorsa Time.unscaledDeltaTime kullanabiliriz
            currentRb.position = Vector3.Lerp(currentRb.position, targetPos, Time.deltaTime * sensitivity);
        }

        // 3. BIRAKMA / FIRLATMA
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ThrowTrash();
        }
    }

    void ThrowTrash()
    {
        if (currentRb != null)
        {
            currentRb.isKinematic = false; 
            currentRb.useGravity = true;
            currentRb.gameObject.layer = originalLayer; 
            
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            // Fırlatma yönü (X: Sağ-Sol, Y: Yukarı kavis, Z: İleri)
            Vector3 throwDir = new Vector3(mouseX * 3f, 1.2f, mouseY * 5f); 
            
            currentRb.AddForce(throwDir * throwForce, ForceMode.Impulse);
            currentRb.AddTorque(Random.insideUnitSphere * 7f, ForceMode.Impulse);
            
            TrashMovement moveScript = currentRb.GetComponent<TrashMovement>();
            if(moveScript == null) moveScript = currentRb.GetComponentInParent<TrashMovement>();
            if(moveScript != null) moveScript.isGrabbed = false;
        }
        
        isDragging = false;
        currentRb = null; 
    }

    public void AddMistake()
    {
        if (Time.time < nextMistakeTime) return;

        if (mistakeCount < hearts.Length) 
        {
            hearts[mistakeCount].SetActive(false); 
        }

        mistakeCount++;
        nextMistakeTime = Time.time + mistakeCooldown;
    }

    void GameOver()
    {
        StopAllGameActions();
        
        int s = (ScoreManager.instance != null) ? ScoreManager.instance.score : 0;
        if (finalScoreText != null) finalScoreText.text = "Final Skor: " + s;
        
        int money = s / 5;
        if (moneyEarnedText != null) moneyEarnedText.text = "Money: " + money + " $";
        
        PlayerPrefs.SetInt("TotalMoney", PlayerPrefs.GetInt("TotalMoney", 0) + money);
        PlayerPrefs.Save();
        
        Invoke("RealGameOver", 0.3f);
    }

    void StopAllGameActions()
    {
        BinJuice[] bj = Object.FindObjectsByType<BinJuice>(FindObjectsSortMode.None);
        foreach (var j in bj) if (j != null) j.enabled = false; 

        ScoreDetector[] sd = Object.FindObjectsByType<ScoreDetector>(FindObjectsSortMode.None);
        foreach (var d in sd) if (d != null) d.enabled = false;

        if(TrashSpawner.instance != null) TrashSpawner.instance.canSpawn = false;
    }

    void RealGameOver() 
    { 
        Time.timeScale = 0f; 
        if (gameOverPanel != null) gameOverPanel.SetActive(true); 
    }

    public void YenidenBaslat() 
    { 
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
}