using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using RPG.CameraUI; 

namespace RPG.Characters
{
    public class PlayerControl : MonoBehaviour
    {

        public NavMeshAgent playerAgent;
        [SerializeField]
        private Animator anim;
        private CameraRaycaster cameraRaycaster;
        private WeaponSystem weaponSystem;


        [SerializeField]
        private SpecialAbilities specialAbilities;

        [SerializeField]
        private float abilityCooldown = 0f;

        private NPCAi enemy;

        public bool canMove = true;
        public bool isCastingOrDrawing = false;

        void Awake()
        {
            RegisterForMouseEvents();
        }

        void Start()
        {
            specialAbilities = GetComponent<SpecialAbilities>();
            weaponSystem = GetComponent<WeaponSystem>();
        }


        // Update is called once per frame
        void Update()
        {
            PlayerMovementAnimation();
            StopWhileCastingOrDrawing();
            //--------Cooldowns------------------

            if (abilityCooldown > 0)
            {
                abilityCooldown -= Time.deltaTime;
            }
            //--------------------------------------------
        }

        private void RegisterForMouseEvents()
        {
            cameraRaycaster = FindObjectOfType<CameraRaycaster>();
            cameraRaycaster.mouseOverEnemy += OnLeftMouseClickAttack;
            cameraRaycaster.mouseOverPlayer += OnleftMouseClickOnPlayer;
            cameraRaycaster.mouseOverPotentiallyWalkable += OnRightClickMovement;
        }

        private void OnleftMouseClickOnPlayer(PlayerControl player)
        {
            if(specialAbilities.isAbilitySelected == true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    specialAbilities.UseAbilityOnSelf(gameObject, weaponSystem.GetWeaponInUse().weaponDamage);
                    specialAbilities.isAbilitySelected = false;
                }
            }
        }

        //Attacks Occur on Left Mouse Click--------------------------------------
        private void OnLeftMouseClickAttack(NPCAi enemy)
        {
            GameObject enemyObj = enemy.gameObject;

            if (specialAbilities.isAbilitySelected == false)
            {
                if (Input.GetMouseButtonDown(0) && weaponSystem.IsTargetInAttackRange(enemyObj) == true)
                {
                    weaponSystem.AttackTarget(enemyObj);
                }

                else if (weaponSystem.GetWeaponInUse().isBowInUse == false && Input.GetMouseButtonDown(0) && weaponSystem.IsTargetInAttackRange(enemyObj) == false)
                {
                    StartCoroutine(MoveToTargetRoutine(enemyObj));
                }
            }

            else
            {
                if (weaponSystem.IsTargetInAttackRange(enemyObj))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        transform.LookAt(enemyObj.transform);
                        specialAbilities.UseAbilityOnEnemy(enemyObj, weaponSystem.GetWeaponInUse().weaponDamage);
                        anim.SetTrigger("Attacking");

                        //Use the ability or cast the spell...
                        specialAbilities.isAbilitySelected = false;
                    }
                }
            }
        }

        IEnumerator MoveToTargetRoutine(GameObject targetToMove)
        {
            playerAgent.SetDestination(targetToMove.transform.position);
            while (weaponSystem.IsTargetInAttackRange(targetToMove) == false)
            {
                yield return new WaitForEndOfFrame();
            }

            //yield return new WaitForEndOfFrame();
            weaponSystem.AttackTarget(targetToMove);
        }

        //TODO try to make it so that it continously follow to target wherever it may go til in attack range
        //After moving to target
       /* IEnumerator StandartAttackToTargetRoutine(GameObject targetToAttack) 
        {
            yield return StartCoroutine(MoveToTargetRoutine(targetToAttack));
            weaponSystem.AttackTarget(targetToAttack);
        } */
//---------------------------------------------------------------------------



        private void PlayerMovementAnimation()
        {
            if (playerAgent.velocity == Vector3.zero)
            {
                anim.SetBool("IsRunning", false);
            }

            else
            {
                anim.SetBool("IsRunning", true);
            }
        }

        public void OnRightClickMovement(Vector3 destination)
        {
            if (canMove == true)
            {
                if (gameObject.activeInHierarchy)
                {
                    if (Input.GetMouseButton(1))
                    {
                        playerAgent.SetDestination(destination);
                    }
                }
            }
        }

        private void StopWhileCastingOrDrawing()
        {
            if (isCastingOrDrawing == true)
            {
                playerAgent.velocity = Vector3.zero;
                playerAgent.SetDestination(transform.position);
            }
        }






        //TODO make a melee attack damage coroutine when weapon touches the enemy

        //----------------------------------------------------------



    }
}
