﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // public delegate void GameOver();
    // public static event GameOver gameOverEvent;
    public IEnumerator deactivate;
    void Start()
    {
    }

    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string n = collision.name;
        if (n == "HealthBoost" || n == "PlayerSpeedBoost" || n == "BulletSpeedBoost" || n == "BulletDamageBoost") return;
        Debug.Log($"Bullet hit");
        Player player = collision.gameObject.GetComponent<Player>();
        if(player != null)
        {
            if (player.id != 0 && player.id != Client.instance.clientId) {
                Debug.Log($"EnemyHit");
                ClientSend.EnemyHit(player.id);
            }
        }
        StopCoroutine(deactivate);
        gameObject.SetActive(false);
    }
    public void Deactivate(int deactivateIn)
    {
        deactivate = DeactivateRoutine(deactivateIn);
        StartCoroutine(deactivate);
    }
    private IEnumerator DeactivateRoutine(int deactivateIn)
    {
        yield return new WaitForSeconds(deactivateIn);
        this.gameObject.SetActive(false);
    }
}
