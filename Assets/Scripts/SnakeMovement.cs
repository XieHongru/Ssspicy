using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

public class SnakeMovement : MonoBehaviour
{
    public int bodyLength;
    public LayerMask detectLayer;
    public LayerMask backwardDetectLayer;
    public LayerMask groundLayer;
    public Vector2 initDirection;

    Animator animator;
    Rigidbody2D rb;

    public GameObject body;
    public GameObject tail;
    public GameObject fire;
    public GameObject star;
    public ParticleSystem dust;
    LinkedList<GameObject> body_list;

    private CinemachineImpulseSource impulseSource;

    private Vector2 moveDirection;
    private Vector2 preDirection;

    private bool isSpicy;
    private List<Moveable> props_list;
    private GameObject current_star;
    private GameObject current_fire;
    private Collider2D current_trigger;

    private bool isEntering;
    private bool isWin;
    private bool isDrop;
    private int magicCount;

    Vector2[,] dirMap = new Vector2[12, 2] { { Vector2.right, Vector2.right }, { Vector2.left, Vector2.left }, { Vector2.up, Vector2.up }, { Vector2.down, Vector2.down },
                                            { Vector2.right, Vector2.down }, { Vector2.up, Vector2.left }, { Vector2.left, Vector2.down }, { Vector2.up, Vector2.right },
                                            { Vector2.right, Vector2.up }, { Vector2.down, Vector2.left }, { Vector2.left, Vector2.up }, { Vector2.down, Vector2.right }};
    //string[] turnStates = new string[6] { "Horizontal", "Vertical", "DownLeft", "DownRight", "UpLeft", "UpRight" };
    Vector2[] turnStates = new Vector2[6] { new Vector2(1f, 0), new Vector2(0, 1f), new Vector2(-1f, 0), new Vector2(0, -1f), new Vector2(-1f, -1f), new Vector2(1f, 1f) };
    Vector2[] holeDirMap = new Vector2[4] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
    string[] holeDirection = new string[4] { "right", "left", "up", "down" };
    // Start is called before the first frame update
    void Start()
    {
        body_list = new LinkedList<GameObject>();
        for (int i = 1; i <= bodyLength; i++)
        {
            Vector3 bodyPos = new Vector3(transform.position.x - i * initDirection.x, transform.position.y - i * initDirection.y, transform.position.z);
            var newBody = Instantiate(body, bodyPos, Quaternion.identity);

            Animator bodyAnimator = newBody.GetComponent<Animator>();
            if(initDirection.x != 0)
            {
                //bodyAnimator.SetBool("Horizontal", true);
                bodyAnimator.SetFloat("bitA", 2);
                bodyAnimator.SetFloat("bitB", 0);
            }
            else
            {
                //bodyAnimator.SetBool("Vertical", true);
                bodyAnimator.SetFloat("bitA", 0);
                bodyAnimator.SetFloat("bitB", 2);
            }

            body_list.AddLast(newBody);
        }
        tail = Instantiate(tail, new Vector3(transform.position.x - (bodyLength + 1) * initDirection.x, transform.position.y - (bodyLength + 1) * initDirection.y, transform.position.z), Quaternion.identity);
        dust = Instantiate(dust, tail.transform.position, Quaternion.identity);
        Animator tailAnimator = tail.GetComponent<Animator>();
        tailAnimator.SetFloat("moveX", initDirection.x);
        tailAnimator.SetFloat("moveY", initDirection.y);

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        animator.SetFloat("moveX", initDirection.x);
        animator.SetFloat("moveY", initDirection.y);

        impulseSource = GetComponent<CinemachineImpulseSource>();
        preDirection = initDirection;
        isSpicy = false;
        props_list = new List<Moveable>();
        isEntering = false;
        isWin = false;
        isDrop = false;
        magicCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpicy)
        {
            GoBackward(preDirection);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                moveDirection = Vector2.right;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                moveDirection = Vector2.left;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                moveDirection = Vector2.up;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                moveDirection = Vector2.down;
            }
            if (moveDirection != Vector2.zero && moveDirection != preDirection * -1)
            {
                if (!IsCollided(moveDirection))
                {
                    SnakeMove(moveDirection);
                }
            }

