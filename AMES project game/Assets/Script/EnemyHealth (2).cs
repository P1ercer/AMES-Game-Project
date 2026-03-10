using UnityEngine;
using UnityEngine.UI;
public class EnemyHealth : MonoBehaviour
{
    public float health = 3;
    public Image healthBar;
    float maxHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxHealth = health;
        healthBar.fillAmount = health / maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        healthBar.fillAmount = health / maxHealth;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "PlayerBullet")
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}
