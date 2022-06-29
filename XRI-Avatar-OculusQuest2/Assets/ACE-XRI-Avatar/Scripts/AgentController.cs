using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public float speed;

    public Transform plantAvater;
    private Transform goal;
    public Transform target1;

    public Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        goal = target1;
        //Debug.Log(goal);
        //print(goal);
        
    }

    // Update is called once per frame
    void Update()
    {

        if(plantAvater.position != goal.position)
        {
            anim.SetBool("Walking", true);
            var step = speed * Time.deltaTime;
            plantAvater.position = Vector3.MoveTowards(plantAvater.position, goal.position, step);
            plantAvater.LookAt(goal);
        }

        else
        {
            anim.SetBool("Walking", false);

            //Vector3 direction = goal.position - plantAvater.position;
            //Quaternion toRotation = Quaternion.FromToRotation(plantAvater.forward, direction);
            //transform.rotation = Quaternion.Lerp(plantAvater.rotation, toRotation, speed * Time.time);
        }
        
    }
}
