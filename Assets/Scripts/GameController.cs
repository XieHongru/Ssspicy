using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public int props_count;
    public bool hole_open;
    // Start is called before the first frame update
    void Start()
    {
        props_count = GameObject.FindGameObjectsWithTag("Props").Length;
        hole_open = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(props_count == 0)
        {
            hole_open = true;
            Object hole = FindObjectOfType<Hole>();
            Animator holeAnimator = hole.GetComponent<Animator>();
            holeAnimator.SetBool("open", true);
        }
    }
}
