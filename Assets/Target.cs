using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    MeshRenderer mesh;
    public Player player;
    Color origin;

    float hp = 200.0f; 
  

    private void Awake()
    {
        //player = GameManager.Inst.MainPlayer;
        mesh = GetComponent<MeshRenderer>();
        origin = mesh.material.color;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            StartCoroutine(OnHit());
            takeDamage(player.Damage);
            Debug.Log($"받은 데미지{player.Damage}, 남은 체력:{hp}");
            if (player.gianHP == true) 
            {
                player.Hp += player.Damage * 0.5f;
                Debug.Log($"가한 데미지{player.Damage}, 체력 회복:{player.Damage * 0.5f} 남은 체력:{player.Hp}");
            }
        } 
    }
    public void takeDamage(float damage)
    {
        hp -= damage;
        hp = Mathf.Clamp(hp, 0.0f, 200.0f);
    }

    IEnumerator OnHit()
    {
        mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        mesh.material.color = origin;
    }
}
