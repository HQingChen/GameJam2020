using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2Detector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player2"))
        {
            Debug.Log("test");
        }
    }
}
