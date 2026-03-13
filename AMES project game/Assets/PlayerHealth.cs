using UnityEngine;

namespace AmesGame
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public int MaxHealth = 100;
        public int CurrentHealth;

        private void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;

            Debug.Log("Player took damage: " + damage +
                      " | Current Health: " + CurrentHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            CurrentHealth += amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

            Debug.Log("Player healed: " + amount +
                      " | Current Health: " + CurrentHealth);
        }

        private void Die()
        {
            Debug.Log("Player Died");

            // Disable player movement
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
        }
    }
}
 