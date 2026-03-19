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

    public abstract class Perk : MonoBehaviour
    {
        public abstract void Activate();
    }

    public class PerkController : MonoBehaviour
    {
        [System.Serializable]
        public class PerkSlot
        {
            [Tooltip("Drag a GameObject that has a Perk-derived component (or the component itself)")]
            public Perk perk;

            [Tooltip("Key used to activate this perk")]
            public ActivationKey activationKey = ActivationKey.Shift;
            [Tooltip("Check to enable this perk at start (choose which perks you want active)")]
            public bool chosen = false;
        }

        // Set up perks and their keybinds in the inspector using these slots
        public List<PerkSlot> perkSlots = new List<PerkSlot>();

        // Maximum number of perks that can be active at once
        [SerializeField]
        private int maxPerks = 3;

        // Expose maxPerks for UI or other systems to query
        public int MaxPerks => maxPerks;

        void Update()
        {
            // Check each configured slot for its key press and activate the associated perk
            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null)
                    continue;

                if (slot.chosen && IsKeyPressed(slot.activationKey))
                {
                    slot.perk.Activate();
                }
            }
        }

        private void Start()
        {
            // validate slots
            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null)
                {
                    Debug.LogWarning("PerkController: Empty perk slot detected in inspector.");
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
            // find the slot for this perk
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
                Debug.LogWarning($"PerkController: Tried to add a perk that is not in slots: {perk.name}");
                return;
            }

            int chosenCount = 0;
            foreach (var s in perkSlots)
                if (s != null && s.chosen) chosenCount++;

            if (found.chosen)
                return; // already chosen

            if (chosenCount >= maxPerks)
            {
                Debug.LogWarning($"Cannot add perk '{perk.name}': maximum of {maxPerks} perks reached.");
                return;
            }

            found.chosen = true;
        }

        public void RemovePerk(Perk perk)
        {
            if (perk == null) return;
            foreach (var s in perkSlots)
            {
                if (s != null && s.perk == perk)
                {
                    s.chosen = false;
                    break;
                }
            }
        }
    }
}