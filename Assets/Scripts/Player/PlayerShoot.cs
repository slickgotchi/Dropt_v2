using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            var newBullet = GameObject.Instantiate(bulletPrefab);
            newBullet.transform.position = transform.position + new Vector3(0,0.7f, 0);

            var bulletRb = newBullet.GetComponent<Rigidbody2D>();
            bulletRb.velocity = gameObject.GetComponent<PlayerMovementAndDash>().GetDirection() * 10f;

            newBullet.GetComponent<NetworkObject>().Spawn();
        }
    }
}
