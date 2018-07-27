using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace RPG.Characters
{
    public class SpecialAbilities : MonoBehaviour
    {

        [SerializeField]
        private AbilityConfig[] abilities;
        [SerializeField]
        private Image energyBar;
        private float currentEnergy;
        public float maxEnergy = 100;
        [SerializeField]
        private Text energyText;
        public int energyRegenPerSec;
        private int pressedAbilityKey;
        [SerializeField]
        public bool isAbilitySelected = false;
        private float abilityCooldown;

        void Start()
        {
            currentEnergy = maxEnergy;
            AttachAbilities();
        }

        // Update is called once per frame
        void Update()
        {
            EnergyRegeneration();

            if (GetComponent<HealthSystem>().GetCurrentHealthPoints() > 0)
            {
                CanUseAbility();
            }
        }


        private void AttachAbilities()
        {
            for (int abilityIndex = 1; abilityIndex < abilities.Length; abilityIndex++)
            {
                abilities[abilityIndex].AttachAbilityTo(gameObject);
            }
        }

        public int GetAbilitiesLength()
        {
            return abilities.Length;
        }

        private void CanUseAbility()
        {
            for (int abilityIndex = 1; abilityIndex < abilities.Length; abilityIndex++)
            {
                if (currentEnergy >= abilities[abilityIndex].GetEnergyCost())
                {
                    if (Input.GetKeyDown(abilityIndex.ToString()))
                    {
                        pressedAbilityKey = abilityIndex;
                        print("Ability of " + pressedAbilityKey + " has been selected");
                        isAbilitySelected = true;
                    }
                }

                else
                {
                    print("Not Enough Energy");
                }
            }
        }

        public void UseAbilityOnEnemy(GameObject enemyTarget, float baseWeaponDamage)
        {
            if (abilities[pressedAbilityKey] != null && abilities[pressedAbilityKey].isSelfAbility == false)
            {

                abilities[pressedAbilityKey].Use(enemyTarget, baseWeaponDamage);
                UpdateEnergyBar(pressedAbilityKey);
                abilityCooldown = abilities[pressedAbilityKey].GetAbilityCooldownThreshold();
            }
        }

        public void UseAbilityOnSelf(GameObject selfTarget, float placeHolder)
        {
            if (abilities[pressedAbilityKey] != null && abilities[pressedAbilityKey].isSelfAbility == true)
            {

                abilities[pressedAbilityKey].Use(selfTarget, placeHolder);
                UpdateEnergyBar(pressedAbilityKey);
            }
        }

        private void UpdateEnergyBar(int abilitySlot)
        {
            currentEnergy = Mathf.Clamp(currentEnergy - abilities[abilitySlot].GetEnergyCost(), 0f, maxEnergy);
            energyBar.fillAmount = currentEnergy / maxEnergy;

            //TODO delete the text component when everything is set
            energyText.text = "Player Energy " + (int)currentEnergy;
        }

        private void EnergyRegeneration()
        {
            if (currentEnergy < maxEnergy)
            {
                currentEnergy += energyRegenPerSec * Time.deltaTime;
                energyBar.fillAmount = currentEnergy / maxEnergy;
                energyText.text = "Player Energy " + (int)currentEnergy;
            }
        }



    }
}


