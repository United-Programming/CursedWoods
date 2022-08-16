using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;


    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = 15;
        }
        else
        {
            speed = 8;
        }
        Movement();
    }
    private void Movement()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        transform.position += new Vector3(inputX, 0, inputZ) * Time.deltaTime * speed;
    }

}
