using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RPG.Core;

namespace RPG.Characters
{
    public abstract class AbilityConfig : ScriptableObject
    {
        //safer than public, only children who inherit from specialAbility can acces this
        protected AbilityBehaviour behaviour;


        [Header("Special Ability General")]
        [SerializeField]
        private int energyCost = 10;
        [SerializeField]
        private int abilityCooldownThreshold = 10;
        [SerializeField]
        private AnimationClip abilityAnimation;
        [SerializeField]
        private GameObject abilityParticleEffectPrefab;
        [SerializeField]
        private AudioClip[] abilitySoundClips;
        public bool isSelfAbility = false;

        public abstract AbilityBehaviour GetBehaviourComponent(GameObject objectToAttachTo);

        public void AttachAbilityTo(GameObject objectToAttachTo)
        {
            AbilityBehaviour behaviourComponent = GetBehaviourComponent(objectToAttachTo);
            behaviourComponent.SetConfig(this);
            behaviour = behaviourComponent;
        }

        public void Use(GameObject target, float baseWeaponDamage)
        {
            behaviour.Use(target, baseWeaponDamage);
        }

        public int GetEnergyCost()
        {
            return energyCost;
        }

        public AnimationClip GetAbilityAnimationClip()
        {
            return abilityAnimation;
        }

        public GameObject GetAbilityParticleEffect()
        {
            return abilityParticleEffectPrefab;
        }

        public AudioClip GetRandomAudioClips()
        {
            return abilitySoundClips[Random.Range(0, abilitySoundClips.Length)];
        }

        public int GetAbilityCooldownThreshold()
        {
            return abilityCooldownThreshold;
        }
    }


}


