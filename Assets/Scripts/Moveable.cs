using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    public LayerMask detectLayer;
    public bool IsCollided(Vector2 direction, LayerMask groundLayer, bool isBackward)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + 0.5f * (Vector3)direction, direction, 0.5f, detectLayer);

        if(!isBackward)
        {
            if (!hit)
            {
                StepMove(direction);
                IsHung(groundLayer);
                return false;
            }
            else
            {
                if (hit.collider.GetComponent<Moveable>() != null)
                {
                    if (hit.collider.GetComponent<Moveable>().IsCollided(direction, groundLayer, isBackward))
                    {
                        //transform.Translate(direction);
                        return true;
                    }
                    else
                    {
                        StepMove(direction);
                        return false;
                    }
                }
                //DestroyImmediate(gameObject);
                //Debug.Log("True");
                return true;
            }
        }
        else
        {
            if (!hit)
            {
                SmoothMove(direction);
                return false;
            }
            else
            {
                if (hit.collider.GetComponent<Moveable>() != null)
                {
                    if (hit.collider.GetComponent<Moveable>().IsCollided(direction, groundLayer, isBackward))
                    {
                        //transform.Translate(direction);
                        return true;
                    }
                    else
                    {
                        SmoothMove(direction);
                        return false;
                    }
                }
                //DestroyImmediate(gameObject);
                //Debug.Log("True");
                return true;
            }
        }
    }

    void StepMove(Vector2 direction)
    {
        transform.Translate(direction);
    }

    void SmoothMove(Vector2 direction)
    {
        float speed = .01f;
        transform.Translate(direction * speed);
    }

    public bool IsHung(LayerMask groundLayer)
    {
        if (Physics2D.OverlapCircle(transform.position, .2f, groundLayer))
        {
            return false;
        }

        Invoke("Drop", .4f);
        return true;
    }

    void Drop()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1.5f;
        sr.sortingLayerName = "Death";
        Destroy(GetComponent<BoxCollider2D>());
    }

    public void Stop(Vector2 direction, LayerMask groundLayer)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + 0.5f * (Vector3)direction, direction, 0.5f, detectLayer);
        if(hit)
        {
            if (hit.collider.GetComponent<Moveable>() != null)
            {
                hit.collider.GetComponent<Moveable>().Stop(direction, groundLayer);
            }
        }
        transform.position = new Vector3(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.7f, transform.position.z);
        IsHung(groundLayer);
    }
}
