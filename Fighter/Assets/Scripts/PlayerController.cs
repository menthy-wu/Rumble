using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 20f;
    public float moveSpeed = 5f;
    public Vector2 knockbackForce = new Vector2(5f, 15f);
    public bool facingRight = true;
    public GameObject enemy = null;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode crouchKey = KeyCode.S;
    public KeyCode attackKey = KeyCode.Space;
    public KeyCode specialKey = KeyCode.LeftShift;
    public bool isJump = false;
    public bool isCrouch = false;
    public bool isAttacking = false;
    public bool isStunned = false;
    public int health = 200;
    public int knockbackThreshold = 50;
    public int currentKnockback = 0;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            animator.Play("die_loop");
            return;
        }
        // face enemy
        if (enemy != null)
        {
            if (enemy.transform.position.x > transform.position.x)
            {
                facingRight = true;
                GetComponentInChildren<SpriteRenderer>().flipX = false;
            }
            else
            {
                facingRight = false;
                GetComponentInChildren<SpriteRenderer>().flipX = true;
            }
        }

        if (isAttacking && !isStunned && !isJump)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
        }

        if (Input.GetKey(crouchKey) && !isJump && !isStunned) // Crouch
        {
            crouch();
            isCrouch = true;
        }
        else
        {
            isCrouch = false;
            if (Input.GetKey(jumpKey) && !isJump && !isStunned && !isAttacking) // Jump
            {
                // Set velocity to jumpForce
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jumpForce);
                animator.Play("jump");
            }
            else if (Input.GetKey(rightKey) && !isJump && !isStunned && !isAttacking) // Move right
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(moveSpeed, GetComponent<Rigidbody2D>().velocity.y);
                animator.Play("walk_forward");
            }
            else if (Input.GetKey(leftKey) && !isJump && !isStunned && !isAttacking) // Move left
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(-moveSpeed, GetComponent<Rigidbody2D>().velocity.y);
                animator.Play("walk_backward");
            }
            else if (!isJump && !isStunned) // Stop moving
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    animator.Play("idle");
                }
            }
        }

        // Attacks
        if (Input.GetKeyDown(attackKey) && isCrouch && !isAttacking && !isJump && !isStunned) // Fast crouch kick
        {
            attack(transform.right, 1f, 0.5f, 4, 7);
            Debug.Log("Fast crouch kick");
            animator.Play("crouch_kick");
        }
        else if (Input.GetKeyDown(attackKey) && isJump && !isAttacking && !isStunned) // High kick
        {
            attack(transform.right, 1.3f, 0.6f, 7, 15);
            Debug.Log("High kick");
            animator.Play("high_kick");
        }
        else if (Input.GetKeyDown(attackKey) && (Input.GetKey(leftKey) || Input.GetKey(rightKey)) && !isJump && !isAttacking && !isStunned) // Side kick
        {
            delayedAttack(transform.right, 1.5f, 0.8f, 6, 12, 0.5f);
            Debug.Log("Side kick");
            animator.Play("kick");
        }
        else if (Input.GetKeyDown(attackKey) && !isAttacking && !isStunned) // Jab
        {
            attack(transform.right, 1f, 0.3f, 3, 2);
            Debug.Log("Jab");
            animator.Play("punch_light");
        }
        else if (Input.GetKeyDown(specialKey) && isCrouch && !isJump && !isAttacking && !isStunned) // Leg sweep
        {
            attack(transform.right, 2f, 0.8f, 10, 30);
            Debug.Log("Leg sweep");
            animator.Play("crouch_end_kick_included");
        }
        else if (Input.GetKeyDown(specialKey) && (Input.GetKey(leftKey) || Input.GetKey(rightKey)) && isJump && !isAttacking && !isStunned) // Flying side kick
        {
            attack(transform.right, 2f, 0.8f, 10, 15);
            Debug.Log("Flying side kick");
            animator.Play("kick");
        }
        else if (Input.GetKeyDown(specialKey) && (Input.GetKey(leftKey) || Input.GetKey(rightKey)) && !isJump && !isAttacking && !isStunned) // Strong punch
        {
            attack(transform.right, 2f, 0.8f, 8, 15);
            Debug.Log("Strong punch");
            animator.Play("punch_heavier");
        }
        else if (Input.GetKeyDown(specialKey) && !isJump && !isAttacking && !isStunned) // Roundhouse
        {
            delayedAttack(transform.right, 2f, 1f, 20, 51, 0.6f);
            delayedAttack(transform.right, 2f, 1f, 30, 51, 1.5f);
            Debug.Log("Roundhouse");
            animator.Play("kick_spin");
        }
        else if (isCrouch && (Input.GetKey(leftKey) || Input.GetKey(rightKey)) && !isJump && !isAttacking && !isStunned) // Dodge
        {
            if (Input.GetKey(rightKey)) // Move right
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(moveSpeed * 5, GetComponent<Rigidbody2D>().velocity.y);
                stun(0.1f);
                attack(Vector3.zero, 0f, 0.2f, 0, 0);
                animator.Play("crouch_kick");
            }
            else if (Input.GetKey(leftKey)) // Move left
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(-moveSpeed * 5, GetComponent<Rigidbody2D>().velocity.y);
                stun(0.1f);
                attack(Vector3.zero, 0f, 0.2f, 0, 0);
                animator.Play("crouch_kick");
            }
            Debug.Log("Dodge");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isJump = false;
            Debug.Log("Ground!");
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isJump = true;
            Debug.Log("Air!");
        }
    }
    void TakeDamage(int[] parameters)
    {
        int damage = parameters[0];
        bool facingRight = parameters[1] == 1;
        int knockbackLevel = parameters[2];
        currentKnockback += (int)(knockbackLevel * (isCrouch ? 0.2f : 1));
        damage = (int)(damage * (isCrouch ? 0.2f : 1));
        health -= damage;
        Debug.Log("I took " + damage + " damage!");
        Debug.Log("Knockback level " + currentKnockback);

        if (health <= 0)
        {
            health = 0;
            stun(100000);
            return;
        }

        animator.Play("stagger_1");

        if (currentKnockback > knockbackThreshold)
        {
            currentKnockback = 0;
            if (facingRight)
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(knockbackForce.x, knockbackForce.y);
                animator.Play("die_start");
            }
            else
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(-knockbackForce.x, knockbackForce.y);
                animator.Play("die_start");
            }
            stun(1);
        }
    }
    void delayedAttack(Vector3 offset, float radius, float cooldown, int damage, int knockbackLevel, float delay)
    {
        isAttacking = true;
        StartCoroutine(delayedAttackCoroutine(offset, radius, cooldown, damage, knockbackLevel, delay));
    }
    IEnumerator delayedAttackCoroutine(Vector3 offset, float radius, float cooldown, int damage, int knockbackLevel, float delay)
    {
        yield return new WaitForSeconds(delay);
        attack(offset, radius, cooldown, damage, knockbackLevel);
    }
    void attack(Vector3 offset, float radius, float cooldown, int damage, int knockbackLevel)
    {
        isAttacking = true;
        // Get all colliders in a 1 unit radius in front of the player
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position + offset * (facingRight ? 1 : -1), radius);
        // Loop through all colliders
        foreach (Collider2D collider in hitColliders)
        {
            // If the collider is the enemy
            if (collider.gameObject == enemy && !collider.isTrigger)
            {
                // Send message to enemy to take damage and direction
                int[] parameters = new int[] { damage, facingRight ? 1 : 0, knockbackLevel };
                collider.gameObject.SendMessage("TakeDamage", parameters);
            }
        }
        attackCooldown(cooldown);
    }
    void attackCooldown(float cooldown)
    {
        StartCoroutine(attackCooldownCoroutine(cooldown));
    }
    IEnumerator attackCooldownCoroutine(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        isAttacking = false;
    }
    void stun(float duration)
    {
        isStunned = true;
        StartCoroutine(stunCoroutine(duration));
    }
    IEnumerator stunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }
    void crouch()
    {
        if (!isCrouch)
        { animator.Play("crouch_start"); }
        else
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                animator.Play("crouch_idle");
                return;
            }
        }
    }
}
