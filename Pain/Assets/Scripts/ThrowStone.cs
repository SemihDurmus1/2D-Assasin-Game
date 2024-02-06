using UnityEngine;

public class StoneManager : MonoBehaviour
{
    private Rigidbody2D rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}