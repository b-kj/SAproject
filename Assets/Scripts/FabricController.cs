using UnityEngine;

public class FabricController : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void spin()
    {
        transform.Rotate(0f, 0f, 90f);
    }
}
