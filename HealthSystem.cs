using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPG.Characters
{
    public class HealthSystem : MonoBehaviour
    {

        private Animator anim;
        private AudioSource audioSource;
        //private PlayerControl playerControl;

        [SerializeField]
        private float maxHealthPoints;
        private float currentHealthPoints;

        [SerializeField]
        private Image healthBar;
        [SerializeField]
        private AudioClip[] playerHitSounds;
        [SerializeField]
        private AudioClip[] deathSounds;

        private float timeBetweenHitSounds;
        private float timeBetweenHitSoundsThreshold = 5f;

        [SerializeField]
        private float deathTimer = 3f;


        void Start()
        {
            anim = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            //playerControl = GetComponent<PlayerControl>();

            currentHealthPoints = maxHealthPoints;
        }

        // Update is called once per frame
        void Update()
        {
            if(healthBar != null)
            {
                healthBar.fillAmount = currentHealthPoints / maxHealthPoints;
            }

            if(timeBetweenHitSounds >= 0)
            {
                timeBetweenHitSounds -= Time.deltaTime;
            }

        }



        public void Damage(float damageTaken)
        {
            if(currentHealthPoints - damageTaken <= 0)
            {
                StartCoroutine(KillCharacterRoutine());
            }

            if (timeBetweenHitSounds <= 0)
            {
                AudioClip hitSoundToPlay = playerHitSounds[Random.Range(0, playerHitSounds.Length)];
                audioSource.PlayOneShot(hitSoundToPlay);
                timeBetweenHitSounds = timeBetweenHitSoundsThreshold;
            }

            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damageTaken, 0f, maxHealthPoints);
            healthBar.fillAmount = currentHealthPoints / maxHealthPoints;
        }

        public void Heal(float healingTaken)
        {
            currentHealthPoints = Mathf.Clamp(currentHealthPoints + healingTaken, 0f, maxHealthPoints);
            healthBar.fillAmount = currentHealthPoints / maxHealthPoints;

        }

        public float GetCurrentHealthPoints()
        {
            return currentHealthPoints;
        }

        public bool IsDead()
        {
            if(currentHealthPoints <= 0)
            {
                return true; 
            }
            else
            {
                return false;
            }
        }

        IEnumerator KillCharacterRoutine()
        {
            anim.SetTrigger("Death");

            AudioClip audioPicked = deathSounds[Random.Range(0, deathSounds.Length)];
            audioSource.Stop();
            audioSource.PlayOneShot(audioPicked);

            PlayerControl player = GetComponent<PlayerControl>();
            if(player != null && player.isActiveAndEnabled)
            {
                player.canMove = false;
                yield return new WaitForSecondsRealtime(audioPicked.length + 0.5f);
                SceneManager.LoadScene(0);
            }

            else
            {
                Destroy(gameObject, deathTimer);

            }
        }
        
    }
}


