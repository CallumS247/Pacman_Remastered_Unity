using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameManager gameManager;

    public MovementController movementController;
    public SpriteRenderer sprite;
    public Animator animator;

    public GameObject startingNode;
    private Vector2 startPos;

    private bool isDead = false;

    public bool invertedControls = false;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();
        startingNode = movementController.currentNode;
        movementController.SetSpeed(2.1f);
        animator = GetComponentInChildren<Animator>();
    }

    public void Setup()
    { //Used when starting a game or resetting a level
        isDead = false;
        sprite = GetComponentInChildren<SpriteRenderer>();
        movementController.currentNode = startingNode;
        movementController.direction = "left";
        movementController.lastMovingDirection = "left";
        sprite.flipX = false;
        startPos = new Vector2(-0.018f, -0.641f);
        transform.position = startPos;

        animator = GetComponentInChildren<Animator>();
        animator.SetBool("Moving", false);
        animator.SetBool("dead", false);
        animator.speed = 1;
    }    

    
    void Update()
    {
        if (!gameManager.gameIsRunning || gameManager.gameIsPaused)
        {
            if (!isDead)
            {
                animator.speed = 0;
            }
            return;
        }

        animator.speed = 1;
        animator.SetBool("Moving", true);
        //Movement Controls
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            movementController.SetDirection("left");
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            movementController.SetDirection("right");
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            movementController.SetDirection("up");
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            movementController.SetDirection("down");
        }

        //Inverted Controls Hazard 
        if (invertedControls)
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                movementController.SetDirection("right");
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                movementController.SetDirection("left");
            }
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                movementController.SetDirection("down");
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                movementController.SetDirection("up");
            }
        }
        
        
        bool flipX = false;
        bool flipY = false;
        //Play animation depending on direction
        if (movementController.lastMovingDirection == "left")
        {
            animator.SetInteger("Direction", 0);
        }
        else if (movementController.lastMovingDirection == "right")
        {
            animator.SetInteger("Direction", 0);
            flipX = true;
        }
        else if (movementController.lastMovingDirection == "up")
        {
            animator.SetInteger("Direction", 1);
        }
        else if (movementController.lastMovingDirection == "down")
        {
            animator.SetInteger("Direction", 1);
            flipY = true;
        }

        sprite.flipY = flipY;
        sprite.flipX = flipX;
    }

    public void Stop()
    {
        animator.speed = 0;
    }

    public void Death()
    { //Play death animation
        isDead = true;
        animator.SetBool("Moving", false);
        animator.speed = 1;
        animator.SetBool("dead", true);
    }
}
