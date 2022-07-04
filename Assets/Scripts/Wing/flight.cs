using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flight : MonoBehaviour
{
    //float gravity = (float)6.67430 * Mathf.Pow(10,-11);
    public float gravity = 9.8f;
    public float mass = 1;
    public float drag = 1;
    public float speed;

    private Vector3 velocity = new Vector3(0,0,1);
    private Vector3 lift;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(transform.forward, 1, Space.World);
        else if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(transform.forward, -1, Space.World);
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 angle = Vector3.Cross(transform.forward, transform.up);
            transform.Rotate(angle, 1, Space.World);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 angle = Vector3.Cross(transform.forward, transform.up);
            transform.Rotate(angle, -1, Space.World);
        }


        lift = transform.up * Vector3.Project(transform.forward, new Vector3(0, 1, 0)).magnitude * Vector3.Project(velocity, new Vector3(0,1,0)).magnitude;
        velocity += lift * Time.deltaTime;
        velocity.y -= gravity * Time.deltaTime;
        transform.position += 0.5f * velocity * Time.deltaTime;

        velocity = Vector3.Slerp(velocity, transform.forward.normalized * velocity.magnitude, Time.deltaTime);
        speed = velocity.magnitude;
    }
}
