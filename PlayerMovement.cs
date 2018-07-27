using System;
using UnityEngine;
using UnityEngine.AI;

using RPG.CameraUI; 


namespace RPG.Characters
{
    //[RequireComponent(typeof (ThirdPersonCharacter))]
    public class PlayerMovement : MonoBehaviour
    {

        private CameraRaycaster _cameraRaycaster;

        [SerializeField]
        private Animator anim;

        [SerializeField]
        private NavMeshAgent playerAgent;



        public bool canMove = true;


        public float maxDistance;

        private GameObject enemyObject;











    }
}

