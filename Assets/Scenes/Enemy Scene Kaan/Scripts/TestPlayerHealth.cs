using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestPlayerHealth : MonoBehaviour
{
    public static int health = 100;

    private void Update()
    {
        if (health <= 0)
        {
            Debug.Log("Dead");
        }
    }
}
