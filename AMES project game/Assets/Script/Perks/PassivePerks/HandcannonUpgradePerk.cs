using System;
using UnityEngine;

namespace AmesGame
{
    public class HandcannonUpgradePerk : Perk
    {

        [Header("HandcannonUpgrade Settings")]
        [Tooltip("Multiplier applied to hand cannon projectile speed")]
        public float cannonSpeedMultiplier = 1.25f;

        [Tooltip("Multiplier applied to hand cannon damage")]
        public float cannonDamageMultiplier = 1.5f;

        [Tooltip("Multiplier applied to hand cannon cooldown (use <1 to reduce) ")]
        public float cannonCooldownMultiplier = 0.75f;

        private HandCannonperk handCannon;

        private int originalDamage;
        private float originalProjectileSpeed;
        private float originalCooldown;

        private bool applied = false;

        [Header("Drawback")]
        [Tooltip("If true, the upgrade will also deal damage to the player each time the handcannon is fired")]
        public bool reflectDamageToPlayer = true;

        [Tooltip("Amount of self-damage applied to the player when the handcannon is fired")]
        public int selfDamageAmount = 5;

        private PlayerController player;

        private void Awake()
        {
            // set perkName if base also uses it
            perkName = string.IsNullOrEmpty(perkName) ? "Hand Destroyer" : perkName;
        }

        public override void OnEquip()
        {
            base.OnEquip();

            if (applied) return;

            // try to find a HandCannonperk on this GameObject or its children
            handCannon = GetComponentInChildren<HandCannonperk>();

            // if not on player, try to locate in perk slots (attached to same PerkController)
            if (handCannon == null)
            {
                var pc = GetComponent<PerkController>();
                if (pc != null)
                {
                    foreach (var slot in pc.perkSlots)
                    {
                        if (slot != null && slot.perk is HandCannonperk)
                        {
                            handCannon = slot.perk as HandCannonperk;
                            break;
                        }
                    }
                }
            }

            if (handCannon == null)
            {
                Debug.Log("Hand Destroyer Perk: No HandCannonperk found to buff.");
                return;
            }

            // store originals
            originalDamage = handCannon.damage;
            originalProjectileSpeed = handCannon.projectileSpeed;
            originalCooldown = handCannon.cooldown;

            // apply buffs
            handCannon.damage = Mathf.Max(1, Mathf.RoundToInt(handCannon.damage * cannonDamageMultiplier));
            handCannon.projectileSpeed *= cannonSpeedMultiplier;
            handCannon.cooldown *= cannonCooldownMultiplier;

            applied = true;

            // hook up to take damage when the handcannon fires
            player = GetComponent<PlayerController>();
            if (handCannon != null && reflectDamageToPlayer)
            {
                handCannon.OnFired += OnHandCannonFired;
            }

            Debug.Log($"HandcannonUpgradePerk: Buffed HandCannon (dmg {originalDamage}->{handCannon.damage}, speed {originalProjectileSpeed}->{handCannon.projectileSpeed}, cooldown {originalCooldown}->{handCannon.cooldown})");
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (!applied || handCannon == null) return;

            // restore originals
            handCannon.damage = originalDamage;
            handCannon.projectileSpeed = originalProjectileSpeed;
            handCannon.cooldown = originalCooldown;
            if (handCannon != null && reflectDamageToPlayer)
            {
                handCannon.OnFired -= OnHandCannonFired;
            }

            applied = false;

            Debug.Log("HandcannonUpgradePerk: Removed HandCannon buff.");
        }

        private void OnHandCannonFired(UnityEngine.GameObject proj)
        {
            if (!reflectDamageToPlayer) return;

            if (player == null)
                player = GetComponent<PlayerController>();

            if (player == null) return;

            player.TakeDamage(selfDamageAmount);
            Debug.Log($"HandcannonUpgradePerk: Reflected {selfDamageAmount} damage to player due to handcannon fire.");
        }
    }
}