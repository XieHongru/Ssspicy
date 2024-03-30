using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pepper : Props
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
    //        Debug.Log(hit.collider);
    //        if (hit.collider.GetComponent<Pepper>() != null)
    //        {
    //            if(hit.collider.GetComponent<Pepper>().IsCollided(direction))
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
