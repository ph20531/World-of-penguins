using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveDirection;
    private float moveSpeed;
    private float moveTime;
    private float stopTime;
    private float timer;
    private bool moving = true;
    private float originalAnimationSpeed;

    public Vector2 moveSpeedRange = new Vector2(1f, 5f);
    public Vector2 moveTimeRange = new Vector2(1f, 3f);
    public Vector2 stopTimeRange = new Vector2(0.5f, 2f);

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalAnimationSpeed = animator.speed;

        SetRandomMovement();
    }

    void Update()
    {
        if (moving)
        {
            timer += Time.deltaTime;

            if (timer >= moveTime)
            {
                // 이동 멈춤
                animator.speed = 0f;
                moving = false;
                timer = 0f;
            }
            else
            {
                // 이동
                rb2d.velocity = moveDirection * moveSpeed;

                if (moveDirection.x < 0) // 왼쪽 방향인 경우
                    spriteRenderer.flipX = false;
                else if (moveDirection.x > 0) // 오른쪽 방향인 경우
                    spriteRenderer.flipX = true;
            }
        }
        else
        {
            timer += Time.deltaTime;

            if (timer >= stopTime)
            {
                // 애니메이션 이동
                animator.speed = originalAnimationSpeed;
                moving = true;
                SetRandomMovement();
            } else {
                rb2d.velocity = Vector2.zero;
            }
        }

        rb2d.angularVelocity = 0f;
    }

    void SetRandomMovement(bool isCollision = false)
    {
        // 타이머 리셋
        timer = 0f;

        if(isCollision) {
            // 충돌한 방향의 반대 방향 계산 및 설정
            Vector2 oppositeDirection = -moveDirection;
            moveDirection = oppositeDirection.normalized;
        } else {
            // 랜덤 방향 설정
            float angle = Random.Range(0f, 360f);
            moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        // 랜덤 속도 설정
        moveSpeed = Random.Range(moveSpeedRange.x, moveSpeedRange.y);

        // 랜덤 이동 시간 설정
        moveTime = Random.Range(moveTimeRange.x, moveTimeRange.y);

        // 랜덤 멈춤 시간 설정
        stopTime = Random.Range(stopTimeRange.x, stopTimeRange.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
            rb2d.velocity = Vector2.zero;
            
            // 충돌 시 반대 방향으로 이동
            animator.speed = originalAnimationSpeed;
            moving = true;
            SetRandomMovement(true);
    }
}
