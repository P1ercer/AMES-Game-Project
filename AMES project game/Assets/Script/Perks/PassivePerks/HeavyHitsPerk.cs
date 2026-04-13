using UnityEngine;

namespace AmesGame
{
    public class HeavyHitsPerk : Perk
    {
        [Header("Damage Boost")]
        public float damageMultiplier = 1.8f;

        [Header("Speed Penalty")]
        public float speedMultiplier = 0.7f;

        [Header("Fire Rate Penalty")]
        public float fireRateMultiplier = 1.5f; // higher = slower shooting

        private PlayerController player;
        private RaycastShoot shooter;

        private float originalSpeed;
        private int originalDamage;

        public override void OnEquip()
        {
            base.OnEquip();

            player = GetComponent<PlayerController>();
            shooter = GetComponentInChildren<RaycastShoot>();

            if (player != null)
            {
                originalSpeed = player.MoveSpeed;
                player.MoveSpeed *= speedMultiplier;
            }

            if (shooter != null)
            {
                // Store original damage
                originalDamage = shooter.damage;

                // Increase damage
                shooter.damage = Mathf.RoundToInt(shooter.damage * damageMultiplier);

                // Decrease fire rate (long-duration trick)
                shooter.AddCooldownMultiplier(fireRateMultiplier, 99999f);
            }
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (player != null)
            {
                player.MoveSpeed = originalSpeed;
            }

            if (shooter != null)
            {
                // Restore damage
                shooter.damage = originalDamage;

                // Revert fire rate penalty
                shooter.AddCooldownMultiplier(1f / fireRateMultiplier, 99999f);
            }
        }
    }
}