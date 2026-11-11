using UnityEngine;
using System.Collections;
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] [Range(0.0f, 1.0f)] private float slowDownTime = 0.1f;
    [SerializeField] private float slowDownRate = 20f;
    [SerializeField] private float attackDistance = 5f;
    [SerializeField] private float attackTime = 1f;
    [SerializeField] private float hitStopTime = 0.1f;
    [SerializeField] private float knockBack = 10f;
    [SerializeField] private float CriticalRate = 25f;
    [SerializeField] public float critMultiplier = 1.5f;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject target;

    [Header("Skill 1")]
    [SerializeField] private float SkillCool1 = 8f;
    [SerializeField] private float SkillTimer1 = 0f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float Damage1 = 30;
    [SerializeField] private int AttackCount = 3;
    private bool isUsingSkill1 = false;
    private bool UsingSkill = false;

    [HideInInspector] public bool slowMotion = false;
    private float attackTimer = 0f;
    private float hitStopTimer = 0f;
    private PlayerController playerController;
    private bool enterSlowMotion = false;
    private bool isKnockback;
    public bool IsKnockback => isKnockback;

    private Rigidbody2D rb;
    [SerializeField] private TargetController targetController;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleSlowMotion();
        
        if (Input.GetButtonDown("Attack") && attackTimer <= 0) //공격
        {
            Attack();
            attackTimer = attackTime;
        }
        if (Input.GetButtonDown("Skill1") && SkillTimer1 <= 0 && enterSlowMotion) //스킬1
        {
            skillThreeAttack();
            UsingSkill = true;
        }
        if (!Input.GetButton("Slow Motion")) UsingSkill = false;
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (SkillTimer1 > 0)  SkillTimer1 -= Time.deltaTime; 
        if (hitStopTimer > 0) hitStopTimer -= Time.unscaledDeltaTime; 
    }
    
    void HandleSlowMotion() //시간조정
    {
        slowMotion = Input.GetButton("Slow Motion") && !UsingSkill;
        overlay.SetActive(slowMotion);
        target.SetActive(slowMotion);
        if (slowMotion)
        {
            if (!enterSlowMotion)
            {
                targetController.ReloadTargetList();
                enterSlowMotion = true;
                anim.SetBool("isSlow", true);
            }
        }
        else
        {
            enterSlowMotion = false;
            anim.SetBool("isSlow", false);
        }
        if (slowMotion)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, slowDownTime, slowDownRate * Time.unscaledDeltaTime);
        }
        else if (hitStopTimer > 0)
        {
            Time.timeScale = 0;
        }
        else
        {   
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, slowDownRate * Time.unscaledDeltaTime);
        }
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    private bool RollCritical(float chance)
    {
        float randomValue = Random.Range(0f, 100f);
        return randomValue <= chance;
    }
    void Attack() //일반 공격(밀치기)
    {
        Vector2 direction = Vector2.right * playerController.direction;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(attackDistance, 1f), 0f, direction, attackDistance, LayerMask.GetMask("Target"));
        DrawBox2D(transform.position, new Vector2(attackDistance, 1f), 0f, Color.red, 1f);
        DrawBox2D((Vector2)transform.position + direction * attackDistance, new Vector2(attackDistance, 1f), 0f, Color.yellow, 1f);
        if (hit.collider != null)
        {
            EnemyController enemyController = hit.collider.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                if (hit.collider.CompareTag("Enemy")) targetController.lastAttackTarget = hit.collider.gameObject;
                enemyController.Hit(direction);
            }
        }
    }

    void skillThreeAttack()
    {
        if (isUsingSkill1) return;

        GameObject targetObject = targetController.CurrentTarget;
        Debug.Log("타겟 이름: " + targetObject.name);
        Debug.Log("타겟 태그: " + targetObject.tag);
        if (targetObject == null || targetObject.tag != "Enemy") return;

        StartCoroutine(SkillCoroutine1(targetObject));
    }

    private IEnumerator SkillCoroutine1(GameObject targetObject)
    {
        isUsingSkill1 = true;
        SkillTimer1 = SkillCool1;
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < AttackCount; i++)
        {
            if (targetObject == null) break;

            EnemyController enemy = targetObject.GetComponent<EnemyController>();

            if (enemy == null) break;
            float Damage = Damage1;
            bool isCrit = RollCritical(CriticalRate);
            if (isCrit) Damage *= critMultiplier;
            enemy.TakeDamage(Damage);

            if (i < AttackCount - 1)
                yield return new WaitForSeconds(tickInterval);
        }

        isUsingSkill1 = false;
    }

    public void Hit(Vector2 direction) //피격판정
    {
        hitStopTimer = hitStopTime;
        GameManager.Instance.TakeDamage();
        anim.SetTrigger("hurt");

        isKnockback = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockBack, ForceMode2D.Impulse);

        CancelInvoke(nameof(EndKnockback));
        Invoke(nameof(EndKnockback), 0.3f);
    }

    void EndKnockback()
    {
        isKnockback = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Vector2 direction = new Vector2(Mathf.Sign(transform.position.x - collision.transform.position.x), 0);
            Hit(direction);
        }
    }

    // debug
    void DrawBox2D(Vector2 origin, Vector2 size, float angle, Color color, float duration = 0f)
    {
        Vector2 half = size / 2f;

        // Local corners (before rotation)
        Vector2[] corners = new Vector2[]
        {
            new Vector2(-half.x, -half.y),
            new Vector2( half.x, -half.y),
            new Vector2( half.x,  half.y),
            new Vector2(-half.x,  half.y)
        };

        // Rotate and translate corners
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        for (int i = 0; i < corners.Length; i++)
        {
            float x = corners[i].x * cos - corners[i].y * sin;
            float y = corners[i].x * sin + corners[i].y * cos;
            corners[i] = new Vector2(x, y) + origin;
        }

        // Draw edges
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 a = corners[i];
            Vector2 b = corners[(i + 1) % corners.Length];
            Debug.DrawLine(a, b, color, duration);
        }
    }
}
