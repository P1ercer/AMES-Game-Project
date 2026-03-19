using System.Collections.Generic;
using UnityEngine;

namespace AmesGame
{
    public enum ActivationKey
    {
        Shift,
        Ctrl,
        Q,
        E,
        R
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

        // Runtime list of active perk instances (populated from slots)
        public List<Perk> activePerks = new List<Perk>();

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

                if (!activePerks.Contains(slot.perk))
                    continue;

                if (IsKeyPressed(slot.activationKey))
                {
                    slot.perk.Activate();
                }
            }
        }

        private void Start()
        {
            // populate activePerks from inspector slots and ensure each slot's key is applied
            activePerks.Clear();

            foreach (var slot in perkSlots)
            {
                if (slot == null || slot.perk == null)
                {
                    Debug.LogWarning("PerkController: Empty perk slot detected in inspector.");
                    continue;
                }

                // Only add perks that have been chosen in the inspector
                if (!slot.chosen)
                    continue;

                if (!activePerks.Contains(slot.perk))
                {
                    if (activePerks.Count >= maxPerks)
                    {
                        Debug.LogWarning($"Cannot add chosen perk '{slot.perk.name}': maximum of {maxPerks} perks reached.");
                        continue;
                    }

                    activePerks.Add(slot.perk);
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

                case ActivationKey.E:
                    return Input.GetKeyDown(KeyCode.E);

                case ActivationKey.R:
                    return Input.GetKeyDown(KeyCode.R);

                default:
                    return false;
            }
        }

        public void AddPerk(Perk perk)
        {
            if (perk == null)
                return;

            if (activePerks.Contains(perk))
                return;

            if (activePerks.Count >= maxPerks)
            {
                Debug.LogWarning($"Cannot add perk '{perk.name}': maximum of {maxPerks} perks reached.");
                return;
            }

            activePerks.Add(perk);
        }

        public void RemovePerk(Perk perk)
        {
            if (activePerks.Contains(perk))
                activePerks.Remove(perk);
        }
    }
}