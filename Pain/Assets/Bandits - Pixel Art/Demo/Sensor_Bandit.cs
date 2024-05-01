using UnityEngine;
using System.Collections;

public class Sensor_Bandit : MonoBehaviour {

    private int m_ColCount = 0;//Collision Count

    private float m_DisableTimer;

    private BoxCollider2D m_Collider;
    private Transform colliderTransform;

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    private void Start()
    {
        m_Collider = GetComponent<BoxCollider2D>();
        colliderTransform = m_Collider.transform;
    }

    public bool State()
    {
        if (m_DisableTimer > 0)
            return false;
        return m_ColCount > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        m_ColCount++;//Herhangi bir nesne icine girerse colcount++
    }

    void OnTriggerExit2D(Collider2D other)
    {
        m_ColCount--;
    }

    void Update()
    {
        m_DisableTimer -= Time.deltaTime;
        m_Collider.transform.position = colliderTransform.position;
    }

    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }
}
