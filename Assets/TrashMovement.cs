using UnityEngine;

public class TrashMovement : MonoBehaviour
{
    public bool isGrabbed = false;
    public string trashColor; 
    public float currentSpeed = 3.5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        // ScoreManager'dan başlangıç hızını al
        if (ScoreManager.instance != null) currentSpeed = ScoreManager.instance.currentGlobalSpeed;
    }

    void Update()
    {
        // Tutulmuyorsa bantta aksın
        if (!isGrabbed)
        {
            transform.Translate(Vector3.back * currentSpeed * Time.deltaTime);
            
            // Banttan düşerse yok et ve can düşür
            if (transform.position.z < -12f) {
                if (ScoreManager.instance != null) ScoreManager.instance.WrongDrop();
                Destroy(gameObject);
            }
        }
    }
}