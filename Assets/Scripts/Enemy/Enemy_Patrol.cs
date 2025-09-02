using System.Collections;
using UnityEngine;

public class Enemy_Patrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    private Transform target;

    public float pauseDuration = 2.5f;
    private bool isPaused;

    private int currentPatrolIndex;
    public float speed = 1.5f;

    private Rigidbody2D rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = patrolPoints[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        Vector2 direction = (target.position - transform.position).normalized;
        if (direction.x < 0 && transform.localScale.x > 0 || direction.x > 0 && transform.localScale.x < 0)
        {
            transform.localScale =
                new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        rb.linearVelocity = direction * speed;

        if (Vector2.Distance(transform.position, target.position) < .1f)
        {
            StartCoroutine(SetPatrolPoint());
        }
    }

    IEnumerator SetPatrolPoint()
    {
        isPaused = true;

        yield return new WaitForSeconds(pauseDuration);
        
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        target = patrolPoints[currentPatrolIndex];
        isPaused = false;
    }
}
