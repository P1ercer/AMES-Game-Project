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
                // --- Synergy: remove all debuffs and boost stats by 30% ---
                finalSpeed = 1f * synergyMultiplier;
                finalHealth = 1f * synergyMultiplier;
                finalJump = 1f * synergyMultiplier;
                finalGravity = 1f * synergyMultiplier;
                finalDamage = 1f * synergyMultiplier;

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