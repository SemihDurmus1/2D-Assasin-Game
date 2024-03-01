using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerMovement : MonoBehaviour
{
    private int health = 100;

    #region Movement
    //Movement
    [SerializeField] float speed = 7f;
    [SerializeField] float jumpForce = 18f;
    private float inputX;
    private Rigidbody2D rb;
    //Ground check
    private Sensor_Bandit m_groundSensor;
    private bool isGrounded = false;
    #endregion

    #region Animations
    private Animator animator;
    //Attack Anim
    public Transform playerAttackPoint;
    [SerializeField] float attackRange = 0.6f;
    [SerializeField] int attackDamage = 100; //Damage
    [SerializeField] float attackRate = 2f;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    public LayerMask enemyLayers;
    [SerializeField]private float waitAfterAttack = 0.48f;

    //Anim States
    private SpriteRenderer playerSprite;
    private bool combatIdle = false;
    private bool isDead = false;

    //CloakAnim
    private bool isOnCloak = false;
    Color playerColor;
    #endregion

    //Throw Stone
    public GameObject stonePrefab;
    public float stoneXForce = 15f;
    public float stoneYForce = 8f;

    void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        playerColor = playerSprite.color;

        animator = GetComponent<Animator>();
        playerAttackPoint = GameObject.FindGameObjectWithTag(nameof(playerAttackPoint)).transform;

        rb = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
    }
	
	void Update()
    {
        GroundControl();

        if (isDead) {  return; }

        CheckInput();

        CheckMovementDirection();

        ControlAnimates();
    }
    private void FixedUpdate()
    {
        if (!isAttacking)
        {
            rb.velocity = new Vector2(speed * inputX, rb.velocity.y);
        }
    }

    private void ControlAnimates()
    {/*AnimStates
        0 = Idle
        1 = CombatIdle
        2 = Run */

        animator.SetFloat("AirSpeed", rb.velocity.y);//AirSpeed 0 dan kucukse calisir
        
        if (Mathf.Abs(inputX) > Mathf.Epsilon)//Run
            animator.SetInteger("AnimState", 2);

        else//Idle
            animator.SetInteger("AnimState", 0);

        if (isAttacking && isGrounded)//Ziplarken havada bir anda durmasin diye ground control
        {
            rb.velocity = Vector2.zero;
        }
    }
    private void CheckInput()
    {
        inputX = Input.GetAxis("Horizontal");

        if (Input.GetMouseButtonDown(0))
        {
            StartAttack();
        }
        if (Input.GetButtonDown("Jump") && isGrounded)//Ziplama
        {
            StartJump();
        }
        if (Input.GetKeyDown(KeyCode.Q))//Throw object
        {
            ThrowStone();
        }
        if (Input.GetKeyDown("f"))
        {
            playerColor.a = isOnCloak ? 1f : 0.2f; //ternary if else yazimi
            playerSprite.color = playerColor;

            isOnCloak = !isOnCloak;
        }
    }

    private void ThrowStone()
    {
        GameObject firlatilanStone = Instantiate(stonePrefab, playerAttackPoint.transform.position, Quaternion.identity);

        Rigidbody2D rb = firlatilanStone.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 firlatmaYonu = transform.localScale.x < 0 ? Vector2.right : Vector2.left;

            rb.AddForce(firlatmaYonu * stoneXForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.up * stoneYForce, ForceMode2D.Impulse);

        }
    }

    private void StartJump()
    {
        animator.SetTrigger("Jump");

        isGrounded = false;
        animator.SetBool("Grounded", isGrounded);

        isAttacking = false;

        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        m_groundSensor.Disable(0.2f);
    }

    private void StartAttack()
    {
        if (Time.time >= nextAttackTime)//Attack cooldownu
        {
            isAttacking = true;

            //Eger attacka basip hemen ziplarsak nadiren x inputu alinmiyor. Bunu onlemek icin yaptim
            //Invoke(nameof(StopAttacking), waitAfterAttack);

            animator.SetTrigger("Attack");
            nextAttackTime = Time.time + 1f / attackRate;//Bir saniye sonrasinin degerini alir
        }
    }

    public void EventAttack()//AnimationEvent
    {
        // Bu kod, AttackPoint deðiþkeni içine giren tüm nesneleri listeler ve enemy olanlara damage atar
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(playerAttackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            //Her enemyye damage at
            enemy.GetComponent<EnemyManager>().TakeDamage(attackDamage);
        }
        Invoke(nameof(StopAttacking), waitAfterAttack);//Yerine sabitlemeyi kaldiri
    }

    private void GroundControl()
    {
        //Check if character just landed on the ground
        if (!isGrounded && m_groundSensor.State())
        {
            isGrounded = true;
            animator.SetBool("Grounded", isGrounded);
        }

        //Check if character just started falling
        if (isGrounded && !m_groundSensor.State())
        {
            isGrounded = false;
            animator.SetBool("Grounded", isGrounded);
        }
    }

    private void CheckMovementDirection()//Sprite flipini halleder
    {
        if (isDead) { return; }

        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    private void StopAttacking()//Vurunca yerine sabitlenme buglarini engeller
    {
        if (isAttacking)
        {
            isAttacking = false;
        }
    }

    public void GetDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            //isAttacking = false;
            if (!isAttacking)
            {
                rb.velocity = Vector2.zero;
                animator.SetTrigger("Hurt");
            }
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");

        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Animasyon bittiðinde yapýlmasý gereken iþlemler
        if (isGrounded)
        {
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.bodyType = RigidbodyType2D.Static;
            animator.enabled = false;
        }

    }
    private void OnDrawGizmosSelected()
    {
        if (playerAttackPoint == null) { return; }

        Gizmos.DrawWireSphere(playerAttackPoint.position, attackRange);
    }
}
