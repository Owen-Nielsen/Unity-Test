using UnityEngine;
using UnityEngine.AI;

public class Pathfind_to_Player : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    private NavMeshAgent agent; // Reference to the NavMeshAgent
    public float health = 100f; // Enemy's health
    public float attackDistance = 3f; // Distance within which the enemy can attack
    public float attackDamage = 10f; // Damage dealt by the enemy
    private float attackCooldown = 0f; // Cooldown timer for attacks

    // Start is called before the first frame update
    void Start()
    {
        // Get the NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // Decrease the attack cooldown timer
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        // Set the agent's destination to the player's position
        agent.SetDestination(player.position);

        // Check if the enemy is close enough to attack the player and the attack cooldown has finished
        if (Vector3.Distance(transform.position, player.position) <= attackDistance && attackCooldown <= 0)
        {
            // Attack the player
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(attackDamage);
            }

            // Reset the attack cooldown
            attackCooldown = 1f;
        }

        // Check if the enemy's health is less than or equal to 0
        if (health <= 0)
        {
            // Enemy is dead
            Debug.Log("Enemy is dead");
        }
    }

    public void TakeDamage(float damage)
    {
        // Decrease the enemy's health by the damage amount
        health -= damage;

        // Check if the enemy's health is less than or equal to 0
        if (health <= 0)
        {
            // Enemy is dead
            Destroy(gameObject);
        }
    }
}