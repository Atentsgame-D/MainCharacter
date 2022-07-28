using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;

public class MpPotion : MonoBehaviour
{
    int potionCount = 10;
    float mpRecovery = 50.0f;
    TextMeshProUGUI text;
    public Player player;
    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void Update()
    {
        if(potionCount > 0)
        {
            if (Keyboard.current.digit6Key.wasPressedThisFrame && player.Mp < player.MaxMP)
            {
                player.Mp += mpRecovery;
                potionCount--;
                text.text = potionCount.ToString();
            }
        }
    }
}
