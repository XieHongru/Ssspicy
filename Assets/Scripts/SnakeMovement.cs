using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    public int bodyLength;
    public LayerMask detectLayer;

    Animator animator;

    public GameObject body;
    public GameObject tail;
    LinkedList<GameObject> body_list;

    private Vector2 moveDirection;
    private Vector2 preDirection;

    Vector2[,] dirMap = new Vector2[12,2] { { Vector2.right, Vector2.right }, { Vector2.left, Vector2.left }, { Vector2.up, Vector2.up }, { Vector2.down, Vector2.down },
                                            { Vector2.right, Vector2.down }, { Vector2.up, Vector2.left }, { Vector2.left, Vector2.down }, { Vector2.up, Vector2.right },
                                            { Vector2.right, Vector2.up }, { Vector2.down, Vector2.left }, { Vector2.left, Vector2.up }, { Vector2.down, Vector2.right }};
    string[] turnStates = new string[6] { "Horizontal", "Vertical", "DownLeft", "DownRight", "UpLeft", "UpRight" };
    // Start is called before the first frame update
    void Start()
    {
        body_list = new LinkedList<GameObject>();
        for(int i=1; i<=bodyLength; i++)
        {
            Vector3 bodyPos = new Vector3(transform.position.x - i * 1f, transform.position.y, transform.position.z);
            var newBody = Instantiate(body, bodyPos, Quaternion.identity);
            body_list.AddLast(newBody);
        }
        tail = Instantiate(tail, new Vector3(transform.position.x - (bodyLength+1) * 1f, transform.position.y, transform.position.z), Quaternion.identity);
        animator = GetComponent<Animator>();
        preDirection = new Vector2(1f , 0);
    }

    // Update is called once per frame
    void Update()
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
            if(!IsCollided(moveDirection))
            {
                SnakeMove(moveDirection);
            }
        }

        moveDirection = Vector2.zero;
    }

    void FixedUpdate()
    {
        
    }

    private bool IsCollided(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.5f, detectLayer);

        if(!hit)
        {
            return false;
        }
        else
        {
            if(hit.collider.GetComponent<Props>() != null)
            {
                bool propsHit = hit.collider.GetComponent<Props>().IsCollided(direction);
                if(propsHit)
                {
                    eatBanana(direction);
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
        animator.SetBool("ate", false);

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
    }

    void SetTurnDirection(Animator bodyAnimator)
    {
        string turnState = "";
        for(int i=0;i<12;i++)
        {
            if (preDirection == dirMap[i,0] && moveDirection == dirMap[i,1])
            {
                turnState = turnStates[i / 2];
            }
            else
            {
                bodyAnimator.SetBool(turnStates[i/2], false);
            }
        }
        bodyAnimator.SetBool(turnState, true);
    }

    void eatBanana(Vector2 direction)
    {
        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        animator.SetBool("ate", true);

        var newBody = Instantiate(body, transform.position, Quaternion.identity);
        Animator bodyAnimator = newBody.GetComponent<Animator>();
        SetTurnDirection(bodyAnimator);
        body_list.AddFirst(newBody);

        preDirection = direction;
        transform.Translate(direction);
    }
}
