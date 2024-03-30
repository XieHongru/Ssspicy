using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SnakeMovement : MonoBehaviour
{
    public int bodyLength;
    public LayerMask detectLayer;
    public LayerMask backwardDetectLayer;
    public LayerMask groundLayer;

    Animator animator;
    Rigidbody2D rb;

    public GameObject body;
    public GameObject tail;
    LinkedList<GameObject> body_list;

    private Vector2 moveDirection;
    private Vector2 preDirection;

    private bool isSpicy;

    Vector2[,] dirMap = new Vector2[12, 2] { { Vector2.right, Vector2.right }, { Vector2.left, Vector2.left }, { Vector2.up, Vector2.up }, { Vector2.down, Vector2.down },
                                            { Vector2.right, Vector2.down }, { Vector2.up, Vector2.left }, { Vector2.left, Vector2.down }, { Vector2.up, Vector2.right },
                                            { Vector2.right, Vector2.up }, { Vector2.down, Vector2.left }, { Vector2.left, Vector2.up }, { Vector2.down, Vector2.right }};
    string[] turnStates = new string[6] { "Horizontal", "Vertical", "DownLeft", "DownRight", "UpLeft", "UpRight" };
    // Start is called before the first frame update
    void Start()
    {
        body_list = new LinkedList<GameObject>();
        for (int i = 1; i <= bodyLength; i++)
        {
            Vector3 bodyPos = new Vector3(transform.position.x - i * 1f, transform.position.y, transform.position.z);
            var newBody = Instantiate(body, bodyPos, Quaternion.identity);
            body_list.AddLast(newBody);
        }
        tail = Instantiate(tail, new Vector3(transform.position.x - (bodyLength + 1) * 1f, transform.position.y, transform.position.z), Quaternion.identity);
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        preDirection = new Vector2(1f, 0);
        isSpicy = false;
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

    }

    private bool IsCollided(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.5f, detectLayer);

        if (!hit)
        {
            return false;
        }
        else
        {
            if (hit.collider.GetComponent<Props>() != null)
            {
                var hitCollider = hit.collider;
                bool hitBanana = hitCollider.GetComponent<Banana>();
                bool hitPepper = hitCollider.GetComponent<Pepper>();
                bool propsHit = hitCollider.GetComponent<Props>().IsCollided(direction);
                if (propsHit && hitBanana)
                {
                    EatBanana(direction);
                }
                if (propsHit && hitPepper)
                {
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

        if (body_list.Count > 0)
        {
            var lastBody = body_list.Last.Value;
            body_list.RemoveLast();
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
        string turnState = "";
        for (int i = 0; i < 12; i++)
        {
            if (preDirection == dirMap[i, 0] && moveDirection == dirMap[i, 1])
            {
                turnState = turnStates[i / 2];
            }
            else
            {
                bodyAnimator.SetBool(turnStates[i / 2], false);
            }
        }
        bodyAnimator.SetBool(turnState, true);
    }

    void EatBanana(Vector2 direction)
    {
        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        animator.SetBool("ateBanana", true);

        var newBody = Instantiate(body, transform.position, Quaternion.identity);
        Animator bodyAnimator = newBody.GetComponent<Animator>();
        SetTurnDirection(bodyAnimator);
        body_list.AddFirst(newBody);

        preDirection = direction;
        transform.Translate(direction);
    }

    void EatPepper(Vector2 direction)
    {
        SnakeMove(direction);
        animator.SetBool("atePepper", true);
        Invoke("FeelSpicy", 1f);
    }

    void FeelSpicy()
    {
        animator.SetBool("atePepper", false);
        animator.SetBool("spicy", true);
        isSpicy = true;
    }

    void GoBackward(Vector2 direction)
    {
        float speed = -.01f;
        bool getCollision = false;

        RaycastHit2D hit;

        foreach (GameObject go in body_list)
        {
            hit = Physics2D.Raycast(go.transform.position, -1f * direction, 1f, backwardDetectLayer);
            if (hit)
            {
                getCollision = true;
                break;
            }
        }
        hit = Physics2D.Raycast(tail.transform.position, -1f * direction, 1f, backwardDetectLayer);
        if (hit)
            getCollision = true;

        if (!getCollision)
        {
            transform.Translate(direction * speed);
            foreach (GameObject go in body_list)
            {
                go.transform.Translate(direction * speed);
            }
            tail.transform.Translate(direction * speed);
        }
        else
        {
            transform.position = new Vector3(Mathf.Floor(transform.position.x) + 0.5f, transform.position.y, transform.position.z);
            foreach (GameObject go in body_list)
            {
                go.transform.Translate(direction * speed);
                go.transform.position = new Vector3(Mathf.Floor(go.transform.position.x) + 0.5f, go.transform.position.y, go.transform.position.z);
            }
            tail.transform.position = new Vector3(Mathf.Floor(tail.transform.position.x) + 0.5f, tail.transform.position.y, tail.transform.position.z);
            animator.SetBool("spicy", false);
            isSpicy = false;
        }
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
}
