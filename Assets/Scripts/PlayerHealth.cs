using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;

    [Header("Respawn")]
    [Tooltip("Mindestabstand zu Spikes/Water")]
    public float safeRadius = 1.8f;
    private Vector2 levelStartPosition;
    private Vector2 lastSafePosition;
    private Rigidbody2D rb;
    private bool isDead = false;
    private float damageCooldown = 0f;

    [Header("UI")]
    public Image[] heartImages;

    void Start()
    {
        // volle Leben + Startposition als ersten Respawn-Punkt
        currentLives = maxLives;
        rb = GetComponent<Rigidbody2D>();
        levelStartPosition = transform.position;
        lastSafePosition = transform.position;
        UpdateUI();
    }

    void Update()
    {
        // kurze Unverwundbarkeit nach Treffer
        if (damageCooldown > 0f)
        {
            damageCooldown -= Time.deltaTime;
        }
    }

    public void UpdateSafePosition(Vector2 newPos)
    {
        // nichts speichern wenn tot oder gerade getroffen
        if (isDead || damageCooldown > 0f) return;

        // speichern wenn ruhig steht 
        if (rb != null && Mathf.Abs(rb.linearVelocity.y) > 0.1f) return;

        // check ob Spikes oder Toxic Water in Nähe
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(newPos, safeRadius);
        foreach (Collider2D col in nearbyColliders)
        {
            if (col.CompareTag("PogoTarget") || col.CompareTag("Deathzone"))
            {
                return;
            }
        }

        // Punkt ist sicher -> neuen Respawn-Punkt
        lastSafePosition = newPos;
    }

    public void TakeDamage()
    {
        // verhindert Mehrfach-Schaden
        if (isDead || damageCooldown > 0f) return;

        damageCooldown = 0.5f;

        // roten Screen-Flash 
        DamagePostProcessing postProcessing = FindAnyObjectByType<DamagePostProcessing>();
        if (postProcessing == null)
        {
            GameObject autoVol = new GameObject("AutoDamageVolume");
            autoVol.AddComponent<Volume>();
            postProcessing = autoVol.AddComponent<DamagePostProcessing>();
        }
        postProcessing.Flash();

        // 1 Leben abziehen + Herzen-UI updaten
        currentLives--;
        UpdateUI();

        // wenn noch Leben übrig -> am letzten sicheren Punkt spawnen, sonst sterben
        if (currentLives > 0)
        {
            Respawn();
        }
        else
        {
            Die();
        }
    }

    private void Respawn()
    {
        // Schwung stoppen -y Respawn nicht wegrutscht
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // leicht über letzten sicheren Position
        transform.position = lastSafePosition + Vector2.up * 0.3f;
    }

    private void Die()
    {
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // im GameManager +1 Try 
        foreach (GameManager gm in FindObjectsByType<GameManager>(FindObjectsSortMode.None))
        {
            gm.AddTryAndSave();
        }

        // kurzen Reset zum Level-Anfang 
        StartCoroutine(DeathSequence());
    }

    private System.Collections.IEnumerator DeathSequence()
    {
        // halbe Sekunde warten nach Tod
        yield return new WaitForSeconds(0.5f);

        // zurück an Level-Anfang setzen
        transform.position = levelStartPosition;
        lastSafePosition = levelStartPosition;

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Herzen voll auffüllen
        currentLives = maxLives;
        UpdateUI();
        isDead = false;
    }

    private void UpdateUI()
    {
        // Herzen-Bilder ein- oder ausblenden
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].enabled = i < currentLives;
            }
        }
    }

    // Schaden durch Trigger-Gefahren
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Deathzone") || collision.CompareTag("PogoTarget"))
        {
            TakeDamage();
        }
    }

    // dauerhafter Schaden falls in Deathzone bleibt
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Deathzone"))
        {
            TakeDamage();
        }
    }

    // Schaden bei normaler Kollision mit Spikes oder Deathzone
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PogoTarget") || collision.gameObject.CompareTag("Deathzone"))
        {
            TakeDamage();
        }
    }
}