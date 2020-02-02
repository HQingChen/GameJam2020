using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyParts1 : MonoBehaviour
{
    public PlayerController1 player;
    public int body;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("AttackBox"))
        {
            player.getHit = true;
            player.bodyPart = body;
        }
    }
}
