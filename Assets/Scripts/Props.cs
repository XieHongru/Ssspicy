using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Props : MonoBehaviour
{
    public LayerMask detectLayer;
    public bool IsCollided(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + 0.5f * (Vector3)direction, direction, 0.5f, detectLayer);

        if (!hit)
        {
            transform.Translate(direction);
            return false;
        }
        else
        {
            if (hit.collider.GetComponent<Props>() != null)
            {
                if (hit.collider.GetComponent<Props>().IsCollided(direction))
                {
                    transform.Translate(direction);
                    return true;
                }
                else
                {
                    transform.Translate(direction);
                    return false;
                }
            }
            DestroyImmediate(gameObject);
            return true;
        }
    }
}
