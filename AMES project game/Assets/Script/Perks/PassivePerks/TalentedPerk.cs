using UnityEngine;

namespace AmesGame
{
    public class TalentedPerk : Perk
    {
        [Header("Passive Perk Increase")]
        [Tooltip("Number of additional passive perks allowed.")]
        public int additionalPassivePerks = 2; // allows more than default

        private PerkController perkController;
        private int originalMaxPassive;

        public override void OnEquip()
        {
            base.OnEquip();

            perkController = GetComponent<PerkController>();
            if (perkController != null)
            {
                originalMaxPassive = perkController.MaxPassivePerks;

                // Increase the max passive perks
                typeof(PerkController)
                    .GetField("maxPassivePerks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(perkController, originalMaxPassive + additionalPassivePerks);

                Debug.Log($"Talented perk equipped. Max passive perks increased to {originalMaxPassive + additionalPassivePerks}");
            }
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (perkController != null)
            {
                // Restore original max passive perks
                typeof(PerkController)
                    .GetField("maxPassivePerks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(perkController, originalMaxPassive);

                Debug.Log($"Talented perk unequipped. Max passive perks restored to {originalMaxPassive}");
            }
        }
    }
}