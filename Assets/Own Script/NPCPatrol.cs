using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    public Transform[] waypoints;   // Assign in Inspector
    public float waitTime = 2f;     // Time to wait at each waypoint
    private int currentWaypoint;
    private NavMeshAgent agent;
    private float waitTimer;
    public Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        if (waypoints.Length > 0)
        {
            currentWaypoint = Random.Range(0, waypoints.Length);
            agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    void Update()
    {
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f);
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                GoToNextWaypoint();
                waitTimer = 0f;
            }
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        int nextWaypoint;
        do
        {
            nextWaypoint = Random.Range(0, waypoints.Length);
        } while (nextWaypoint == currentWaypoint);

        currentWaypoint = nextWaypoint;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }
}
