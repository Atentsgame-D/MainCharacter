using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class HP_Bar : MonoBehaviour
{
    public Player player;
    float hpRate = 0.0f;
    [Tooltip("1초당 회복량")]
    public float hpRecovery = 1.0f;

    TextMeshProUGUI text;
    Transform HpBarRate;

    private void Awake()
    {
        text = transform.Find("HpBar").GetComponent<TextMeshProUGUI>();
        HpBarRate = transform.Find("CurrentHP").GetComponent<Transform>();
    }
    private void Start()
    {
        player.OnHpChange += hpBarReset;
    }
    private void Update()
    {
        //if(player.Hp < player.MaxHP)
        //{
        //    player.Hp += hpRecovery * Time.deltaTime;
        //}
    }
    private void hpBarReset(float Hp)
    {
        text.text = $"{Hp:0}/{player.MaxHP}";
        hpRate = player.Hp / player.MaxHP;
        HpBarRate.localScale = new(hpRate,1,1);
    }
}
