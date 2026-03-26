using UnityEngine;

namespace AmesGame
{
    public class TheFlashPerk : Perk
    {
        [Header("Speed Boost")]
        public float speedMultiplier = 1.6f;

        [Header("Fire Rate Penalty")]
        public float fireRateMultiplier = 1.5f; // higher = slower shooting

        private PlayerController player;
        private RaycastShoot shooter;

        private float originalSpeed;

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
                // Since we can't permanently set it, fake it with a VERY long duration
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
                // Reverse the multiplier (same trick)
                shooter.AddCooldownMultiplier(1f / fireRateMultiplier, 99999f);
            }
        }
    }
}