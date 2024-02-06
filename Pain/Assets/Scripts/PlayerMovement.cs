using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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
    //Animation
    private Animator animator;
    //Attack Anim
    public Transform playerAttackPoint;
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] int attackDamage = 100; //Damage
    [SerializeField] float attackRate = 2f;
    private float nextAttackTime = 0f;
    public LayerMask enemyLayers;

    private bool combatIdle = false;
    private bool isDead = false;

    private bool isAttacking = false;
    private float waitAfterAttack = 0.65f;
    #endregion


    public GameObject stonePrefab;
    public float stoneXForce = 15f;
    public float stoneYForce = 8f;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerAttackPoint = GameObject.FindGameObjectWithTag(nameof(playerAttackPoint)).transform;

        rb = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
    }
	
	void Update()
    {
        GroundControl();

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
    {
        //Set AirSpeed in animator
        animator.SetFloat("AirSpeed", rb.velocity.y);

        //Death
        if (Input.GetKeyDown("e"))
        {
            if (!isDead)
                animator.SetTrigger("Death");
            else
                animator.SetTrigger("Recover");

            isDead = !isDead;
        }

        //Hurt
        else if (Input.GetKeyDown("q"))
            animator.SetTrigger("Hurt");

        //Attack
        else if (Input.GetMouseButtonDown(0))
        {
            if (Time.time >= nextAttackTime)
            {
                isAttacking = true;
                Invoke(nameof(StopAttacking), waitAfterAttack);//Eger attacka basip hemen ziplarsak nadiren x inputu alinmiyor.
                                                  //Bunu onlemek icin yaptim

                animator.SetTrigger("Attack");

                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

        //Change between idle and combat idle
        else if (Input.GetKeyDown("f"))
            combatIdle = !combatIdle;


        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
            animator.SetInteger("AnimState", 2);

        //Combat Idle
        else if (combatIdle)
            animator.SetInteger("AnimState", 1);

        //Idle
        else
            animator.SetInteger("AnimState", 0);
    }

    public void EventAttack()//AnimationEvent
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(playerAttackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyManager>().GetDamage(attackDamage);
        }
        Invoke(nameof(StopAttacking), waitAfterAttack);
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

    private void CheckInput()
    {
        inputX = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            animator.SetTrigger("Jump");
            isGrounded = false;
            isAttacking = false;

            animator.SetBool("Grounded", isGrounded);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            m_groundSensor.Disable(0.2f);
        }
        if (Input.GetKeyDown(KeyCode.Q))
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
    }

    private void CheckMovementDirection()
    {
        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    private void StopAttacking()
    {
        if (isAttacking)
        {
            isAttacking = false;
        }
    }



    private void OnDrawGizmosSelected()
    {
        if (playerAttackPoint == null) { return; }

        Gizmos.DrawWireSphere(playerAttackPoint.position, attackRange);
    }
}
