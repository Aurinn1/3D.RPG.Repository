using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace RPG.Characters {

    public class NPCAi : MonoBehaviour
    {
        
        private NPCMovement npcMovement;
        private PlayerControl player;
        private WeaponSystem weaponSystem;
        private GameObject playerObj;
        private HealthSystem healthSystem;
        private bool isAlive = true;  // to make the enemy npcs stop attacking player after dying
        [SerializeField]
        private bool isHostile = true; // for neutral npcs that dont attack player
        [SerializeField]
        private bool canChase = true; // for ranged enemies they dont chase for now

        [SerializeField]
        private float catchRadius = 5f;
        [SerializeField]
        private float attackRadius;
        private float distanceBetween;
        private float waitNextRangedAttack = 0f;
        public float waitNextRangedAttackThreshold = 2f;
        private Vector3 returningPosition;

        [Header("PATROL CONTROL")]
        [SerializeField]
        private bool canPatrol = true;
        [SerializeField]
        private PatrolPointHolder patrolPath;
        [Range(0, 3)]
        [SerializeField]
        private int nextPatrolPoint;
        [SerializeField]
        private float patrolWaitTime = 1f;

        enum State
        {
            idle,
            patrolling,
            chasing,
            attacking
        }
        State state = State.idle;


        void Start()
        {

            weaponSystem = GetComponent<WeaponSystem>();
            npcMovement = GetComponent<NPCMovement>();
            healthSystem = GetComponent<HealthSystem>();
            player = FindObjectOfType<PlayerControl>();
            playerObj = player.gameObject;

            if(weaponSystem != null)
            {
                attackRadius = weaponSystem.GetWeaponInUse().GetWeaponAttackRange();
            }

            returningPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {

            if (playerObj.activeInHierarchy)
            {
                
                isAlive = healthSystem.GetCurrentHealthPoints() > 0;
                if (isAlive == true)
                {
                    distanceBetween = (transform.position - playerObj.transform.position).magnitude;

                    HostileEnemyAI();


                    if (canPatrol == true && distanceBetween > attackRadius && distanceBetween > catchRadius && state != State.attacking && state != State.patrolling)
                    {
                        StopAllCoroutines();
                        npcMovement.animatorForwardCap = 0.5f;
                        npcMovement.agent.speed = 1.5f;
                        state = State.patrolling;
                        StartCoroutine(PatrolAround());
                    }

                }

                else
                {
                    StopAllCoroutines();
                    npcMovement.SetTarget(transform);
                    gameObject.layer = 12; //no collision layer
                }


            }

        }

//----------------ENEMY STATES--------------------------------------
        IEnumerator ChasePlayer()
        {
            while(distanceBetween < catchRadius)
            {
                npcMovement.SetTarget(playerObj.transform);
                yield return new WaitForEndOfFrame();
            }

        }

        IEnumerator PatrolAround()
        {
            state = State.patrolling;

            while(true)
            {
                Transform patrolPointT = patrolPath.patrolPoints[nextPatrolPoint].transform;
                npcMovement.SetTarget(patrolPointT);
                GetTheNextPoint(patrolPointT.position);
                yield return new WaitForSeconds(patrolWaitTime);

                //go to the next path
            }
        }

        private void GetTheNextPoint(Vector3 nextPoint)
        {
            if(Vector3.Distance(nextPoint, transform.position) <= npcMovement.agent.stoppingDistance + 0.5f)
            {
                nextPatrolPoint = (nextPatrolPoint + 1) % patrolPath.patrolPoints.Length;
            }
        }

        private void HostileEnemyAI()
        {
            if (isHostile == true)
            {
                if( distanceBetween <= attackRadius && state != State.attacking)
                {
                    StopAllCoroutines();
                    state = State.attacking;
                    //weaponSystem.AttackTarget(playerObj);
                    weaponSystem.AttackTargetRepeatedly(playerObj);
                }

                if (canChase == false && distanceBetween > attackRadius)
                {
                    state = State.idle;
                }

                if (canPatrol == false && canChase == true &&
                distanceBetween > attackRadius && distanceBetween > catchRadius && state != State.idle)
                {
                    StopAllCoroutines();
                    print("I should stop chasing");
                    npcMovement.agent.destination = returningPosition;
                    state = State.idle;
                }
            }



            if (canChase == true && distanceBetween > attackRadius && distanceBetween <= catchRadius && state != State.chasing)
            {
                StopAllCoroutines();
                state = State.chasing;
                StartCoroutine(ChasePlayer());
                if (isHostile == true)
                {
                    npcMovement.animatorForwardCap = 1f;
                    npcMovement.agent.speed = npcMovement.currentSpeed;

                }
            }


        }
/*
        //-----------------------------------------------------------------
        public void OnDrawGizmos()
        {
            //For Move/Catch Distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, catchRadius);


            //For Attack Distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);

        }*/

    } 

}
