using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class Scr_CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 5f;

    [Header("Knockback Settings")]
    public float knockbackDecayRate = 5f;

    private Rigidbody rb;
    private Vector2 currentMoveInput;
    private List<Vector3> activeKnockbacks = new List<Vector3>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Takes movement input in the form of a Vector2.
    /// X maps to world X, Y maps to world Z.
    /// </summary>
    public void SetMovement(Vector2 moveInput)
    {
        currentMoveInput = moveInput;
    }

    /// <summary>
    /// Applies an instant upward force for jumping.
    /// </summary>
    public void Jump()
    {
        // Reset Y velocity first to ensure consistent jump heights regardless of current vertical momentum
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Adds a new knockback force to the list to be processed.
    /// </summary>
    public void AddKnockback(Vector3 knockbackForce)
    {
        activeKnockbacks.Add(knockbackForce);
    }

    private void FixedUpdate()
    {
        // 1. Calculate base target velocity from input
        Vector3 targetVelocity = new Vector3(currentMoveInput.x, 0f, currentMoveInput.y) * moveSpeed;

        // 2. Process and apply knockbacks on top of the movement
        Vector3 totalKnockback = Vector3.zero;

        // Loop backwards so we can safely remove elements while iterating
        for (int i = activeKnockbacks.Count - 1; i >= 0; i--)
        {
            totalKnockback += activeKnockbacks[i];

            // Decrease the magnitude to simulate falloff
            activeKnockbacks[i] = Vector3.Lerp(activeKnockbacks[i], Vector3.zero, knockbackDecayRate * Time.fixedDeltaTime);

            // Remove knockback from the list once it's small enough to ignore
            if (activeKnockbacks[i].sqrMagnitude < 0.1f)
            {
                activeKnockbacks.RemoveAt(i);
            }
        }

        // Add the combined knockback to our target velocity
        targetVelocity += totalKnockback;

        // 3. Calculate exactly how much force is needed to instantly reach the target velocity
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 velocityDifference = targetVelocity - currentHorizontalVelocity;

        // F = m * (dv / dt) -> Mass * (VelocityDifference / FixedDeltaTime)
        Vector3 requiredForce = velocityDifference * (rb.mass / Time.fixedDeltaTime);

        // 4. Apply the calculated force to the Rigidbody
        rb.AddForce(requiredForce, ForceMode.Force);
    }
}
