using System.Collections.Generic;
using UnityEngine;

namespace AmesGame
{
    public class APricePerk : Perk
    {
        [Header("Penalties")]
        public float speedMultiplier = 0.7f;
        public float healthMultiplier = 0.7f;
        public float jumpMultiplier = 0.7f;
        public float gravityMultiplier = 1.4f;

        [Header("Buff")]
        public float damageMultiplier = 2.0f;

        [Header("Synergy Bonus")]
        public float synergyMultiplier = 1.3f; // 30% bonus

        private PlayerController player;
        private RaycastShoot shooter;
        private PerkController perkController;

        private float originalSpeed;
        private int originalMaxHealth;
        private float originalJumpHeight;
        private float originalGravity;
        private int originalDamage;

        private List<Perk> disabledPerks = new List<Perk>();
        private float appliedFireRateMultiplier = 1f;

        public override void OnEquip()
        {
            base.OnEquip();

            player = GetComponent<PlayerController>();
            shooter = GetComponentInChildren<RaycastShoot>();
            perkController = GetComponent<PerkController>();

            bool synergyActive = false;

            // --- Check for synergy with TheCostPerk ---
            if (perkController != null)
            {
                foreach (var slot in perkController.perkSlots)
                {
                    if (slot != null && slot.perk != null && slot.chosen && slot.perk is TheCostPerk)
                    {
                        synergyActive = true;
                        break;
                    }
                }
            }

            float finalSpeed, finalHealth, finalJump, finalGravity, finalDamage;

            if (synergyActive)
            {
                // --- Synergy: disable debuffs and apply only buffs scaled by synergy multiplier ---
                finalSpeed = 1f * synergyMultiplier; // APrice primarily buffs damage, keep neutral movement
                finalHealth = 1f * synergyMultiplier;
                finalJump = 1f * synergyMultiplier;
                finalGravity = 1f; // do not apply gravity penalty when synergy active
                finalDamage = damageMultiplier * synergyMultiplier;

                disabledPerks.Clear();
            }
            else
            {
                finalSpeed = speedMultiplier;
                finalHealth = healthMultiplier;
                finalJump = jumpMultiplier;
                finalGravity = gravityMultiplier;
                finalDamage = damageMultiplier;

                // --- Disable all other perks ---
                if (perkController != null)
                {
                    disabledPerks.Clear();
                    foreach (var slot in perkController.perkSlots)
                    {
                        if (slot == null || slot.perk == null || slot.perk == this)
                            continue;
                        if (slot.chosen)
                        {
                            disabledPerks.Add(slot.perk);
                            slot.perk.OnUnequip();
                            slot.chosen = false;
                        }
                    }
                }
            }

            // --- Apply stats ---
            if (player != null)
            {
                originalSpeed = player.MoveSpeed;
                originalMaxHealth = player.MaxHealth;
                originalJumpHeight = player.JumpHeight;
                originalGravity = player.Gravity;

                player.MoveSpeed *= finalSpeed;
                player.MaxHealth = Mathf.RoundToInt(player.MaxHealth * finalHealth);
                player.JumpHeight *= finalJump;
                player.Gravity *= finalGravity;

                player.CurrentHealth = Mathf.Min(player.CurrentHealth, player.MaxHealth);
            }

            if (shooter != null)
            {
                originalDamage = shooter.damage;
                shooter.damage = Mathf.RoundToInt(shooter.damage * finalDamage);
                appliedFireRateMultiplier = 1f; // APrice doesn't change fire rate, but keep symmetry
            }
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (player != null)
            {
                player.MoveSpeed = originalSpeed;
                player.MaxHealth = originalMaxHealth;
                player.JumpHeight = originalJumpHeight;
                player.Gravity = originalGravity;

                player.CurrentHealth = Mathf.Clamp(player.CurrentHealth, 0, player.MaxHealth);
            }

            if (shooter != null)
            {
                shooter.damage = originalDamage;
                if (appliedFireRateMultiplier != 0f)
                    shooter.AddCooldownMultiplier(1f / appliedFireRateMultiplier, 99999f);
                appliedFireRateMultiplier = 1f;
            }

            if (perkController != null && disabledPerks.Count > 0)
            {
                foreach (var perk in disabledPerks)
                {
                    perkController.AddPerk(perk);
                }
                disabledPerks.Clear();
            }
        }
    }
}