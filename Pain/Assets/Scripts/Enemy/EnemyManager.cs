using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    #region Variables
    private Animator animator;
    private BoxCollider2D enemyCollider;
    private Rigidbody2D rb;

    [SerializeField] private int health = 100;
    public float moveSpeed = 6f;
    public float walkSpeed = 3f;

    private bool isDead = false;

    //Attack
    public Transform attackPoint;
    [SerializeField] float attackRange = 0.6f;
    public int attackDmg = 10;
    [SerializeField] float attackRate = 2f;
    private float nextAttackTime = 0f;

    //Sigh of View
    public float radius = 10f;
    public float awareRange = 1.5f;
    [Range(1, 360)] public float angle = 45f;

    //LayerMasks
    public LayerMask targetLayer;
    public LayerMask obstructionLayer;
    public LayerMask stoneLayer;

    //Obje duyma mekanigi
    private bool isHeardSomething = false;

    public GameObject playerRef;

    public bool CanSeePlayer { get; private set; }

    //Patrol ------------ Burasý gelistirilecek
    public Transform patrolPoint1, patrolPoint2;
    private int currentPatrolIndex = 0;
    private bool isMovingFirst = true;
    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        playerRef = GameObject.FindGameObjectWithTag("Player");

        StartCoroutine(FOVCheck());

        attackPoint = transform.Find(nameof(attackPoint));
    }

    private void FixedUpdate()
    {
        if (isDead) { return; }
        CheckMovementDirection();

        if (CanSeePlayer && !isDead) { ChasePlayer(); }
        //else Patrol();
    }
    void Patrol()
    {
        
    }
    private IEnumerator FOVCheck()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FOV();
        }
    }
    private void FOV()
    {
        if (isDead) { return; }


        Collider2D[] stoneCheck = Physics2D.OverlapCircleAll(transform.position, radius, stoneLayer);

        if (stoneCheck.Length > 0) 
        {
            Transform targetStone = stoneCheck[0].transform;
            Vector2 directionToStone = (targetStone.position - transform.position).normalized; //Aradaki acisal vektoru hesaplar
            float distanceToStone = Vector2.Distance(transform.position, targetStone.position);//iki pozisyon arasi uzakligi al


            if (!CanSeePlayer && isHeardSomething)
            {
                RunToPoint(targetStone.position);
            }
        }


        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);
        WhenSeePLayer(rangeCheck);
    }

    private void WhenSeePLayer(Collider2D[] rangeCheck)
    {
        if (!CanSeePlayer)
        {
            StopChasing();
        }
        if (rangeCheck.Length <= 0)//Gorus alaninda bir collider yoksa return et
        {
            StopChasing();
            return;
        }
        if (!CanSeePlayer && GameManager.Instance.IsPlayerHiding())
        {
            return;
        }

        Transform target = rangeCheck[0].transform;                                    //targetLayer olan ilk objenin transformu
        Vector2 directionToTarget = (target.position - transform.position).normalized; //Aradaki acisal vektoru hesaplar
        float distanceToTarget = Vector2.Distance(transform.position, target.position);//iki pozisyon arasi uzakligi al

        if (distanceToTarget <= awareRange)//Belli yakinliga girince arkasinda olsak bile calismasi icin
        {
            CanSeePlayer = true;
            return;
        }
        RaycastHit2D rayHit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionLayer);
        if (CanSeePlayer && !rayHit)//arada engel layer yoksa
        {
            CanSeePlayer = true;
            return; //Burada muhtemel bir bug yasanabilir, canseeplayer =true yaparak cozebiliriz belki -- cozuldu
        }
        else
        {
            StopChasing();
        }
        if (Vector2.Angle(transform.right, directionToTarget) > angle / 2)
        {//Aradaki vektor, gorus acisi vektorunden buyukse return et
            StopChasing();
            return;
        }
        if (Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionLayer))
        {//Raycast engele takiliyor ise playeri goremez
            StopChasing();
            return;
        }
        CanSeePlayer = true;//Eger tum sartlari basarili hallettiyse
    }

    //Run and Chase-----------------------------------------------------------------------------
    private void ChasePlayer()
    {
        float distanceToPlayerX = Mathf.Abs(attackPoint.position.x - GameManager.Instance.playerTransform.position.x);
        float distanceToPlayerY = Mathf.Abs(attackPoint.position.y - GameManager.Instance.playerTransform.position.y);

        if (distanceToPlayerX <= 2f && distanceToPlayerY <= 3f) // player attackRange içindeyse
        {
            StartAttack();
        }
        else if (GameManager.Instance.playerTransform.position.x > attackPoint.position.x)//player saðda ise
        {
            //Saga kos
            StartRunning(moveSpeed);
        }
        else if (GameManager.Instance.playerTransform.position.x < attackPoint.position.x)//player solda ise
        {
            //Sola kos
            StartRunning(-moveSpeed);
        }
        else
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
        }
    }
    private void StopChasing()
    {
        CanSeePlayer = false;
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
        rb.velocity = Vector2.zero;//Buraya rb yi zamanla azaltma eklenecek
    }
    private void RunToPoint(Vector2 RunPoint)
    {
        float RunPointX = Mathf.Abs(attackPoint.position.x - RunPoint.x);
        float RunPointY = Mathf.Abs(attackPoint.position.y - RunPoint.y);

        if (RunPointX <= 2f && RunPointY <= 3f) // player attackRange içindeyse
        {
            
        }
        else if (RunPoint.x > attackPoint.position.x)//player saðda ise
        {
            //Saga kos
            StartRunning(moveSpeed);
        }
        else if (RunPoint.x < attackPoint.position.x)//player solda ise
        {
            //Sola kos
            StartRunning(-moveSpeed);
        }
        else
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
        }
    }
    private void StartRunning(float moveSpeed)
    {
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isRunning", true);
    }
    //--------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        if (attackPoint != null)//Draw Attackpoint
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        //Gorus cemberi
        Gizmos.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, radius);

        //aware cemberi
        Gizmos.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, awareRange);

        //Gorus alani cizgileri
        Vector3 angleFirst = DirectionFromAngle(transform.eulerAngles.y, -angle / 2);
        Vector3 angleSecond = DirectionFromAngle(transform.eulerAngles.y, angle / 2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + angleFirst * radius);
        Gizmos.DrawLine(transform.position, transform.position + angleSecond * radius);

        if (CanSeePlayer)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerRef.transform.position);
        }
    }
    private Vector2 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector2(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
    }

    private void StartAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isAttacking", true);
            nextAttackTime = Time.time + 1f / attackRate;//Bir saniye sonrasinin degerini alir
        }

    }
    public void EventAttack()//Animation Event
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, targetLayer);

        if (hitPlayer != null)//Player hala attackrange icinde ise 
        {
            GameManager.Instance.AttackToPlayer(attackDmg);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) { return; }//This check, if death anim started already,
                               //returns null to prevent start death again 
        health -= damageAmount;
        if (health <= 0)
        {
            isDead = true;
            Die();
        }
        else
        {
            animator.SetTrigger("getDamage");
        }
    }
    private void Die()
    {
        animator.SetTrigger("death");

        enemyCollider.isTrigger = true;
        rb.bodyType = RigidbodyType2D.Static;
        this.enabled = false;
        enemyCollider.enabled = false;
        isDead = true;
    }

    public void EventDestroyEnemy()//Animation Event!!!
    {
        Destroy(gameObject);
    }

    private void CheckMovementDirection()//Sprite flipini halleder
    {
        if (rb.velocity.x > 0.2f)
        {
            transform.rotation = Quaternion.Euler(0f, 0, 0f);
        }
        else if (rb.velocity.x < - 0.8f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }
}
