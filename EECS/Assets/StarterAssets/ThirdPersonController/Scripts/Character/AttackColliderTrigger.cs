using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Attach this script to bones colliders to handle attack collisions
    /// </summary>
    public class AttackColliderTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Collider curr = GetComponent<Collider>();

            if (curr.enabled) {

                // 1. Get the root controller of whoever this hitbox belongs to
                C_Controller myController = GetComponentInParent<C_Controller>();

                // 2. Get the controller of the object we just hit
                C_Controller hitController = other.GetComponentInParent<C_Controller>();

                if (myController != null && hitController != null)
                {
                    // 3. CRITICAL: If the player numbers match, stop! It's a self-hit.
                    if (myController.GetPlayerNumber() == hitController.GetPlayerNumber())
                    {
                        return; // Ignore the collision entirely
                    }
                }
                // Check if the thing that touched us belongs to an enemy weapon layer
                // (Or check the name/tag of the collider that hit us)
                if (other.gameObject.layer == LayerMask.NameToLayer("Player_Hitbox"))
                {
                    Debug.Log($"{gameObject.transform.parent.name} was HIT by {other.name}!");

                    other.GetComponentInParent<C_Controller>().TakeDamage(256, false);

                    // Clean, direct component disabling
                    curr.enabled = false;
                }
            }
        }
    }
}
