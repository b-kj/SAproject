using UnityEngine;
using System.Collections;

public class RangedController : EnemyController
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform pos;

    private int moveDirection = 1;
    private bool isKnockback;
    private bool isAttacking;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float knockBack = 10f;

    private float attackDistance = 5f;
    public LayerMask isLayer;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

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
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;

        // 플레이어 감지

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, attackDistance, isLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player") && !isAttacking)
        {
            PlayerCombat playerCombat = hit.collider.GetComponentInParent<PlayerCombat>();

            if (playerCombat != null)
            {

                StartCoroutine(Attack());

                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }

        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        Vector2 frontVec = new Vector2(rb.position.x + moveDirection, rb.position.y);

        RaycastHit2D rayGround = Physics2D.Raycast(frontVec, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        RaycastHit2D rayWall = Physics2D.Raycast(frontVec + new Vector2(0f, 1f), Vector2.right * moveDirection, 0.1f, LayerMask.GetMask("Ground", "Target"));

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
    private IEnumerator Attack()
    {
        isAttacking = true;

        // 공격 준비
        anim.SetBool("isWalk", false);
        anim.SetTrigger("isWaiting");
        yield return new WaitForSeconds(1f);

        // 공격 실행
        anim.SetTrigger("isAttack");

        GameObject newBullet = Instantiate(bullet, pos.position + new Vector3(2f * moveDirection, 0f, 0f), Quaternion.identity);
        Bullet bulletScript = newBullet.GetComponent<Bullet>();

        bulletScript.SetDirection(moveDirection);

        // 공격 애니메이션 유지 시간
        yield return new WaitForSeconds(0.8f);

        // 공격 종료
        isAttacking = false;
        anim.SetBool("isWalk", true);
    }
    public override void Hit(Vector2 direction)
    {
        if (isKnockback) return;

        isKnockback = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockBack);

        if (Mathf.Sign(direction.x) != moveDirection) Turn();

        Invoke(nameof(EndKnockback), 0.3f);
    }
    void EndKnockback()
    {
        isKnockback = false;
    }
    private void OnDrawGizmos()
    {
        Vector2 direction = Vector2.right * (
            moveDirection == 0 ? 1 : moveDirection
        );

        Vector2 rayOrigin = (Vector2)transform.position
                            + Vector2.up * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayOrigin, direction * attackDistance);
    }
}
