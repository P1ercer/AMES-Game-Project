using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;

public class EnemyController : MonoBehaviour
{
    // Global multiplier applied to incoming damage. Perks can modify this.
    public static float DamageMultiplier = 1f;

    // Event fired when any enemy dies (used by perks like "In a Rush")
    public static event Action<GameObject> OnEnemyKilled;

    public event Action OnEnemyDied;

    // Shooting
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float bulletSpeed = 15f;
    private float nextFireTime = 0f;

    // Audio
    [Tooltip("Sound played when this enemy fires")]
    public AudioClip shootSound;
    [Tooltip("Sound played when this enemy dies")]
    public AudioClip deathSound;
    private AudioSource audioSource;

    // Movement
    private GameObject player;
    private NavMeshAgent agent;
    public float chaseDistance = 10f;
    public float stopDistance = 2f; // distance at which enemy should stop approaching the player
    private Vector3 home;

    // Health
    public float health = 3;
    public Image healthBar;
    protected float maxHealth;

    void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player");

        // NavMesh setup
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;
        }

        // Audio setup: prefer an existing AudioSource, otherwise add one
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        home = transform.position;

        // Health setup
        maxHealth = health;
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }
    }

    protected virtual void Update()
    {
        if (player == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance < chaseDistance)
        {
            // If farther than stopDistance -> approach, otherwise stop moving but keep facing/shooting
            if (distance > stopDistance)
            {
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
            }
            else
            {
                // Stop agent movement while remaining oriented toward player
                agent.isStopped = true;

                // snap velocity to zero to avoid sliding
                if (agent.velocity.sqrMagnitude > 0f)
                {
                    agent.velocity = Vector3.zero;
                }
            }

            // Face the player (optional if agent updates rotation)
            transform.LookAt(player.transform);

            // Shooting
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(home);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = (player.transform.position - firePoint.position).normalized;
            rb.linearVelocity = direction * bulletSpeed;
        }

        // play shooting sound
        if (shootSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(shootSound, transform.position);
            }
        }
    }

    // Health
    public virtual void TakeDamage(int damage)
    {
        float adjusted = damage * DamageMultiplier;
        int applied = Mathf.Max(1, Mathf.RoundToInt(adjusted));
        health -= applied;

        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }

        if (health <= 0)
        {
            // play death sound at position so it continues even after object is destroyed
            if (deathSound != null)
            {
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }

            // Fire perk-wide kill event BEFORE destroying
            OnEnemyKilled?.Invoke(gameObject);

            // Fire local death event
            OnEnemyDied?.Invoke();

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerBullet"))
        {
            // Try to get damage from a ProjectileDamage component if present
            var pd = other.gameObject.GetComponent<ProjectileDamage>();
            int dmg = pd != null ? pd.damage : 1;
            TakeDamage(dmg);
            Destroy(other.gameObject);
        }
    }
}