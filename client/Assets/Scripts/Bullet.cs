﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public delegate void GameOver();
    public static event GameOver gameOverEvent;
    void Start()
    {
    }

    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerManager playerManager = collision.gameObject.GetComponent<PlayerManager>();
        if(playerManager != null)
        {
            ClientSend.PlayerHit(playerManager.id);
            // collision.gameObject.GetComponent<Player>().health -= 20;
            // if (collision.gameObject.GetComponent<Player>().health <= 0)
            // {
            //     collision.gameObject.SetActive(false);
            //     GM.instance.alivePlayers -= 1;
            //     if (GM.instance.alivePlayers <= 1) gameOverEvent?.Invoke();
            // }
        }
        StopCoroutine("DeactivateRoutine");
        gameObject.SetActive(false);
    }
    public void Deactivate(int deactivateIn)
    {
        StartCoroutine(DeactivateRoutine(deactivateIn));
    }
    private IEnumerator DeactivateRoutine(int deactivateIn)
    {
        yield return new WaitForSeconds(deactivateIn);
        this.gameObject.SetActive(false);
    }
}
