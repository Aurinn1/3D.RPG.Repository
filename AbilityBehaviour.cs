using System.Collections;
using UnityEngine;

namespace RPG.Characters
{
    public abstract class AbilityBehaviour : MonoBehaviour
    {
        protected AbilityConfig config;


        public abstract void Use(GameObject target, float baseWeaponDamage);

        public void SetConfig(AbilityConfig configToSet)
        {
            config = configToSet;
        }

        protected void PlayAbilityParticleEffect()
        {
            GameObject particleObj = Instantiate(config.GetAbilityParticleEffect(), transform.position, Quaternion.identity,transform);
            ParticleSystem abilityParticleEffect = particleObj.GetComponentInChildren<ParticleSystem>();
            abilityParticleEffect.Play();
            Destroy(particleObj, abilityParticleEffect.main.duration + 2f);
        }

        protected void PlayAbilitySound()
        {
            AudioClip audioToPlay = config.GetRandomAudioClips();
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.PlayOneShot(audioToPlay);
        }

        protected void SetAbilityAnimationClip()
        {
            if(config.GetAbilityAnimationClip() != null )
            {
                AnimatorOverrideController animOverride = GetComponent<WeaponSystem>().GetAnimatorOverride();
                Animator anim = GetComponent<Animator>();
                anim.runtimeAnimatorController = animOverride;
                animOverride["DefaultAttack"] = config.GetAbilityAnimationClip();
            }

        }
        
    }
}


