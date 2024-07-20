using Unity.Netcode;
using UnityEngine;

public class PickupItem : NetworkBehaviour
{
    public float speed = 5f;
    private GameObject target;

    void Update()
    {
        if (target != null)
        {
            // Move towards the target
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);

            // Check if the item is close enough to the target
            if (Vector3.Distance(transform.position, target.transform.position) < 0.3f)
            {
                // Notify the player's magnet that the item has been collected
                PlayerPickupItemMagnet magnet = target.GetComponent<PlayerPickupItemMagnet>();
                if (magnet != null)
                {
                    magnet.Collect(this);
                }
            }
        }
    }

    public void GoTo(GameObject target)
    {
        this.target = target;
    }
}
