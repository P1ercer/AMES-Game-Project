using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMove : MonoBehaviour
{
    GameObject player;
    NavMeshAgent agent;
    public float chaseDistance = 10;
    private Vector3 home;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        home = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = player.transform.position - transform.position;
        if (direction.magnitude < chaseDistance)
        {
            agent.destination = player.transform.position;
        }
        else
        {
            agent.destination = home;
        }
    }
}
