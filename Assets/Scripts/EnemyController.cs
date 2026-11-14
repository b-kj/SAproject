using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    [SerializeField] protected float HP = 80f;
    [SerializeField] protected float currentHP;

    protected virtual void Awake()
    {
        currentHP = HP;
    }

    public abstract void Hit(Vector2 direction);

    public virtual void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentMP += 3;
        }

        Destroy(gameObject);
    }
}