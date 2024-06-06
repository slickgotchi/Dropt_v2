using Mono.CSharp.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DashTrail : MonoBehaviour
{
    [SerializeField] private Sprite _front;
    [SerializeField] private Sprite _back;
    [SerializeField] private Sprite _left;
    [SerializeField] private Sprite _right;

    private SpriteRenderer m_spriteRenderer;

    [SerializeField] private float destroyTimer = 1f;

    private float m_timer;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_timer = destroyTimer;
    }

    public void SetSpriteFromDirection(Vector3 direction)
    {
        if (direction.y > math.abs(direction.x)) m_spriteRenderer.sprite = _back;
        if (direction.y < -math.abs(direction.x)) m_spriteRenderer.sprite = _front;
        if (direction.x <= -math.abs(direction.y)) m_spriteRenderer.sprite = _left;
        if (direction.x >= math.abs(direction.y)) m_spriteRenderer.sprite = _right;
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (m_timer < 0)
        {
            Destroy(gameObject);
            return;
        }

        var color = m_spriteRenderer.color;
        color.a = m_timer / destroyTimer;
        m_spriteRenderer.color = color;
    }
}
