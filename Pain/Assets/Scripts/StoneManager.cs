using UnityEngine;

public class ThrowStone : MonoBehaviour
{

    private void Start()
    {
        Invoke(nameof(DestroyItSelf), 5f);
    }

    void DestroyItSelf()
    {
        Destroy(gameObject);
    }
}