using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance {  get { return instance; } }

    //Player Variables
    public GameObject playerObject;
    public Transform playerTransform;
    public  PlayerMovement playerScript;


    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); }
        else { instance = this; }
    }

    private void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        playerTransform = playerObject.transform;
        playerScript = playerObject.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        playerTransform.position = playerObject.transform.position;
    }

    public void AttackToPlayer(int damageAmount)
    {
        playerScript.GetDamage(damageAmount);
    }

    public bool IsPlayerHiding()
    {
        return playerScript.isOnCloak;
    }
}
