using System.Collections.Generic;
using UnityEngine;

namespace AmesGame
{
    public class TheCostPerk : Perk
    {
        [Header("Buffs")]
        public float speedMultiplier = 1.5f;
        public float healthMultiplier = 1.3f;
        public float jumpMultiplier = 1.4f;

        [Header("Penalties")]
        public float gravityMultiplier = 1.3f;
        public float damageMultiplier = 0.7f;
        public float fireRateMultiplier = 1.5f;

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

            // --- Check for synergy with APricePerk ---
            if (perkController != null)
            {
                foreach (var slot in perkController.perkSlots)
                {
                    if (slot != null && slot.perk != null && slot.chosen && slot.perk is APricePerk)
                    {
                        synergyActive = true;
                        break;
                    }
                }
            }

            float finalSpeed, finalHealth, finalJump, finalGravity, finalDamage, finalFireRate;

            if (synergyActive)
            {
                // --- Synergy: disable debuffs and apply only this perk's buffs scaled by synergy multiplier ---
                finalSpeed = speedMultiplier * synergyMultiplier;
                finalHealth = healthMultiplier * synergyMultiplier;
                finalJump = jumpMultiplier * synergyMultiplier;
                // do not apply gravity or damage penalties when synergy active
                finalGravity = 1f;
                finalDamage = 1f;
                // do not apply fire rate penalty when synergy active
                finalFireRate = 1f;

                // Unlock all other perks
                disabledPerks.Clear();
            }
            else
            {
                finalSpeed = speedMultiplier;
                finalHealth = healthMultiplier;
                finalJump = jumpMultiplier;
                finalGravity = gravityMultiplier;
                finalDamage = damageMultiplier;
                finalFireRate = fireRateMultiplier;

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

                // apply cooldown multiplier and remember what we applied so we can undo it accurately
                appliedFireRateMultiplier = finalFireRate;
                shooter.AddCooldownMultiplier(appliedFireRateMultiplier, 99999f);
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
                // undo the exact multiplier we applied
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