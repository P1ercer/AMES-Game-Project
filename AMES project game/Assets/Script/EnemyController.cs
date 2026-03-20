using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;

public class EnemyController : MonoBehaviour
{
    // Global multiplier applied to incoming damage. Perks can modify this.
    public static float DamageMultiplier = 1f;
    public event Action OnEnemyDied;
    //Shooting
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float bulletSpeed = 15f;
    private float nextFireTime = 0f;

    //Movement
    private GameObject player;
    private NavMeshAgent agent;
    public float chaseDistance = 10f;
    private Vector3 home;

    //Health
    public float health = 3;
    public Image healthBar;
    private float maxHealth;

    void Start()
    {
        //Find player
        player = GameObject.FindGameObjectWithTag("Player");

        //NavMesh setup
        agent = GetComponent<NavMeshAgent>();
        home = transform.position;

        //Health setup
        maxHealth = health;
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }
    }

    void Update()
    {
        if (player == null) return;

        //Movement
        Vector3 direction = player.transform.position - transform.position;

        if (direction.magnitude < chaseDistance)
        {
            agent.destination = player.transform.position;

            //Face the player
            transform.LookAt(player.transform);

            //Shooting
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            agent.destination = home;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = (player.transform.position - firePoint.position).normalized;
            rb.linearVelocity = direction * bulletSpeed;
        }
    }

    //Health
    public void TakeDamage(int damage)
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
            OnEnemyDied?.Invoke(); // 🔥 Fire event BEFORE destroy
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerBullet"))
        {
            // try to get damage from a ProjectileDamage component if present
            var pd = other.gameObject.GetComponent<ProjectileDamage>();
            int dmg = pd != null ? pd.damage : 1;
            TakeDamage(dmg);
            Destroy(other.gameObject);
        }
    }
}