using UnityEngine;

public class Projectile : MonoBehaviour
{

    public bool canReflect = false;
    [SerializeField] private Rigidbody projectileRB;

    Vector3 direction = Vector3.zero;

    float speed = 1f;

    public void InitalizeProjectile(Vector3 startingDirection, float speed)
    {
        projectileRB.velocity = startingDirection * speed;
        direction = projectileRB.velocity.normalized;
        this.speed = speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (canReflect)
        {
            Vector3 normal = collision.contacts[0].normal;
            direction = VectorReflectionCalculation(direction, normal);
            projectileRB.velocity = direction * speed;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Vector3 VectorReflectionCalculation(Vector3 originalDirectionToReflect, Vector3 collisionNormal)
    {
        originalDirectionToReflect.Normalize();
        collisionNormal.Normalize();
        Vector3 reflectionVector = originalDirectionToReflect -
                        (2 * Vector3.Dot(originalDirectionToReflect, collisionNormal) * collisionNormal);

        return reflectionVector.normalized;
    }


}
