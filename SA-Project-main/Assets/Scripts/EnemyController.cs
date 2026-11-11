using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private int moveDirection = 1;
    private bool isKnockback;
    private bool isAttacking;

    [SerializeField] private float HP = 80f;
    [SerializeField] private float currentHP = 0f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float knockBack = 10f;

    private Vector2 attackSize = new Vector2(3f, 3f);
    private float attackDistance = 2.5f;

    void Awake()
    {   
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        currentHP = HP;

        Move();
    }
    void FixedUpdate()
    {
        if (isKnockback) return;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Vector2 direction = Vector2.right * moveDirection;

        Vector2 attackCenter =
            (Vector2)transform.position + direction * attackDistance;

        // 플레이어 감지
        Collider2D hit = Physics2D.OverlapBox(
    attackCenter,
    attackSize,
    0f
);

        if (hit != null && hit.CompareTag("Player") && !isAttacking)
        {
            PlayerCombat playerCombat =  hit.GetComponentInParent<PlayerCombat>();

            if (playerCombat != null) StartCoroutine(Attack());

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        Vector2 frontVec = new Vector2(rb.position.x + moveDirection,rb.position.y);

        RaycastHit2D rayGround = Physics2D.Raycast(frontVec, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        RaycastHit2D rayWall = Physics2D.Raycast(rb.position, Vector2.right * moveDirection, 1f, LayerMask.GetMask("Ground"));

        if (rayGround.collider == null || rayWall.collider != null) Turn();
        
    }

    void Move()
    {
        moveDirection = 1;
        spriteRenderer.flipX = moveDirection == -1;

    }

    void Turn()
    {
        moveDirection *= -1;
        spriteRenderer.flipX = moveDirection == -1;
    }
    public void Hit(Vector2 direction)
    {
        if (isKnockback) return;

        isKnockback = true;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(direction * knockBack);

        if (Mathf.Sign(direction.x) != moveDirection) Turn();

        Invoke(nameof(EndKnockback), 0.3f);
    }
    private IEnumerator Attack()
    {
        isAttacking = true;

        // 공격 준비 시간
        anim.SetTrigger("isWaiting");
        yield return new WaitForSeconds(1f);

        Vector2 direction = Vector2.right * moveDirection;

        Vector2 attackCenter = (Vector2)transform.position + direction * attackDistance;

        // 공격 시점에 플레이어 재확인
        Collider2D hit = Physics2D.OverlapBox( attackCenter, attackSize, 0f);
        anim.SetTrigger("isAttack");
        if (hit != null && hit.CompareTag("Player"))
        {
            PlayerCombat playerCombat = hit.GetComponentInParent<PlayerCombat>();

            if (playerCombat != null)
            {
                playerCombat.Hit(direction);
            }
        }

        isAttacking = false;
        anim.SetBool("isWalk", true);
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
        spriteRenderer.color = new Color(1, 0, 0);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.CurrentMP += 3;
        Destroy(gameObject);
    }
    void EndKnockback()
    {
        isKnockback = false;
    }
    private void OnDrawGizmos()
    {
        Vector2 direction = Vector2.right * (moveDirection == 0 ? 1 : moveDirection);

        Vector2 attackCenter = (Vector2)transform.position + direction * 2.5f;

        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(attackCenter, new Vector2(3f, 3f));
    }
}
