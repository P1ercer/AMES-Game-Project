using UnityEngine;

namespace AmesGame
{
    public class AlleyOopPerk : Perk
    {
        [Header("Alley-Oop Settings")]
        public float speedMultiplier = 1.2f;      // Increase player speed
        public float damageMultiplier = 1.5f;     // Increase bullet damage
        public float bulletGravity = -2f;         // Gentle downward acceleration
        public float bulletSpeedMultiplier = 1.2f; // Optional: increase bullet speed

        private PlayerController player;
        private RaycastShoot shooter;

        private float originalMoveSpeed;
        private int originalBulletDamage;
        private float originalBulletSpeedMultiplier;

        public override void OnEquip()
        {
            base.OnEquip();

            player = GetComponent<PlayerController>();
            shooter = GetComponent<RaycastShoot>();

            if (player != null)
            {
                originalMoveSpeed = player.MoveSpeed;
                player.MoveSpeed *= speedMultiplier;
            }

            if (shooter != null)
            {
                originalBulletDamage = shooter.damage;
                shooter.damage = Mathf.RoundToInt(shooter.damage * damageMultiplier);

                originalBulletSpeedMultiplier = shooter.bulletSpeedMultiplier;
                shooter.bulletSpeedMultiplier *= bulletSpeedMultiplier;

                shooter.OnBulletSpawned += ApplyGentleGravity;
            }
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (player != null)
            {
                player.MoveSpeed = originalMoveSpeed;
            }

            if (shooter != null)
            {
                shooter.damage = originalBulletDamage;
                shooter.bulletSpeedMultiplier = originalBulletSpeedMultiplier;
                shooter.OnBulletSpawned -= ApplyGentleGravity;
            }
        }

        private void ApplyGentleGravity(GameObject bullet)
        {
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false; // Disable Unity gravity
                // Add a BulletGravity component to apply gradual gravity over time
                var bg = bullet.GetComponent<BulletGravity>() ?? bullet.AddComponent<BulletGravity>();
                bg.gravity = bulletGravity;
            }
        }
    }

    // Component applied to bullets for gentle gravity
    public class BulletGravity : MonoBehaviour
    {
        public float gravity = -2f; // downward acceleration
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (rb != null)
            {
                rb.linearVelocity += Vector3.up * gravity * Time.fixedDeltaTime;
            }
        }
    }
}