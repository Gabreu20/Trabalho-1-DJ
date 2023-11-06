using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{

    
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private AnimationScript anim;

    [Header("Stats")]
    public float speed = 10;
    public float leadgImpulse = 300;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;

    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallGrabed;

    public bool diying;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    private bool groundTouch;
    private bool hasDashed;

    private bool isDead = false;

    public int side = 1;

    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    [Header("Audio")]
    public AudioSource source;
    public AudioClip jump, dash, death, revive, climb, walk;
    float walkSoundCooldown = 0;

    [SerializeField] GameObject wallslideSound;

    [Header("Level")]
    public SceneInfo scene;
    public GameObject canvas;

    void Start()
    {
        leadgImpulse = 9;
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isDead)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float xRaw = Input.GetAxisRaw("Horizontal");
            float yRaw = Input.GetAxisRaw("Vertical");
            Vector2 dir = new Vector2(x, y);

            Walk(dir);
            anim.SetHorizontalMovement(x, y, rb.velocity.y);

            //som de andar
            if (walkSoundCooldown > 0)
                walkSoundCooldown -= Time.deltaTime;

            if (coll.onGround && x != 0 && walkSoundCooldown <= 0)
            {
                source.PlayOneShot(walk, 0.3f);
                walkSoundCooldown = 0.35f;
            }
            if (wallGrab && y != 0 && walkSoundCooldown <= 0)
            {
                source.PlayOneShot(walk, 0.3f);
                walkSoundCooldown = 0.35f;
            }
            //------------

            //som de slide
            if (wallSlide)
                wallslideSound.SetActive(true);
            else
                wallslideSound.SetActive(false);
            //------------

            if (coll.onWall && Input.GetKey(KeyCode.Z) && canMove)
            {
                if (side != coll.wallSide)
                    anim.Flip(side * -1);
                wallGrab = true;
                wallSlide = false;
                //source.PlayOneShot(climb, 1);
            }

            if (Input.GetKeyUp(KeyCode.Z) || !coll.onWall || !canMove)
            {
                wallGrab = false;
                wallSlide = false;
            }

            if (wallGrab != wallGrabed && Input.GetKey(KeyCode.Z))
            {
                if(y > 0)
                    rb.velocity = new Vector2(x, y + leadgImpulse);
            }
            wallGrabed = wallGrab;

            if (coll.onGround && !isDashing)
            {
                wallJumped = false;
                GetComponent<BetterJumping>().enabled = true;
            }

            if (wallGrab && !isDashing)
            {
                rb.gravityScale = 0;
                if (x > .2f || x < -.2f)
                    rb.velocity = new Vector2(rb.velocity.x, 0);

                float speedModifier = y > 0 ? .5f : 1;

                rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
            }
            else
            {
                rb.gravityScale = 3;
            }

            if (coll.onWall && !coll.onGround)
            {
                if (x != 0 && !wallGrab)
                {
                    wallSlide = true;
                    WallSlide();
                }
            }

            if (!coll.onWall || coll.onGround)
                wallSlide = false;

            if (Input.GetButtonDown("Jump"))
            {
                anim.SetTrigger("jump");

                source.PlayOneShot(jump, 1);

                if (coll.onGround)
                    Jump(Vector2.up, false);
                if (coll.onWall && !coll.onGround)
                    WallJump();
            }

            if (Input.GetKeyDown(KeyCode.X) && !hasDashed)
            {
                if (xRaw != 0 || yRaw != 0)
                    Dash(xRaw, yRaw);
                else
                {
                    Dash(side, 0);
                }
            }

            if (coll.onGround && !groundTouch)
            {
                GroundTouch();
                groundTouch = true;
            }

            if (!coll.onGround && groundTouch)
            {
                groundTouch = false;
            }

            WallParticle(y);

            if (wallGrab || wallSlide || !canMove)
                return;

            if (x > 0)
            {
                side = 1;
                anim.Flip(side);
            }
            if (x < 0)
            {
                side = -1;
                anim.Flip(side);
            }
        }
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    private void Dash(float x, float y)
    {
        source.PlayOneShot(dash, 1);

        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;

        anim.SetTrigger("dash");

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 2f + wallDir / 2f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if(coll.wallSide != side)
         anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;

        particle.Play();
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void Die()
    {
        if (diying == false)
        {
            rb.velocity = Vector3.zero;
            rb.gravityScale = 0;
            diying = true;
            source.PlayOneShot(death);
            anim.anim.SetBool("dead", true);

            anim.anim.Play("Death");

            isDead = true;
            StartCoroutine(waitToDie());
        }

    }
    IEnumerator waitToDie()
    {

        yield return new WaitForSeconds(0.75f);

        transform.position = GameObject.Find("RespawnPoint").transform.position;
        source.PlayOneShot(revive);

        anim.anim.SetBool("dead", false);
        rb.gravityScale = 3;
        diying = false;
        isDead = false;

    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("die"))
            Die();
        if (other.CompareTag("win"))
            ChangeScene(1);
        if (other.CompareTag("back"))
            ChangeScene(-1);
        if (other.CompareTag("end"))
            endGame();
    }

    void ChangeScene(int i)
    {
        scene.level = scene.level + i;
        string goTo = "Fase" + scene.level.ToString();
        SceneManager.LoadScene(goTo);
    }

    void endGame()
    {
        Time.timeScale = 0;
        canvas.SetActive(true);
    }
}
