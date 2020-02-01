using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public string RestartLevelName;

    [Header("Animation")]
    public Animator animator;
    private SpriteRenderer spriteRenderer;

    //需要状态机：站立，跑步，攻击，跳跃，下落，下蹲，受击。(7个状态）
    //身体状态：手（攻击，拾取），脚（跳更高，下蹲）。
    //设置名字，手，脚，头，身体
    private enum State
    {
        STATE_IDLE,
        STATE_RUN,
        STATE_JUMP,
        STATE_FALL,
        STATE_ATTACK,
        STATE_CROUCH,
        STATE_GETHIT
    };
    State state_;

    [Header("Control")]
    public float moveSpeed;
    public float maxSpeed;
    public float maxFallSpeed;
    public float jumpSpeed;
    public float distance;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("BodyParts")]
    public GameObject head;
    public GameObject arm;
    public GameObject foot;

    [Header("Attack")]

    [Header("Sound")]
    public AudioClip[] cantSpray;
    public AudioSource audioSource;

    private float attackReady;
    private float HorizontalSpeed;
    private float VerticalSpeed;
    private Rigidbody2D rb;
    private BoxCollider2D bc;
    private bool ground;
    private bool jumpKey;
    private bool touchGround;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        State state_ = State.STATE_IDLE;
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        ground = false;
    }
    void Update()
    {
        Debug.Log(state_);
        IsGround();
        switch (state_)
        {
            //发呆，一定在地板上，身体部位不确定
            case State.STATE_IDLE:
                //左右键移动 run
                //不在地板上掉落 fall
                //空格攻击 attack
                //上键 jump
                //下键--》检查脚--》crouch
                //受击
                if (Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.D))
                {
                    //animator.SetInteger("State", 4);
                    state_ = State.STATE_RUN;
                }
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    //animator.SetInteger("State", 1);
                    state_ = State.STATE_JUMP;
                }
                else if(Input.GetKeyDown(KeyCode.S))
                {
                    state_ = State.STATE_CROUCH;
                }
                else if (!ground)
                {
                    //animator.SetInteger("State", 2);
                    state_ = State.STATE_FALL;
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    //animator.SetInteger("State", 3);
                    state_ = State.STATE_ATTACK;
                }
                break;

            case State.STATE_RUN:
                Run();
                break;

            case State.STATE_ATTACK:

                break;

            case State.STATE_JUMP:
               
                break;

            case State.STATE_FALL:

                break;

            case State.STATE_CROUCH:

                break;
        }
    }
    void FixedUpdate()
    {

    }

    void IsGround()
    {
        Vector2 start = groundCheck.position;
        Vector2 end = new Vector2(start.x, start.y - distance);

        Debug.DrawLine(start, end, Color.blue);
        ground = Physics2D.Linecast(start, end, groundLayer);
    }

    public void Speak()
    {
        audioSource.clip = cantSpray[Random.Range(0, 3)];
        audioSource.Play();
    }

    public void Run()
    {
        HorizontalSpeed = rb.velocity.x;
        VerticalSpeed = rb.velocity.y;
        Debug.Log(HorizontalSpeed);
        if (Input.GetKey(KeyCode.A))
        {
            if (HorizontalSpeed > -maxSpeed && HorizontalSpeed < maxSpeed)
            {
                rb.AddForce(Vector3.left * moveSpeed * Time.deltaTime);
            }
            spriteRenderer.flipX = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (HorizontalSpeed > -maxSpeed && HorizontalSpeed < maxSpeed)
            {
                rb.AddForce(Vector3.right * moveSpeed * Time.deltaTime);
            }
            spriteRenderer.flipX = false;
        }
        else if (Input.GetKeyUp(KeyCode.A)|| Input.GetKeyUp(KeyCode.D))
        {
            rb.velocity = new Vector2(0.0f, VerticalSpeed);
            state_ = State.STATE_IDLE;
            //animator.SetInteger("State", 0);
        }
    }

    public void Attack()
    {

    }

    public void Crouch()
    {
        if(Input.GetKey(KeyCode.S))
        {

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].point.y < rb.position.y - 0.7)
        {
            touchGround = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(""))
        {
            other.gameObject.SetActive(false);
        }
    }
}
