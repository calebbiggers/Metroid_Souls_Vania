using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Space]
    [Header("Interactable")]
    [SerializeField] private float interact_radius = 3f;   // How close player needs to be to interact
    [SerializeField] private bool is_focused = false;
    [SerializeField] private Transform player;
    [SerializeField] private bool has_interacted = false;
    [SerializeField] private Transform interaction_transform;

    public virtual void Interact()
    {
        // This method is meant to be overwritten
        Functions.DebugLog("Interacting with " + transform.name);
    }
    
    public void Update()
    {
        if (is_focused && !has_interacted)
        {

            if(interaction_transform == null)
            {
                Functions.DebugLogError("Interaction transform of " + transform.name + " is null");
                return;
            }

            float distance = Vector3.Distance(player.position, interaction_transform.position);

            if(distance <= interact_radius)
            {
                Interact();
                has_interacted = true;
            }
        }
    }
    public void OnFocused(Transform player_transform)
    {
        is_focused = true;
        player = player_transform;
        has_interacted = false;
    }

    public void OnDefocused()
    {
        is_focused = false;
        player = null;
        has_interacted = false;
    }

    // Used to draw debug gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if(interaction_transform == null)
        {
            interaction_transform = transform;
        }
        // Draw circle that represents the interact radius
        //Gizmos.DrawWireSphere(interaction_transform.position, interact_radius);
    }
}
