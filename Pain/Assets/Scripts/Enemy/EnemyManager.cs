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

    private bool isDead = false;

    //Attack
    public Transform attackPoint;
    [SerializeField] float attackRange = 0.6f;
    public int attackDmg = 10;
    [SerializeField] float attackRate = 2f;
    private float nextAttackTime = 0f;

    //Sigh of View
    public float radius = 10f;
    [Range(1, 360)] public float angle = 45f;
    public LayerMask targetLayer;
    public LayerMask obstructionLayer;

    public GameObject playerRef;

    public bool CanSeePlayer {  get; private set; }
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
    private void FOV()//Kodun sýra iþleyiþi sýkýntýlý, bir kere gördü mü, arada engel layer olsa da görüyor
    {
        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);

        if (!CanSeePlayer)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
        }

        if (rangeCheck.Length <= 0)//Gorus alaninda bir collider yoksa return et
        {
            CanSeePlayer = false;
            animator.SetBool("isRunning", false);
            return;
        }

        Transform target = rangeCheck[0].transform;//Layeri targetLayer olan ilk objenin transformunu al
        Vector2 directionToTarget = (target.position - transform.position).normalized;//Aradaki acisal vektoru hesaplar
        float distanceToTarget = Vector2.Distance(transform.position, target.position);//iki pozisyon arasi uzakligi al

        if (CanSeePlayer && !Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionLayer))
        {//Eger bir kere gorus alani icine girmisse, aci disinda da olsak dusman range boyunca gorebilecek 
            return;
        }
        else
        {
            CanSeePlayer = false;
            animator.SetBool("isRunning", false);
        }
        if (Vector2.Angle(transform.right, directionToTarget) > angle / 2)
        {//Aradaki vektor, gorus acisi vektorunden buyukse return et
            CanSeePlayer = false;
            animator.SetBool("isRunning", false);
            return;
        }
        if (Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionLayer))
        {//Atilan raycast bir engel layerina(obstructionLayer) takiliyor ise playeri goremez
            CanSeePlayer = false;
            animator.SetBool("isRunning", false);
            return;
        }
        CanSeePlayer = true;//Eger tum sartlari basarili hallettiyse
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)//Draw Attackpoint
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        //Beyaz cember
        Gizmos.color = Color.white;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, radius);

        //Acilar
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

    private void ChasePlayer()
    {
        float distanceToPlayer = Mathf.Abs(attackPoint.position.x - GameManager.Instance.playerTransform.position.x);

        if (distanceToPlayer <= 2f) // player attackRange içindeyse
        {
            StartAttack();
        }
        else if (attackPoint.position.x + 1f < GameManager.Instance.playerTransform.position.x)//player sagda ise
        {
            //Saga kos
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isRunning",true);
        }
        else if(attackPoint.position.x -1f > GameManager.Instance.playerTransform.position.x)//player solda ise
        {
            //Sola kos
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
        }
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

    //private IEnumerator KillAfterAnimation()
    //{
    //    // Animasyonun uzunluguna gore bekle
    //    yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    //}
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

        isDead = true;
    }

    public void EventDestroyEnemy()//Animation Event!!!
    {
        Destroy(gameObject);
    }

    private void CheckMovementDirection()//Sprite flipini halleder
    {
        if (rb.velocity.x > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, 0, 0f);
        }
        else if (rb.velocity.x < - 0.9f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }
}
