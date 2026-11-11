using UnityEngine;

public class Targetable : MonoBehaviour
{
    private bool isColliding = false;
    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (isColliding)
        {
            spriteRenderer.color = new Color(0, 0, 1);
        }
        else
        {
            spriteRenderer.color = new Color(1, 1, 1);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Aiming"))
        {
            isColliding = true;
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Aiming"))
        {
            isColliding = false;
        }
    }
}
