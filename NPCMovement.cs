using UnityEngine;
using UnityEngine.AI;

namespace RPG.Characters
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class NPCMovement : MonoBehaviour
	{
		[SerializeField] float movingTurnSpeed = 360;
		[SerializeField] float stationaryTurnSpeed = 180;
		[SerializeField] float moveSpeedMultiplier = 1f;
        [SerializeField] float animationSpeedMultiplier = 1f;

        [Range(0.1f, 1.0f)]
        public float animatorForwardCap = 0.5f;
        public NavMeshAgent agent;           // the navmesh agent required for the path finding
        public Transform target;                                    // target to aim for

        Rigidbody rB;
		Animator animator;
		float turnAmount;
		float forwardAmount;
		Vector3 groundNormal;

        public float currentSpeed;


		void Start()
		{
			animator = GetComponent<Animator>();
			rB = GetComponent<Rigidbody>();

            rB.constraints = RigidbodyConstraints.FreezeRotation;

            agent = GetComponent<NavMeshAgent>();

            agent.updateRotation = false;
            agent.updatePosition = true;
            currentSpeed = agent.speed;
        }

        private void Update()
        {
            if (target != null)
                agent.SetDestination(target.position);

            if (agent.remainingDistance > agent.stoppingDistance)
            {
                Move(agent.desiredVelocity, false, false);
            }

            else
            {
                agent.velocity = Vector3.zero;
                Move(Vector3.zero, false, false);
            }
        }


        public void Move(Vector3 move, bool crouch, bool jump)
		{

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();
			move = transform.InverseTransformDirection(move);
			move = Vector3.ProjectOnPlane(move, groundNormal);
			turnAmount = Mathf.Atan2(move.x, move.z);
			forwardAmount = move.z;

			ApplyExtraTurnRotation();

			// send input and other state parameters to the animator
			UpdateAnimator();
		}


		void UpdateAnimator()
		{
			// update the animator parameters
			animator.SetFloat("Forward", forwardAmount * animatorForwardCap, 0.1f, Time.deltaTime);
			animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
            animator.speed = animationSpeedMultiplier;

		}


		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
			transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
		}


		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (Time.deltaTime > 0)
			{
				Vector3 v = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = rB.velocity.y;
				rB.velocity = v;
			}
		}

        public void SetTarget(Transform target)
        {
            this.target = target;
        }








    }
}
