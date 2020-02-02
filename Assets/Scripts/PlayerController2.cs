using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController2 : MonoBehaviour
{
    public string RestartLevelName;

    [Header("Animation")]
    public Animator animator;
    private SpriteRenderer spriteRenderer;

    //需要状态机：站立，跑步，攻击，跳跃，下落，下蹲，受击。(7个状态）
    //身体状态：手（攻击，拾取），脚（跳更高，下蹲）。
    //设置名字，手，脚，头，身体
    public enum State
    {
        STATE_IDLE,
        STATE_RUN,
        STATE_JUMP,
        STATE_FALL,
        STATE_ATTACK,
        STATE_CROUCH,
        STATE_GETHIT
    };
    State state_ = State.STATE_FALL;

    public PlayerController1 player1;
    [Header("Control")]
    public float moveSpeed;
    public float maxSpeed;
    public float jumpSpeed;
    public float maxJumpSpeed;
    public float distance;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("BodyParts")]
    public GameObject head;
    public GameObject arm;
    public GameObject foot;

    [Header("Attack")]
    public int enterState = 0;
    public float attackCoolDown;
    public GameObject leftAttack;
    public GameObject rightAttack;
    private float attackTimer;


    [Header("Sound")]
    public AudioClip[] soundEffects;
    public AudioSource audioSource;

    private float HorizontalSpeed;
    private float VerticalSpeed;
    private Rigidbody2D rb;
    private BoxCollider2D foot_bc;
    private BoxCollider2D arm_bc;
    private BoxCollider2D head_bc;
    private bool ground;

    [Header("GetHit")]
    public bool getHit;
    public int bodyPart;
    public float hitTimer;
    public float hitCoolDown;
    public GameObject headInit;
    public GameObject armInit;
    public GameObject footInit;
    public bool faceRight;
    private bool headGet;
    private bool armGet;
    private bool footGet;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        foot_bc = foot.GetComponent<BoxCollider2D>();
        arm_bc = arm.GetComponent<BoxCollider2D>();
        head_bc = head.GetComponent<BoxCollider2D>();
        headGet = false;
        armGet = false;
        footGet = false;
        ground = false;
        getHit = false;
        faceRight = true;
        attackTimer = 0;
        attackCoolDown = 2f;
        hitTimer = 0;
        hitCoolDown = 2;
        bodyPart = 0;
    }
    void Update()
    {
        CheckBody();
        IsGround();
        //gethit
        switch (state_)
        {
            case State.STATE_IDLE:
                //animator.SetInteger("State", 0);
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    //animator.SetInteger("State", 1);
                    state_ = State.STATE_RUN;
                }
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    //animator.SetInteger("State", 3);
                    state_ = State.STATE_JUMP;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    if (footGet)
                    {
                        //animator.SetInteger("State", 6);
                        state_ = State.STATE_CROUCH;
                    }
                }
                else if (!ground)
                {
                    //animator.SetInteger("State", 4);
                    state_ = State.STATE_FALL;
                }
                else if (Input.GetKeyDown(KeyCode.J))
                {
                    if (armGet)
                    {
                        //animator.SetInteger("State", 2);
                        enterState = 1;
                        state_ = State.STATE_ATTACK;
                    }
                }
                else if (getHit)
                {
                    //animator.SetInteger("State", 5);
                    state_ = State.STATE_GETHIT;
                }
                break;

            case State.STATE_RUN:
                if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
                {
                    state_ = State.STATE_IDLE;
                }

                if (armGet || footGet)
                {
                    Run(2 * moveSpeed, 2 * maxSpeed);
                }
                else
                {
                    Run(moveSpeed, maxSpeed);
                }

                if (Input.GetKeyDown(KeyCode.W))
                {
                    //animator.SetInteger("State", 3);
                    state_ = State.STATE_JUMP;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    if (footGet)
                    {
                        //animator.SetInteger("State", 6);
                        state_ = State.STATE_CROUCH;
                    }
                }
                else if (!ground)
                {
                    //animator.SetInteger("State", 4);
                    state_ = State.STATE_FALL;
                }
                else if (Input.GetKeyDown(KeyCode.J))
                {
                    if (armGet)
                    {
                        enterState = 2;
                        //animator.SetInteger("State", 2);
                        state_ = State.STATE_ATTACK;
                    }
                }
                else if (getHit)
                {
                    //animator.SetInteger("State", 5);
                    state_ = State.STATE_GETHIT;
                }
                break;

            case State.STATE_ATTACK:
                Attack();
                if (getHit)
                {
                    //animator.SetInteger("State", 5);
                    state_ = State.STATE_GETHIT;
                }
                break;

            case State.STATE_JUMP:
                if (footGet)
                {
                    Jump(jumpSpeed, maxJumpSpeed);
                }
                else
                {
                    Jump(0.5f * jumpSpeed, 0.5f * maxJumpSpeed);
                }
                Run(moveSpeed * 0.5f, maxSpeed);
                if (getHit)
                {
                    //animator.SetInteger("State", 5);
                    state_ = State.STATE_GETHIT;
                }
                break;

            case State.STATE_FALL:
                Fall();
                Run(moveSpeed * 0.5f, maxSpeed);
                if (getHit)
                {
                    //animator.SetInteger("State", 5);
                    state_ = State.STATE_GETHIT;
                }
                break;

            case State.STATE_CROUCH:
                Crouch();
                enterState = 3;
                if (Input.GetKeyDown(KeyCode.J))
                {
                    enterState = 3;
                    state_ = State.STATE_ATTACK;
                }
                Run(0.5f * moveSpeed, 0.5f * maxSpeed);
                if (getHit)
                {
                    //animator.SetInteger("State", 5);
                    state_ = State.STATE_GETHIT;
                }
                break;

            case State.STATE_GETHIT:
                GetHit();
                break;
        }

        Debug.Log(state_);
    }

    void IsGround()
    {
        Vector2 start = groundCheck.position;
        Vector2 end = new Vector2(start.x, start.y - distance);

        Debug.DrawLine(start, end, Color.blue);
        ground = Physics2D.Linecast(start, end, groundLayer);
    }

    void CheckBody()
    {
        if (head.activeSelf)
        {
            headGet = true;
        }
        else
        {
            headGet = false;
        }
        if (arm.activeSelf)
        {
            armGet = true;
        }
        else
        {
            armGet = false;
        }
        if (foot.activeSelf)
        {
            footGet = true;
        }
        else
        {
            footGet = false;
        }
    }
    public void Speak()
    {
        audioSource.clip = soundEffects[Random.Range(0, 3)];
        audioSource.Play();
    }

    void Run(float moveSpeed, float maxSpeed)
    {
        HorizontalSpeed = rb.velocity.x;
        VerticalSpeed = rb.velocity.y;
        if (Input.GetKey(KeyCode.A))
        {
            if (HorizontalSpeed > -maxSpeed && HorizontalSpeed < maxSpeed)
            {
                rb.AddForce(Vector3.left * moveSpeed * Time.deltaTime);
                faceRight = false;
            }
            spriteRenderer.flipX = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (HorizontalSpeed > -maxSpeed && HorizontalSpeed < maxSpeed)
            {
                rb.AddForce(Vector3.right * moveSpeed * Time.deltaTime);
                faceRight = true;
            }
            spriteRenderer.flipX = false;
        }
    }

    public void Attack()
    {
        if (attackTimer < attackCoolDown)
        {
            attackTimer += Time.deltaTime;
            if (faceRight)
            {
                rightAttack.SetActive(true);
            }
            else
            {
                leftAttack.SetActive(true);
            }
        }
        else
        {
            rightAttack.SetActive(false);
            leftAttack.SetActive(false);
            attackTimer = 0;
            if (enterState == 1)
            {
                state_ = State.STATE_IDLE;
            }
            else if (enterState == 2)
            {
                state_ = State.STATE_RUN;
            }
            else
            {
                state_ = State.STATE_CROUCH;
            }
        }
    }

    public void Jump(float jumpSpeed, float maxJumpSpeed)
    {
        HorizontalSpeed = rb.velocity.x;
        VerticalSpeed = rb.velocity.y;
        if (VerticalSpeed >= 0 && VerticalSpeed < maxJumpSpeed)
        {
            rb.AddForce(Vector2.up * jumpSpeed * Time.deltaTime);
        }
        else
        {
            state_ = State.STATE_FALL;
        }
    }

    public void Fall()
    {
        rb.gravityScale = 2;
        if (ground)
        {
            rb.gravityScale = 1;
            state_ = State.STATE_IDLE;
        }
    }
    public void Crouch()
    {
        if (Input.GetKey(KeyCode.S))
        {
            foot_bc.enabled = false;
            rb.gravityScale = 3;
            foot.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 0);
        }
        else
        {
            foot_bc.enabled = true;
            rb.gravityScale = 1;
            foot.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);
            state_ = State.STATE_IDLE;
        }
    }

    public void GetHit()
    {
        if (bodyPart == 1)
        {
            if (armGet)
            {
                if(player1.faceRight)
                {
                    GameObject clone = Instantiate(armInit, new Vector2(transform.position.x + 1.5f, transform.position.y + 1), Quaternion.identity);
                    clone.GetComponent<Rigidbody2D>().velocity = new Vector2(4, 0);
                }
                else
                {
                    GameObject clone = Instantiate(armInit, new Vector2(transform.position.x - 1.5f, transform.position.y + 1), Quaternion.identity);
                    clone.GetComponent<Rigidbody2D>().velocity = new Vector2(-4, 0);
                }
                armGet = false;
                arm.gameObject.SetActive(false);
            }

        }
        else if (bodyPart == 2)
        {
            if (footGet)
            {
                if (player1.faceRight)
                {
                    GameObject clone = Instantiate(footInit, new Vector2(transform.position.x + 1.5f, transform.position.y + 1), Quaternion.identity);
                    clone.GetComponent<Rigidbody2D>().velocity = new Vector2(4, 0);
                }
                else
                {
                    GameObject clone = Instantiate(footInit, new Vector2(transform.position.x - 1.5f, transform.position.y + 1), Quaternion.identity);
                    clone.GetComponent<Rigidbody2D>().velocity = new Vector2(-4, 0);
                }
                footGet = false;
                foot.gameObject.SetActive(false);
            }

        }
        else if (bodyPart == 3)
        {
            if (headGet)
            {
                if (player1.faceRight)
                {
                    GameObject clone = Instantiate(headInit, new Vector2(transform.position.x + 1.5f, transform.position.y + 1), Quaternion.identity);
                    clone.GetComponent<Rigidbody2D>().velocity = new Vector2(4, 0);
                }
                else
                {
                    GameObject clone = Instantiate(headInit, new Vector2(transform.position.x - 1.5f, transform.position.y + 1), Quaternion.identity);
                    clone.GetComponent<Rigidbody2D>().velocity = new Vector2(-4, 0);
                }
                headGet = false;
                head.gameObject.SetActive(false);
            }

        }

        if (hitTimer < hitCoolDown)
        {
            hitTimer += Time.deltaTime;
        }
        else
        {
            hitTimer = 0;
            getHit = false;
            state_ = State.STATE_IDLE;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("AttackBox"))
        {
            getHit = true;
        }

        if (other.gameObject.CompareTag("Arm"))
        {

            if (!armGet)
            {
                arm.gameObject.SetActive(true);
                Destroy(other.gameObject);
            }
        }
        if (other.gameObject.CompareTag("Foot"))
        {
            if (armGet)
            {
                if (!footGet)
                {
                    foot.gameObject.SetActive(true);
                    Destroy(other.gameObject);
                }
            }
        }
        if (other.gameObject.CompareTag("Head"))
        {
            if (armGet)
            {
                if (!headGet)
                {
                    head.gameObject.SetActive(true);
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
