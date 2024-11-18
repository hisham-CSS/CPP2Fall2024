using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public Transform player;

    Transform target;
    NavMeshAgent agent;

    public enum EnemyState
    {
        Chase, Patrol
    }

    public EnemyState state;

    public Transform[] path;
    public int pathIndex = 0;
    public float distThreshold = 0.2f; //floating point math is inexact, we have to have a threshold value to ensure we properly navigate through our patrol path.


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        if (state == EnemyState.Patrol)
        {
            if (target == player.transform) target = path[pathIndex];
            if (agent.remainingDistance < distThreshold)
            {
                pathIndex++;
                pathIndex %= path.Length;
                target = path[pathIndex];
            }
        }
        
        if (state == EnemyState.Chase)
        {
            target = player.transform;
        }

        agent.SetDestination(target.position);
    }
}
