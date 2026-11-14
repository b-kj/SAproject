using UnityEngine;
using UnityEngine.EventSystems;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public float speed;
    private int moveDirection;
    public LayerMask isLayer;
    private Vector2 direction;

    public void SetDirection(int dir)
    {
        moveDirection = dir;
        direction = Vector2.right * moveDirection;
        spriteRenderer.flipX = moveDirection == -1;
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Invoke(nameof(DestroyBullet), 2f);
    }
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCombat playerCombat = collision.GetComponentInParent<PlayerCombat>();

            if (playerCombat != null)  playerCombat.Hit(direction);
            DestroyBullet();
        }
    }
    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}