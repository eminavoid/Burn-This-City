using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public string enemyName = "Default Enemy";
        public float maxHealth = 100f;
        protected float currentHealth;
    
        public float moveSpeed = 2f;
    
        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }
    
        protected virtual void Update()
        {
            // comportamiento general
        }
    
        public virtual void TakeDamage(float amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
                Die();
        }
    
        protected virtual void Die()
        {
            Debug.Log(enemyName + " murió.");
            Destroy(gameObject);
        }
}
