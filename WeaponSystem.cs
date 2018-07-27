using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public class WeaponSystem : MonoBehaviour
    {

        [SerializeField]
        private Animator anim;
        [SerializeField]
        private AnimatorOverrideController animOverride;

        [Header("WEAPON")]
        [SerializeField]
        private WeaponConfig currentWeaponInUse;
        private GameObject instantiatedWeapon;
        [SerializeField]
        private GameObject projectile;

        [SerializeField]
        private ParticleSystem criticalParticleEffect;
        public ParticleSystem particleEffectToUse;

        [Header("ATTACK CONTROL")]
        [SerializeField]
        private float castOrDrawTime = 0.5f;
        private float attackCooldown = 0f;
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float criticalChance = 0.1f;
        [SerializeField]
        private float delayedDamageTime = 0.5f;
        private GameObject targetToAttack;

        void Start()
        {
            PutWeaponInHand(currentWeaponInUse);
            WeaponDependantAnimation();
        }


        void Update()
        {
            bool isTargetDead;
            bool isTargetInRange;

            if(targetToAttack == null)
            {
                isTargetDead = false;
                isTargetInRange = false;
            }

            else
            {
                isTargetDead = targetToAttack.GetComponent<HealthSystem>().IsDead();
                isTargetInRange = IsTargetInAttackRange(targetToAttack);
                
                //test if target is out of range
            }

            float characterHealth = GetComponent<HealthSystem>().GetCurrentHealthPoints();
            bool characterIsDead = characterHealth <= 0;

            if(characterIsDead || isTargetDead || isTargetInRange == false)
            {
                StopAllCoroutines();
            }


            if (attackCooldown >= 0)
            {
                attackCooldown -= Time.deltaTime;
            }
        }

//------------------STANDART ATTACK AND DISTANCE TO TARGET CALCULATIONS----------------------


        /// <summary>
        /// Enemy AI Attack pattern
        /// </summary>
        /// <param name="targetToAttack"></param>
        public void AttackTargetRepeatedly(GameObject target)
        {
            targetToAttack = target;
            StartCoroutine(AttackTargetRepeatedlyRoutine(target));
            
        }

        IEnumerator AttackTargetRepeatedlyRoutine(GameObject targetToAttack)
        {
            bool isTargetStillAlive = targetToAttack.GetComponent<HealthSystem>().GetCurrentHealthPoints() > 0;
            bool isAttackerStillAlive = GetComponent<HealthSystem>().GetCurrentHealthPoints() > 0;

            while(isTargetStillAlive == true && isAttackerStillAlive == true)
            {
                bool isItTimeToHit = attackCooldown <= 0;
                if(isItTimeToHit == true)
                {
                    AttackTargetForEnemy(targetToAttack);
                    attackCooldown = currentWeaponInUse.GetWeaponAttackSpeed();
                }
                yield return new WaitForSeconds(attackCooldown);   
            }

        }

        public void StopAttackTargetRepeatedlyRoutine()
        {
            StopCoroutine(AttackTargetRepeatedlyRoutine(targetToAttack));
        }

        private void AttackTargetForEnemy(GameObject target)
        {
            transform.LookAt(target.transform);
            anim.SetTrigger("Attacking");

            if (currentWeaponInUse.isBowInUse == true)
            {
                //_playerMovement.isCastingOrDrawing = true;
                StartCoroutine(RangedAttackCoroutine(targetToAttack));
            }
            else
            {
                StartCoroutine(DamageAfterDelayRoutine());
                print("Hit the " + target + " for " + CalculateStandartDamage() + " damage");
            }
        }

        IEnumerator DamageAfterDelayRoutine()
        {
            yield return new WaitForSeconds(delayedDamageTime);
            targetToAttack.GetComponent<HealthSystem>().Damage(CalculateStandartDamage());


        }

        /// <summary>
        /// Player Attack Pattern
        /// </summary>
        /// <param name="target"></param>
        public void AttackTarget(GameObject target)
        {
            targetToAttack = target;
            WeaponDependantAnimation();

           /* AnimationClip attackAnimationClip = currentWeaponInUse.GetWeaponAnimation();
            float attackAnimationLength = attackAnimationClip.length / anim.speed;// + currentWeaponInUse.GetWeaponAttackSpeed(); */ 
            //TODO try to make the attack cooldown baed on animation length


            if (IsTargetInAttackRange(targetToAttack) && attackCooldown <= 0)
            {
                transform.LookAt(targetToAttack.transform);
                anim.SetTrigger("Attacking");

                if (currentWeaponInUse.isBowInUse == true)
                {
                    //_playerMovement.isCastingOrDrawing = true;
                    StartCoroutine(RangedAttackCoroutine(targetToAttack));
                }
                else
                {
                    StartCoroutine(DamageAfterDelayRoutine());
                    print("Hit the " + target + " for " + CalculateStandartDamage() + " damage");
                }


                attackCooldown = currentWeaponInUse.GetWeaponAttackSpeed();

            }
        }

        public WeaponConfig GetWeaponInUse()
        {
            return currentWeaponInUse;
        }



        IEnumerator RangedAttackCoroutine(GameObject target)
        {
            GameObject firedProjectile = Instantiate(projectile, transform.position + new Vector3(0, 1.1f, 0f), projectile.transform.rotation);
            firedProjectile.transform.rotation = transform.rotation;
            Arrow arrowComponent = firedProjectile.GetComponent<Arrow>();
            arrowComponent.arrowRB.velocity = DistanceToTarget(target) * arrowComponent.GetArrowSpeed();
            yield return new WaitForSeconds(castOrDrawTime);
            //_playerMovement.isCastingOrDrawing = false;
        }

        public float CalculateStandartDamage()
        {
            bool isItCritical = criticalChance > UnityEngine.Random.Range(0f, 1.0f);

            if (isItCritical == true)
            {
                float totalDamage = currentWeaponInUse.GetWeaponCriticalDamageModifier() * currentWeaponInUse.weaponDamage;

                if(criticalParticleEffect != null)
                {
                    criticalParticleEffect.Play();
                }

                return totalDamage;
            }

            else
            {
                return currentWeaponInUse.weaponDamage;
            }

        }

        public bool IsTargetInAttackRange(GameObject target)
        {
            float distBetween = (transform.position - target.transform.position).magnitude;
            return distBetween < currentWeaponInUse.GetWeaponAttackRange();
        }

        private Vector3 DistanceToTarget(GameObject target)
        {
            Vector3 towardsTarget = (target.transform.position - transform.position).normalized;
            return towardsTarget;
        }


        //-------------------------WEAPON ON HAND------------------------------------
        public void PutWeaponInHand(WeaponConfig weapon)
        {
            GameObject weaponPrefab = weapon.GetWeaponPrefab();
            GameObject powerAttackParticlePrefab = weapon.GetWeaponPowerAttackGlowPrefab();

            currentWeaponInUse = weapon;
            Destroy(instantiatedWeapon);
            Transform weaponHandT = RequestDominantHand();
            instantiatedWeapon = Instantiate(weaponPrefab, weaponHandT);
            instantiatedWeapon.transform.localPosition = currentWeaponInUse.weaponGrip.localPosition;
            instantiatedWeapon.transform.localRotation = currentWeaponInUse.weaponGrip.localRotation;

            if(powerAttackParticlePrefab != null)
            {
                GameObject powerAttackParticleObj = Instantiate(powerAttackParticlePrefab, instantiatedWeapon.transform);
                powerAttackParticleObj.transform.localRotation = currentWeaponInUse.particleGrip.localRotation;
                powerAttackParticleObj.transform.localPosition = currentWeaponInUse.particleGrip.localPosition;
                particleEffectToUse = powerAttackParticleObj.GetComponent<ParticleSystem>();
            }


            //print("Put the " + weapon + " in " + gameObject + "'s hand");

            WeaponDependantAnimation();
        }

        private Transform RequestDominantHand()
        {
            DominantHand[] dominantHand = GetComponentsInChildren<DominantHand>();

            if (currentWeaponInUse.isBowInUse == true) //left hand. check player in hierarchy
            {
                return dominantHand[0].transform;
            }
            else //right hand. check player in hierarchy
            {
                return dominantHand[1].transform;
            }
        }

        //--------------------------------------------------------------------------

        private void WeaponDependantAnimation()
        {
            anim = GetComponent<Animator>();
            anim.runtimeAnimatorController = animOverride;
            animOverride["DefaultAttack"] = currentWeaponInUse.GetWeaponAnimation();
        }

      /*  public void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, currentWeaponInUse.GetWeaponAttackRange());
        } */

        public AnimatorOverrideController GetAnimatorOverride()
        {
            return animOverride;
        }


    }



}


