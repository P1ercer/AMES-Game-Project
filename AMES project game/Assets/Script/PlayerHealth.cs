using UnityEngine;
using UnityEngine.UI;

namespace AmesGame
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public int MaxHealth = 100;
        public int CurrentHealth;

        [Header("UI")]
        public Image healthBar; // Image type should be Filled

        [Header("Damage Settings")]
        public int bulletDamage = 10;

        private PlayerController controller;

        private void Start()
        {
            controller = GetComponent<PlayerController>();

            CurrentHealth = MaxHealth;
            UpdateHealthUI();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("EnemyBullet"))
            {
                TakeDamage(bulletDamage);
                Destroy(collider.gameObject);
            }
        }

        public void TakeDamage(int damage)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

            UpdateHealthUI();

            if (CurrentHealth == 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth += amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {
            if (healthBar != null)
            {
                healthBar.fillAmount = (float)CurrentHealth / MaxHealth;
            }
        }

        private void Die()
        {
            Debug.Log("Player Died");

            if (controller != null)
            {
                controller.enabled = false;
            }
        }
    }
}