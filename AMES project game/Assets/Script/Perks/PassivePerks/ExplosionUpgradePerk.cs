using System.Collections;
using UnityEngine;

namespace AmesGame
{
    public class ExplosionUpgradePerk : Perk
    {


        [Header("Explosion Buffs")]
        [Tooltip("Multiplier applied to explosion damage")]
        public float damageMultiplier = 1.5f;

        [Tooltip("Multiplier applied to explosion radius")]
        public float radiusMultiplier = 1.25f;


        [Tooltip("Factor applied to projectile speed")]
        public float projectileSpeedMultiplier = 1.2f;

        [Tooltip("Factor applied to cooldown (use <1 to reduce cooldown)")]
        public float cooldownMultiplier = 0.8f;

        private EXPLOSION explosion;

        private int originalDamage;
        private float originalRadius;
        private float originalProjectileSpeed;
        private float originalCooldown;

        private bool applied = false;

        public override void OnEquip()
        {
            base.OnEquip();
            if (applied) return;

            // find EXPLOSION perk on this object or in perk slots
            explosion = GetComponentInChildren<EXPLOSION>();
            if (explosion == null)
            {
                var pc = GetComponent<PerkController>();
                if (pc != null)
                {
                    foreach (var slot in pc.perkSlots)
                    {
                        if (slot != null && slot.perk is EXPLOSION)
                        {
                            explosion = slot.perk as EXPLOSION;
                            break;
                        }
                    }
                }
            }

            if (explosion == null)
            {
                Debug.Log("ExplosionUpgradePerk: No EXPLOSION perk found to buff.");
                return;
            }

            // store originals
            originalDamage = explosion.explosionDamage;
            originalRadius = explosion.explosionRadius;
            originalProjectileSpeed = explosion.projectileSpeed;
            originalCooldown = explosion.cooldown;

            // apply buffs
            explosion.explosionDamage = Mathf.Max(1, Mathf.RoundToInt(explosion.explosionDamage * damageMultiplier));
            explosion.explosionRadius *= radiusMultiplier;
            explosion.projectileSpeed *= projectileSpeedMultiplier;
            explosion.cooldown *= cooldownMultiplier;

            applied = true;
            Debug.Log($"ExplosionUpgradePerk: Buffed EXPLOSION (dmg {originalDamage}->{explosion.explosionDamage}, radius {originalRadius}->{explosion.explosionRadius}).");
        }

        public override void OnUnequip()
        {
            base.OnUnequip();
            if (!applied || explosion == null) return;

            // restore originals
            explosion.explosionDamage = originalDamage;
            explosion.explosionRadius = originalRadius;
            explosion.projectileSpeed = originalProjectileSpeed;
            explosion.cooldown = originalCooldown;

            applied = false;
            Debug.Log("ExplosionUpgradePerk: Removed EXPLOSION buffs.");
        }
    }
}