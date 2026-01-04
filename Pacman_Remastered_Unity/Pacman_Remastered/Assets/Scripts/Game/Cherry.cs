using UnityEngine;

public class Cherry : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] private SpriteRenderer cherrySprite;
    public bool IsEaten { get; private set; } = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsEaten = true;
            cherrySprite.enabled = false;
            gameManager.FruitEaten();
        }
    }
}
