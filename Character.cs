﻿using System;
using UnityEngine;
using UnityEngine.AI;
using RPG.CameraUI;

namespace RPG.Characters
{
    [SelectionBase]
    public class Character : MonoBehaviour
    {
        [Header("Animator")] [SerializeField] RuntimeAnimatorController animatorController;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        [SerializeField] Avatar characterAvatar;

        [Header("Audio")]
        [SerializeField] float audioSourceSpatialBlend = 0.5f;

        [Header("Capsule Collider")]
        [SerializeField] Vector3 colliderCenter = new Vector3(0, 1.03f, 0);
        [SerializeField] float colliderRadius = 0.2f;
        [SerializeField] float colliderHeight = 2.03f;

        [Header("Movement")]
        [SerializeField] float moveSpeedMultiplier = .7f;
        [SerializeField] float animationSpeedMultiplier = 1.5f;
        [SerializeField] float movingTurnSpeed = 360;
        [SerializeField] float stationaryTurnSpeed = 180;
        [SerializeField] float moveThreshold = 1f;

        [Header("Nav Mesh Agent")]
        [SerializeField] float navMeshAgentSteeringSpeed = 1.0f;
        [SerializeField] float navMeshAgentStoppingDistance = 1.3f;

        Vector3 clickPoint;
        NavMeshAgent navMeshAgent;
        CapsuleCollider capsuleCollider;
        Animator animator;
        Rigidbody ridigBody;
        AudioSource audioSource;
        float turnAmount;
        float forwardAmount;

        void Awake()
        {
            GetRequiredComponents();
        }

        private void GetRequiredComponents()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            capsuleCollider.center = colliderCenter;
            capsuleCollider.radius = colliderRadius;
            capsuleCollider.height = colliderHeight;

            ridigBody = gameObject.GetComponent<Rigidbody>();
            ridigBody.constraints = RigidbodyConstraints.FreezeRotation;

            audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.spatialBlend = audioSourceSpatialBlend;

            animator = gameObject.GetComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            animator.avatar = characterAvatar;

            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
            navMeshAgent.speed = navMeshAgentSteeringSpeed;
            navMeshAgent.stoppingDistance = navMeshAgentStoppingDistance;
            navMeshAgent.autoBraking = false;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updatePosition = true;
        }

        void Start()
        {
            CameraRaycaster cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();

        }

        void Update()
        {
            if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                Move(navMeshAgent.desiredVelocity);
            }
            else
            {
                Move(Vector3.zero);
            }
        }

        public void Move(Vector3 movement)
        {
            SetForwardAndTurn(movement);
            ApplyExtraTurnRotation();
            UpdateAnimator();
        }

        public void Kill()
        {
            // to allow death signaling
        }

        void SetForwardAndTurn(Vector3 movement)
        {
            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired direction
            if (movement.magnitude > moveThreshold)
            {
                movement.Normalize();
            }
            var localMove = transform.InverseTransformDirection(movement);
            turnAmount = Mathf.Atan2(localMove.x, localMove.z);
            forwardAmount = localMove.z;
        }

        void UpdateAnimator()
        {
            animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
            animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
            animator.speed = animationSpeedMultiplier;
        }

        void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
        }


        void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (Time.deltaTime > 0)
            {
                Vector3 velocity = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

                // we preserve the existing y part of the current velocity.
                velocity.y = ridigBody.velocity.y;
                ridigBody.velocity = velocity;
            }
        }


    }
}