﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public const int fullHealth = 100;
    public int id;
    public string username;
    public GameObject aim;
    public int health = 100;
    public string playerName;

    public int killCount = 0;
    public TMPro.TextMeshPro killCountContainer;

    public GameObject HealthBar;
    Vector3 localScale;
    void Start()
    {
        killCountContainer.text = killCount.ToString();
        localScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        localScale.x = (float)(health * 1.2 / 100);
        HealthBar.transform.localScale = localScale;
    }

    public void SetHealth(int health)
    {
        this.health = health;
        Debug.Log($"Set health. Health: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (id == Client.instance.clientId)
        {
            Client.instance.Disconnect();
            GM.players.Clear();

            SceneManager.LoadScene("MainMenu");
            return;
        }
        Debug.Log($"Player with id {id} is dead. Alive: {GM.instance.alivePlayers}");
        Destroy(GM.players[id].gameObject);
        GM.players.Remove(id);
        GM.instance.alivePlayers -= 1;
        if (GM.instance.alivePlayers <= 1) GM.instance.EndGame(GameProperties.myName);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController controller = GetComponent<PlayerController>();
        if (controller == null) return;//only handle collisions by ME
        //Debug.Log($"!!!!!!!!!!!!!!!!!!!!!!!Player collided. Name: {collision.name}. GameObj Name: {collision.gameObject.name}!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        //Player player = collision.gameObject.GetComponent<Player>();
        if (collision.name == "HealthBoost")
        {
            Debug.Log("Health");
            ClientSend.BoostHandle(0, collision.gameObject.transform.position);
        } else if (collision.name == "PlayerSpeedBoost")
        {
            Debug.Log("Speed");
            ClientSend.BoostHandle(1, collision.gameObject.transform.position);
        }
        else if(collision.name == "BulletSpeedBoost")
        {
            Debug.Log("B Speed");
            controller.bulletSpeedMultiplier += 1f;
            controller.ResetBulletSpeedMultiplierDelayed(PlayerController.multiplierDuration);
            ClientSend.BoostHandle(2, collision.gameObject.transform.position);
        }
        else if(collision.name == "BulletDamageBoost")
        {
            Debug.Log("Damage");
            ClientSend.BoostHandle(3, collision.gameObject.transform.position);
        }
    }

    internal void IncrementKillCount()
    {
        killCount += 1;
        killCountContainer.text = killCount.ToString();
    }
}
