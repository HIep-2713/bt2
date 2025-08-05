using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragonfake : MonoBehaviour
{
    public float move;
    public float speed = 5f;
    public Animator ani;
    public Rigidbody2D rb;
    public bool Isfacing = true;   
    public float jumpForce = 5f; 
    public int jumpCount=0; 
    public bool isCrouching = false; 
    public Vector3 theScale;
    private bool isGrounded;
    public bool isAttacking = false ;
    public GameObject bulletPrefab; 
    public Transform firePoint;
    public Transform groundCheck; // thêm
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private float attackDuration = 0.4f; // th?i gian attack animation
    private float attackTimer = 0f;
    public bool isDizzy = false;
    public bool canDash = true;
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;

    public GameObject dashEffect; 
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
            jumpCount = 0;

        if (isAttacking || isDizzy)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDuration)
            {
                isAttacking = false;
                attackTimer = 0f;
            }
        }

        if (!isAttacking)
        {
            HandleMovement();
            HandleJump();
            HandleCrouch();
        }

        HandleAttack();
        HandleDash();
    }
    void HandleMovement()
    {
        if (isAttacking || isDizzy || isDashing)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (Input.GetKey(KeyCode.A))
            move = -1f;
        else if (Input.GetKey(KeyCode.D))
            move = 1f;
        else
            move = 0f;

        rb.velocity = new Vector2(move * speed, rb.velocity.y);

        if (move > 0 && !Isfacing)
            Flip();
        else if (move < 0 && Isfacing)
            Flip();

        ani.SetFloat("move", Mathf.Abs(move));
    }
    private void Flip()
    {
        Isfacing = !Isfacing;
        theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.W) && jumpCount < 2)
        {
            if (isGrounded || jumpCount < 1) // Allow jump if grounded or has one jump left
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                ani.SetTrigger("Jump");
                jumpCount++;
            }
        }
    }
        void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.S) && !isCrouching)
            {
                isCrouching = true;
                ani.SetBool("isCrouching", true);
            }
            else if (Input.GetKeyUp(KeyCode.S) && isCrouching)
            {
                isCrouching = false;
                ani.SetBool("isCrouching", false);
            }
        }
    void HandleAttack()
    {
        if (!isAttacking)
        {
            if (!isGrounded && Input.GetKey(KeyCode.W) && Input.GetKeyDown(KeyCode.J))
            {
                ani.SetTrigger("JumpAttack");
                isAttacking = true;
                Shoot();
            }
            else if (isGrounded && Input.GetKeyDown(KeyCode.J))
            {
                ani.SetTrigger("Attack");
                isAttacking = true;
                Shoot();
            }
        }

    }
   
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Instantiate the bullet at the firePoint's position and rotation
        GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bulletScript = bulletObject.GetComponent<Bullet>();

        // Determine the direction based on the character's facing
        Vector2 shootDirection;
        if (Isfacing)
        {
            shootDirection = Vector2.right;
        }
        else
        {
            shootDirection = Vector2.left;

            // Also flip the bullet's sprite to match the direction
            Vector3 bulletScale = bulletObject.transform.localScale;
            bulletScale.x *= -1;
            bulletObject.transform.localScale = bulletScale;
        }

        // Call the new SetDirection method on the bullet
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDirection);
        }
    }
    public void FinishAttack()
    {
        isAttacking = false;
    }
    public void TakeDamage()
    {
        if (!isAttacking && !isDizzy)
        {
            ani.SetTrigger("Hurt");
            rb.velocity = Vector2.zero;
        }
    }
    public void EnterDizzy(float duration)
    {
        if (!isDizzy)
        {
            StartCoroutine(DizzyRoutine(duration));
        }
    }

    IEnumerator DizzyRoutine(float duration)
    {
        isDizzy = true;
        ani.SetTrigger("Dizzy");
        rb.velocity = Vector2.zero;

        // Vô hi?u hóa ?i?u khi?n
        yield return new WaitForSeconds(duration);

        isDizzy = false;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Knight"))
        {
            TakeDamage();

            // N?u mu?n dizzy sau khi hurt
            EnterDizzy(2f); // 2 giây b? choáng
        }
    }
    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.K) && canDash && isGrounded && !isAttacking && !isDizzy)
        {
            StartCoroutine(DashRoutine());
        }
    }
    IEnumerator DashRoutine()
    {
        canDash = false;
        ani.SetTrigger("Dash");

        if (dashEffect != null)
        {
            dashEffect.SetActive(true);
            Animator fxAni = dashEffect.GetComponent<Animator>();
            if (fxAni != null)
            {
                fxAni.Play("DashEffect", -1, 0f);
            }
        }

        float dashDistance = 5f; // kho?ng cách dash
        float dashSpeed = dashDistance / dashDuration; // t?c ?? c?n ?? ?i ?úng kho?ng cách
        float elapsedTime = 0f;
        Vector2 direction = Isfacing ? Vector2.right : Vector2.left;

        rb.gravityScale = 0; // t?m t?t gravity ?? không b? r?i khi dash

        while (elapsedTime < dashDuration)
        {
            rb.MovePosition(rb.position + direction * dashSpeed * Time.fixedDeltaTime);
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); // dash m??t theo FixedUpdate
        }

        rb.gravityScale = 1; // khôi ph?c gravity

        if (dashEffect != null)
            dashEffect.SetActive(false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }




}

