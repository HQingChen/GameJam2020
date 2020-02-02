using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyParts2 : MonoBehaviour
{
    public PlayerController2 player;
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
