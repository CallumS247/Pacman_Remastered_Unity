using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && gameManager.isInvincible == false)
        {
            gameManager.StartCoroutine(gameManager.playerEaten());
            //Prevent double death
            gameManager.isInvincible = true;
        }
    }
}

