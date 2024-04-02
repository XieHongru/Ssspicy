using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : Moveable
{
    //public bool IsCollided(Vector2 direction)
    //{
    //    RaycastHit2D hit = Physics2D.Raycast(transform.position + 0.5f * (Vector3)direction, direction, 0.5f);

    //    if (!hit)
    //    {
    //        transform.Translate(direction);
    //        return false;
    //    }
    //    else
    //    {
    //        Debug.Log(hit.collider.name);
    //        if (hit.collider.GetComponent<Banana>() != null)
    //        {
    //            if (hit.collider.GetComponent<Banana>().IsCollided(direction))
    //            {
    //                return true;
    //            }
    //            else
    //            {
    //                transform.Translate(direction);
    //                return false;
    //            }
    //        }
    //        return true;
    //    }
    //}
}
