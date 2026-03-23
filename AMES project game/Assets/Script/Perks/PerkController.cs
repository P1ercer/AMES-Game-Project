using System.Collections.Generic;
using UnityEngine;

namespace AmesGame
{
    public enum ActivationKey
    {
        Shift,
        Ctrl,
        Q
    }

    public enum PerkMode
    {
        Active,
        Passive
    }

    public abstract class Perk : MonoBehaviour
    {
        [Header("Perk Info")]
        public string perkName = "New Perk"; //default fallback name

        // Called when the perk is triggered manually
        public virtual void Activate() { }

        // Called when the perk becomes active (chosen)
        public virtual void OnEquip()
        {
            Debug.Log($"Equipped perk: {perkName}");
        }

        // Called when the perk is removed
        public virtual void OnUnequip()
        {
            Debug.Log($"Unequipped perk: {perkName}");
        }

        // Called every frame if passive
        public virtual void Tick() { }
    }

    public class PerkController : MonoBehaviour
    {
        [System.Serializable]
        public class PerkSlot
        {
            [Tooltip("Drag a GameObject that has a Perk-derived component")]
            public Perk perk;

            [Tooltip("Is this perk passive (always active) or triggered by a key?")]
            public PerkMode mode = PerkMode.Active;

            [Tooltip("Key used to activate this perk (only used if Active)")]
            public ActivationKey activationKey = ActivationKey.Shift;

            [Tooltip("Check to enable this perk at start")]
            public bool chosen = false;
        }

        public List<PerkSlot> perkSlots = new List<PerkSlot>();

        [SerializeField]
        private int maxPerks = 3;

        public int MaxPerks => maxPerks;

        private void Start()
        {
            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null)
                {
                    Debug.LogWarning("PerkController: Empty perk slot detected in inspector.");
                    continue;
                }

                if (string.IsNullOrEmpty(slot.perk.perkName))
                {
                    slot.perk.perkName = slot.perk.name;
                    Debug.LogWarning($"Perk '{slot.perk.name}' had no perkName, using GameObject name instead.");
                }

                if (slot.chosen)
                {
                    Debug.Log($"Starting with perk: {slot.perk.perkName}");
                    slot.perk.OnEquip();
                }
            }
        }

        private void Update()
        {
            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null || !slot.chosen)
                    continue;

                if (slot.mode == PerkMode.Passive)
                {
                    slot.perk.Tick();
                }
                else
                {
                    if (IsKeyPressed(slot.activationKey))
                    {
                        Debug.Log($"Activating perk: {slot.perk.perkName}");
                        slot.perk.Activate();
                    }
                }
            }
        }

        bool IsKeyPressed(ActivationKey key)
        {
            switch (key)
            {
                case ActivationKey.Shift:
                    return Input.GetKeyDown(KeyCode.LeftShift);

                case ActivationKey.Ctrl:
                    return Input.GetKeyDown(KeyCode.LeftControl);

                case ActivationKey.Q:
                    return Input.GetKeyDown(KeyCode.Q);

                default:
                    return false;
            }
        }

        public void AddPerk(Perk perk)
        {
            if (perk == null)
                return;

            PerkSlot found = null;

            foreach (var s in perkSlots)
            {
                if (s != null && s.perk == perk)
                {
                    found = s;
                    break;
                }
            }

            if (found == null)
            {
                Debug.LogWarning($"PerkController: Tried to add a perk not in slots: {perk.name}");
                return;
            }

            if (found.chosen)
                return;

            int chosenCount = 0;
            foreach (var s in perkSlots)
                if (s != null && s.chosen) chosenCount++;

            if (chosenCount >= maxPerks)
            {
                Debug.LogWarning($"Cannot add perk '{perk.perkName}': max ({maxPerks}) reached.");
                return;
            }

            found.chosen = true;

            Debug.Log($"Added perk: {perk.perkName}");

            found.perk.OnEquip();
        }

        public void RemovePerk(Perk perk)
        {
            if (perk == null) return;

            foreach (var s in perkSlots)
            {
                if (s != null && s.perk == perk)
                {
                    if (s.chosen)
                    {
                        Debug.Log($"Removed perk: {perk.perkName}");
                        s.perk.OnUnequip();
                    }

                    s.chosen = false;
                    break;
                }
            }
        }
    }
}