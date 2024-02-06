using UnityEngine;

public class ThrowStone : MonoBehaviour
{
    private GameObject stonePrefab;

    private void Start()
    {
        stonePrefab = GameObject.FindGameObjectWithTag("stone");
    }

    public void ThrowObject(Vector2 throwAxis)
    {
        Instantiate(stonePrefab, transform.position, Quaternion.identity);
    }
}