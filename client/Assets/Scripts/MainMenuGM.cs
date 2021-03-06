﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuGM : MonoBehaviour
{
    private bool escPressed = false;
    public TMPro.TextMeshProUGUI winnerText;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void Awake()
    {
        winnerText.text = GameProperties.winnerString;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void setMyName(string newName)
    {
        //Client.instance.myName = newName;
        GameProperties.myName = newName;
    }
    public void setIp(string newIP)
    {
        Debug.Log("Changed IP");
        Client.FindObjectOfType<Client>().ip = newIP;
    }
    public void setPort(string newPort)
    {
        Client.FindObjectOfType<Client>().port = System.Int32.Parse(newPort);
    }
    private void OnGUI()
    {
        if (Event.current.Equals(Event.KeyboardEvent(KeyCode.Escape.ToString())))//pressed Escape
        {
            if (escPressed)
            {
                Application.Quit(0);
            }
            else
            {
                escPressed = true;
            }
        }
    }
    public void ChangeScene()
    {
        SceneManager.LoadScene("Main");
        // Debug.Log("AfterLC");
    }
}
