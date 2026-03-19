using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastShoot : MonoBehaviour
{
    public InputActionReference shootAction;

    public bool projectileShoot = true;
    public GameObject prefab;
    public Transform spawnPosition;
    public float shootSpeed = 60f;
    public float bulletLifetime = 10;

    [Tooltip("Minimum time between shots (seconds).")]
    public float shotCooldown = 0.2f;

    private float _cooldownMultiplier = 1f;
    private float _lastShotTime = -999f;

    [Tooltip("Damage dealt by this weapon / projectile")]
    public int damage = 1;

    private void OnEnable()
    {
        shootAction.action.Enable();
    }

    private void OnDisable()
    {
        shootAction.action.Disable();
    }

    private void Update()
    {
        if (!shootAction.action.IsPressed())
            return;

        float cooldown = shotCooldown * _cooldownMultiplier;

        if (Time.time >= _lastShotTime + cooldown)
        {
            _lastShotTime = Time.time;
            ShootOnce();
        }
    }

    private void ShootOnce()
    {
        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out hit, 10) && !projectileShoot)
        {
            var enemy = hit.collider?.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }
        else
        {
            Vector3 dest = hit.point;
            if (hit.collider == null)
                dest = Camera.main.transform.position + Camera.main.transform.forward * shootSpeed;

            GameObject bullet = Instantiate(prefab, spawnPosition.position, Quaternion.identity);

            Vector3 velocity = (dest - spawnPosition.position).normalized;

            var rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = velocity * shootSpeed;

            var pd = bullet.GetComponent<ProjectileDamage>() ?? bullet.AddComponent<ProjectileDamage>();
            pd.damage = damage;

            Destroy(bullet, bulletLifetime);
        }
    }

    public void AddCooldownMultiplier(float multiplier, float seconds)
    {
        StartCoroutine(CooldownMultiplierRoutine(multiplier, seconds));
    }

    private System.Collections.IEnumerator CooldownMultiplierRoutine(float multiplier, float seconds)
    {
        _cooldownMultiplier *= multiplier;
        yield return new WaitForSeconds(seconds);
        _cooldownMultiplier /= multiplier;
    }
}