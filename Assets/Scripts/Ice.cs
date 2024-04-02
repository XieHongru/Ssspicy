using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice : Moveable
{
    public void DeleteSelf()
    {
        Destroy(gameObject);
    }
}
