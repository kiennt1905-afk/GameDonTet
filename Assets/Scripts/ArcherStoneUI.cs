using UnityEngine;
using UnityEngine.UI;

public class ArcherStoneUI : MonoBehaviour
{
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] atkSprites;
    public Sprite[] hitSprites;

    public float frameRate = 12f;
    private Image img;
    private Sprite[] currentAnim;
    private int frameIndex = 0;
    private float timer = 0;

    public enum State { Idle, Walk, Atk, Hit }
    private State currentState = State.Idle;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    public void Init(Sprite[] idle, Sprite[] walk, Sprite[] atk, Sprite[] hit)
    {
        idleSprites = idle;
        walkSprites = walk;
        atkSprites = atk;
        hitSprites = hit;
        SetState(State.Idle);
    }

    public void SetState(State newState)
    {
        if (currentState == newState && currentAnim != null) return;
        currentState = newState;
        frameIndex = 0;
        timer = 0;
        switch (newState)
        {
            case State.Idle: currentAnim = idleSprites; break;
            case State.Walk: currentAnim = walkSprites; break;
            case State.Atk: currentAnim = atkSprites; break;
            case State.Hit: currentAnim = hitSprites; break;
        }
        UpdateFrame();
    }

    void Update()
    {
        if (currentAnim == null || currentAnim.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            frameIndex = (frameIndex + 1) % currentAnim.Length;
            UpdateFrame();
        }
    }

    private void UpdateFrame()
    {
        if (currentAnim != null && currentAnim.Length > 0 && img != null)
        {
            img.sprite = currentAnim[frameIndex];
        }
    }
}
