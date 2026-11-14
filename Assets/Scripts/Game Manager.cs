using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] public int HP = 6;
    [SerializeField] public int MP = 10;
    [SerializeField] public int CurrentHP = 6;
    [SerializeField] public float CurrentMP = 0;
    [SerializeField] public float regenInterval = 5f;
    [SerializeField] public float jumpGaze = 3;
    [SerializeField] public float currnetjumpGaze = 0;

    [Header("Heart Sprites")]
    public Sprite Heart;
    public Sprite Dead;

    public Image[] Health;
    public Slider Mama;
    public Slider Jump;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        HP = 6;
        MP = 10;
        currnetjumpGaze = jumpGaze;

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        CurrentHP = 6;
        CurrentMP = 0;
        StartCoroutine(AutoRegenMP());
    }

    private void Update()
    {
        Mama.value = CurrentMP;
        Jump.value = currnetjumpGaze;
    }
    private System.Collections.IEnumerator AutoRegenMP()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);

            if (CurrentMP < MP)
            {
                CurrentMP += 1;
                CurrentMP = Mathf.Min(CurrentMP, MP); // 최대 MP 초과 방지
            }
        }
    }

    public void TakeDamage()
    {
        HP--;
        Health[HP].sprite = Dead;
    }
    public void AddHP()
    {
        HP++;
        Health[HP].sprite = Heart;
    }
    public void UseJumpGaze(float amount)
    {
        currnetjumpGaze -= amount;
        currnetjumpGaze = Mathf.Max(0, currnetjumpGaze);
    }

    public void ResetJumpGaze()
    {
        currnetjumpGaze = jumpGaze;
    }
}