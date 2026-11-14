using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;

    [Header("Dash")]
    public bool canDash = true; 
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float dashTimeCounter;
    private float dashCooldownCounter;
    private bool isDashing;

    [Header("Game Feel")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    [Header("Pogo")]
    public float pogoBounceForce = 15f; 
    public float attackDuration = 0.2f; 
    private float downAttackCounter;
    private float sideAttackCounter;
    public GameObject pogoSparkVFX; 
    
    [Header("Side Attack Recoil")]
    public float sideRecoilForceX = 15f; 
    public float sideRecoilForceY = 5f;  
    public float recoilDuration = 0.15f; 
    private float recoilCounter;
    
    public Transform downAttackPoint; 
    public Transform sideAttackPoint; 
    public float attackRadius = 0.5f;
    public LayerMask pogoLayer; 

    [Header("Audio")]
    public AudioSource sfxAudioSource;
    public AudioClip[] spikeHitSounds;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private PlayerHealth playerHealth;
    private Animator anim;

    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private float idleTimer = 0f;

    private void Awake()
    {
        // Komponenten auf Player 
        Time.timeScale = 1f; 
        
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        anim = GetComponent<Animator>();

        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // kein Input wenn Spiel pausiert oder Dash
        if (Time.timeScale == 0f || isDashing) return;

        // Tasten-Input für links/rechts und oben/unten abfragen
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Dash Cooldown
        if (dashCooldownCounter > 0f)
        {
            dashCooldownCounter -= Time.deltaTime;
        }

        // Dash Left Shift
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && dashCooldownCounter <= 0f)
        {
            idleTimer = 0f;
            StartDash();
            return;
        }

        // recoil Timer 
        if (recoilCounter > 0f)
        {
            recoilCounter -= Time.deltaTime;
        }

        // jump buffer: merkt Sprung-Tastendruck kurz vor Landen
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Attacks starten
        bool pointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (!pointerOverUI && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)))
        {
            idleTimer = 0f;
            bool isHoldingDown = verticalInput < 0f;

            // wenn S gedrückt -> Schlag nach unten (Pogo), sonst seitlicher Schlag
            if (isHoldingDown)
            {
                downAttackCounter = attackDuration; 
                if (anim != null)
                {
                    anim.SetTrigger("DownAttack");
                }
            }
            else
            {
                sideAttackCounter = attackDuration;
            }
        }

        // solange Angriff aktiv ist, auf Treffer mit Spikes prüfen
        if (downAttackCounter > 0f)
        {
            downAttackCounter -= Time.deltaTime;
            CheckDownAttack();
        }

        if (sideAttackCounter > 0f)
        {
            sideAttackCounter -= Time.deltaTime;
            CheckSideAttack();
        }

        CheckGrounded();

        // Coyote Time: Sprung kurz nach Verlassen der Kante
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // abspringen wenn Coyote Time + Jump Buffer aktiv 
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            idleTimer = 0f;
        }

        // kürzerer Sprung wenn Leertaste früh losgelassen 
        if ((Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space)) && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }

        // IdleTimer hochzählen wenn Spieler stillsteht
        bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;

        if (isMoving || !isGrounded || downAttackCounter > 0f || sideAttackCounter > 0f)
        {
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.deltaTime;
        }

        // Werte an Animator
        if (anim != null)
        {
            anim.SetBool("IsMoving", isMoving);
            anim.SetFloat("IdleTimer", idleTimer);
        }

        Flip();
    }

    private void FixedUpdate()
    {
        // Bewegung während Dash (konstante Geschwindigkeit in Blickrichtung)
        if (isDashing)
        {
            dashTimeCounter -= Time.fixedDeltaTime;
            
            if (dashTimeCounter <= 0f)
            {
                StopDash();
            }
            else
            {
                float dashDirection = isFacingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            }
            return;
        }

        // normale Bewegung links/rechts (nur wenn gerade kein Rückstoß aktiv ist)
        if (recoilCounter <= 0f)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    // Dash aktivieren
    private void StartDash()
    {
        isDashing = true;
        dashTimeCounter = dashDuration;
        dashCooldownCounter = dashCooldown;
        rb.gravityScale = 0f;
    }

    // Dash beenden + Schwerkraft einschalten
    private void StopDash()
    {
        isDashing = false;
        rb.gravityScale = 1f; 
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // prüfen ob sichere Position für Respawn 
    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        
        if (isGrounded && playerHealth != null)
        {
            playerHealth.UpdateSafePosition(transform.position);
        }
    }

    // Pogo-Check: wenn unter uns Spikes getroffen -> nach oben abprallen + Funken & Sound
    private void CheckDownAttack()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(downAttackPoint.position, attackRadius, pogoLayer);
        foreach (Collider2D target in hitTargets)
        {
            if (target.CompareTag("PogoTarget"))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);
                
                if (pogoSparkVFX != null)
                {
                    Instantiate(pogoSparkVFX, downAttackPoint.position, Quaternion.identity);
                }

                PlaySpikeHitSound();
                downAttackCounter = 0f; 
                break; 
            }
        }
    }

    // Side-Attack-Check: wenn seitlich Spikes getroffen-> Rückstoß + Funken & Sound
    private void CheckSideAttack()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(sideAttackPoint.position, attackRadius, pogoLayer);
        foreach (Collider2D target in hitTargets)
        {
            if (target.CompareTag("PogoTarget"))
            {
                float recoilDirection = isFacingRight ? -1f : 1f;
                rb.linearVelocity = new Vector2(recoilDirection * sideRecoilForceX, sideRecoilForceY); 
                recoilCounter = recoilDuration;
                
                if (pogoSparkVFX != null)
                {
                    Instantiate(pogoSparkVFX, sideAttackPoint.position, Quaternion.identity);
                }

                PlaySpikeHitSound();
                sideAttackCounter = 0f; 
                break; 
            }
        }
    }

    // zufälligen Metall-Sound Array abspielen
    private void PlaySpikeHitSound()
    {
        if (sfxAudioSource != null && spikeHitSounds != null && spikeHitSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, spikeHitSounds.Length);
            AudioClip randomClip = spikeHitSounds[randomIndex];

            if (randomClip != null)
            {
                sfxAudioSource.pitch = Random.Range(0.95f, 1.05f);
                sfxAudioSource.PlayOneShot(randomClip);
            }
        }
    }

    // Sprite X-Achse spiegeln wenn andere Richtung läuft
    private void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    // Hilfslinien für GroundCheck + Attack-Radien
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (downAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(downAttackPoint.position, attackRadius);
        }
        if (sideAttackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(sideAttackPoint.position, attackRadius);
        }
    }
}