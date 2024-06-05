using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float deathTimer_s = 5f;
    [SerializeField] private GameObject explosionPrefab;

    private void Update()
    {
        deathTimer_s -= Time.deltaTime;

        if (deathTimer_s < 0)
        {
            Destroy(gameObject);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovementFixed>() != null) return;

        var newExplosion = Instantiate(explosionPrefab);
        newExplosion.transform.position = transform.position;

        Destroy(gameObject);
    }
}