            moveDirection = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if(isEntering)
        {
            if(magicCount==0)
            {
                if(isWin)
                {
                    GameWin();
                }
                if(isDrop)
                {
                    TrapComplete();
                }
                DeleteLastBody();
            }
            magicCount = (magicCount+1)%5;
        }
    }

    private bool IsCollided(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.2f, detectLayer);

        if (!hit)
        {
            return false;
        }
        else
        {
            if (hit.collider.GetComponent<Moveable>() != null)
            {
                var hitCollider = hit.collider;
                var hitProp = hitCollider.GetComponent<Moveable>();
                var hitBanana = hitCollider.GetComponent<Banana>();
                var hitPepper = hitCollider.GetComponent<Pepper>();
                bool propsHit = hitProp.IsCollided(direction, groundLayer, false);
                if (propsHit && hitBanana)
                {
                    DestroyImmediate(hitBanana.gameObject);
                    EatBanana(direction);
                }
                if (propsHit && hitPepper)
                {
                    DestroyImmediate(hitPepper.gameObject);
                    EatPepper(direction);
                }

                return propsHit;
            }
            return true;
        }
    }

    private void SnakeMove(Vector2 direction)
    {
        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        animator.SetBool("ateBanana", false);
        animator.SetBool("atePepper", false);

        if (body_list.Count > 0)
        {
            var lastBody = body_list.Last.Value;
            body_list.RemoveLast();
            dust.transform.position = (tail.transform.position + lastBody.transform.position) / 2;
            dust.Play();
            tail.transform.position = lastBody.transform.position;
            lastBody.transform.position = transform.position;
            Animator bodyAnimator = lastBody.GetComponent<Animator>();
            SetTurnDirection(bodyAnimator);
            body_list.AddFirst(lastBody);

            var new_lastBody = body_list.Last.Value;
            Animator tailAnimator = tail.GetComponent<Animator>();
            tailAnimator.SetFloat("moveX", new_lastBody.transform.position.x - tail.transform.position.x);
            tailAnimator.SetFloat("moveY", new_lastBody.transform.position.y - tail.transform.position.y);
        }
        else
        {
            dust.transform.position = (tail.transform.position + transform.position) / 2;
            dust.Play();
            tail.transform.position = transform.position;
            Animator tailAnimator = tail.GetComponent<Animator>();
            tailAnimator.SetFloat("moveX", direction.x);
            tailAnimator.SetFloat("moveY", direction.y);
        }

        preDirection = direction;
        transform.Translate(direction);

        if (IsHung())
        {
            animator.SetBool("drop", true);
            Invoke("Drop", .8f);
        }
    }

    void SetTurnDirection(Animator bodyAnimator)
    {
        //string turnState = "";
        for (int i = 0; i < 12; i++)
        {
            if (preDirection == dirMap[i, 0] && moveDirection == dirMap[i, 1])
            {
                //turnState = turnStates[i / 2];
                bodyAnimator.SetFloat("bitA", turnStates[i / 2].x);
                bodyAnimator.SetFloat("bitB", turnStates[i / 2].y);
                break;
            }
            //else
            //{
            //    bodyAnimator.SetBool(turnStates[i / 2], false);
            //}
        }
        //bodyAnimator.SetBool(turnState, true);
    }

    void EatBanana(Vector2 direction)
    {
        FindObjectOfType<GameController>().props_count--;

        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        animator.SetBool("ateBanana", true);

        var newBody = Instantiate(body, transform.position, Quaternion.identity);
        Animator bodyAnimator = newBody.GetComponent<Animator>();
        SetTurnDirection(bodyAnimator);
        body_list.AddFirst(newBody);

        preDirection = direction;
        transform.Translate(direction);
        current_star = Instantiate(star, new Vector3(transform.position.x + 0.6f * preDirection.x, transform.position.y + 0.6f * preDirection.y, transform.position.z), Quaternion.identity);
        Invoke("DeleteStar", .3f);
    }

    void EatPepper(Vector2 direction)
    {
        FindObjectOfType<GameController>().props_count--;

        SnakeMove(direction);
        animator.SetBool("atePepper", true);
        Invoke("FeelSpicy", .8f);
    }

    void FeelSpicy()
    {
        animator.SetBool("spicy", true);
        current_fire = Instantiate(fire, new Vector3(transform.position.x + 0.6f * preDirection.x, transform.position.y + 0.6f * preDirection.y, transform.position.z), Quaternion.identity); 
        current_fire.transform.eulerAngles = new Vector3(0, Mathf.Abs(preDirection.x) * (preDirection.x * 90f - 90f), preDirection.y * 90f);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, preDirection, 1.2f, detectLayer);
        if (hit.collider.CompareTag("Ice"))
            hit.collider.GetComponent<Ice>().DeleteSelf();
        if (hit.collider.CompareTag("Wood"))
            hit.collider.GetComponent<Wood>().DeleteSelf();
        isSpicy = true;
    }

    void GoBackward(Vector2 direction)
    {
        float speed = -.01f;
        bool getCollision = false;

        impulseSource.GenerateImpulseWithForce(.4f);

        RaycastHit2D hit;

        foreach (GameObject go in body_list)
        {
            hit = Physics2D.Raycast(go.transform.position, -1f * direction, .7f, backwardDetectLayer);
            if (hit)
            {
                var prop = hit.collider.GetComponent<Moveable>();
                if (prop != null)
                {
                    props_list.Add(prop);
                    if(prop.IsCollided(-1f *direction, groundLayer, true))
                    {
                        getCollision = true;
                    }
                }
                else
                {
                    getCollision = true;
                }
            }
        }
        hit = Physics2D.Raycast(tail.transform.position, -1f * direction, .7f, backwardDetectLayer);
        if (hit)
        {
            var prop = hit.collider.GetComponent<Moveable>();
            if (prop != null)
            {
                props_list.Add(prop);
                if (prop.IsCollided(-1f * direction, groundLayer, true))
                {

                    getCollision = true;
                }
            }
            else
            {
                getCollision = true;
            }
        }

        if (!getCollision)
        {
            transform.Translate(direction * speed);
            if(direction == Vector2.right || direction == Vector2.left)
            {
                current_fire.transform.Translate(direction.x * direction * speed);
            }
            else
            {
                current_fire.transform.Translate(direction.y * new Vector2(direction.y, direction.x) * speed);
            }
            foreach (GameObject go in body_list)
            {
                go.transform.Translate(direction * speed);
            }
            tail.transform.Translate(direction * speed);
        }
        else
        {
            Destroy(current_fire);
            current_fire = null;
            transform.position = new Vector3(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.5f, transform.position.z);
            foreach (GameObject go in body_list)
            {
                go.transform.position = new Vector3(Mathf.Floor(go.transform.position.x) + 0.5f, Mathf.Floor(go.transform.position.y) + 0.5f, go.transform.position.z);
            }
            tail.transform.position = new Vector3(Mathf.Floor(tail.transform.position.x) + 0.5f, Mathf.Floor(tail.transform.position.y) + 0.5f, tail.transform.position.z);
            animator.SetBool("spicy", false);
            isSpicy = false;

            foreach(Moveable p in props_list)
            {
                p.Stop(-1f * direction, groundLayer);
            }

            if (IsHung())
            {
                animator.SetBool("drop", true);
                Invoke("Drop", .8f);
            }
        }

        props_list.Clear();
    }

    bool IsHung()
    {
        if (Physics2D.OverlapCircle(transform.position, .2f, groundLayer))
        {
            return false;
        }
        if (Physics2D.OverlapCircle(tail.transform.position, .2f, groundLayer))
        {
            return false;
        }
        foreach (GameObject go in body_list)
        {
            if (Physics2D.OverlapCircle(go.transform.position, .2f, groundLayer))
            {
                return false;
            }
        }

        return true;
    }

    void Drop()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 1.5f;
        sr.sortingLayerName = "Death";
        Destroy(GetComponent<BoxCollider2D>());

        Rigidbody2D tailrb = tail.GetComponent<Rigidbody2D>();
        tailrb.gravityScale = 1.5f;
        sr = tail.GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Death";
        Destroy(tail.GetComponent<BoxCollider2D>());

        foreach (GameObject go in body_list)
        {
            Rigidbody2D bodyrb = go.GetComponent<Rigidbody2D>();
            bodyrb.gravityScale = 1.5f;
            sr = go.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = "Death";
            Destroy(go.GetComponent<BoxCollider2D>());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Hole") && FindObjectOfType<GameController>().hole_open == true)
        {
            current_trigger = collision;
            EnterHole();
        }
        if(collision.CompareTag("Trap") && !isSpicy)
        {
            animator.SetBool("drop", true);
            current_trigger = collision;
            Invoke("EnterTrap", .8f);
        }
    }

    void EnterHole()
    {
        GetComponent<SpriteRenderer>().sortingLayerName = "Death";
        Animator holeAnimator = FindObjectOfType<Hole>().GetComponent<Animator>();
        for (int i=0; i<4; i++)
        {
            if ( -1f * preDirection == holeDirMap[i] )
            {
                Debug.Log(holeDirMap[i]);
                holeAnimator.SetBool(holeDirection[i], true);
                break;
            }
        }

        isEntering = true;
    }

    void EnterTrap()
    {
        GetComponent<SpriteRenderer>().sortingLayerName = "Death";
        Animator trapAnimator = current_trigger.GetComponent<Animator>();
        trapAnimator.SetFloat("moveX", preDirection.x);
        trapAnimator.SetFloat("moveY", preDirection.y);
        trapAnimator.SetBool("isDrop", true);

        isEntering = true;
    }

    void DeleteLastBody()
    {
        if (body_list.Count > 1)
        {
            var lastBody = body_list.Last.Value;
            tail.transform.position = lastBody.transform.position;
            body_list.RemoveLast();
            Destroy(lastBody);
            lastBody = body_list.Last.Value;
            Animator tailAnimator = tail.GetComponent<Animator>();
            tailAnimator.SetFloat("moveX", lastBody.transform.position.x - tail.transform.position.x);
            tailAnimator.SetFloat("moveY", lastBody.transform.position.y - tail.transform.position.y);
        }
        else if (body_list.Count == 1)
        {
            var lastBody = body_list.Last.Value;
            tail.transform.position = lastBody.transform.position;
            body_list.RemoveLast();
            Destroy(lastBody);
            Animator tailAnimator = tail.GetComponent<Animator>();
            tailAnimator.SetFloat("moveX", transform.position.x - tail.transform.position.x);
            tailAnimator.SetFloat("moveY", transform.position.y - tail.transform.position.y);
        }
        else
        {
            Destroy(tail);
            if(current_trigger.CompareTag("Hole"))
            {
                Animator holeAnimator = FindObjectOfType<Hole>().GetComponent<Animator>();
                holeAnimator.SetBool("tail", true);
                isWin = true;
            }
            if(current_trigger.CompareTag("Trap"))
            {
                Animator trapAnimator = current_trigger.GetComponent<Animator>();
                trapAnimator.SetBool("tail", true);
                isDrop = true;
            }
        }
    }

    void GameWin()
    {
        Debug.Log("win");
        GameController gc = FindObjectOfType<GameController>();
        gc.props_count--;
        gc.hole_open = false;
        Animator holeAnimator = FindObjectOfType<Hole>().GetComponent<Animator>();
        holeAnimator.SetBool("open", false);
        isEntering = false;
        isWin = false;
    }

    void TrapComplete()
    {
        Animator trapAnimator = current_trigger.GetComponent<Animator>();
        trapAnimator.SetBool("isDrop", false);
        trapAnimator.SetBool("tail", false);
        isEntering = false;
        isDrop = false;
    }

    void DeleteStar()
    {
        Destroy(current_star);
        current_star = null;
    }

}
