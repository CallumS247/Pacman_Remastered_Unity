using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GameManager gameManager;

    public GameObject currentNode;
    public float speed;

    public string direction = "";
    public string lastMovingDirection = "";

    private bool canWarp = true;

    public bool isGhost = false;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }


    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }

        nodeController currentNodeController = currentNode.GetComponent<nodeController>();

        transform.position = Vector2.MoveTowards(transform.position, currentNode.transform.position, speed * Time.deltaTime);

        //Dont wait for player to reach the center of a node if reversing direction
        bool reverseDirection = false;
        if (
            (direction == "left" && lastMovingDirection == "right") ||
            (direction == "right" && lastMovingDirection == "left") ||
            (direction == "up" && lastMovingDirection == "down")    ||
            (direction == "down" && lastMovingDirection == "up")
            )
            {
            reverseDirection = true;
            }

        //Check if player is at the center of the node
        if ((transform.position.x == currentNode.transform.position.x && transform.position.y == currentNode.transform.position.y) || reverseDirection)
        {
            if (isGhost)
            {
                GetComponent<EnemyController>().CentreOfNodeReached(currentNodeController);
            }

            //If player reaches left warp node, move them to right warp
            if (currentNodeController.isWarpLeftNode && canWarp)
            {
                currentNode = gameManager.rightWarpNode;
                direction = "left";
                lastMovingDirection = "left";
                transform.position = currentNode.transform.position;
                canWarp = false;
            }
            //If player reaches right warp node, move them to left warp
            else if (currentNodeController.isWarpRightNode && canWarp)
            {
                currentNode = gameManager.leftWarpNode;
                direction = "right";
                lastMovingDirection = "right";
                transform.position = currentNode.transform.position;
                canWarp = false;
            }
            else
            {
                //if object is not a respawning ghost trying to move into ghost home, stop.
                if (currentNodeController.isGhostStartNode && direction == "down" && 
                    (!isGhost || GetComponent<EnemyController>().ghostState != EnemyController.GhostNodeStates.respawning))
                    {
                        direction = lastMovingDirection;
                    }
                {

                }
                //Get next node using current player direction
                GameObject newNode = currentNodeController.GetNodeFromDirection(direction);
                //If node can be moved to
                if (newNode != null)
                {
                    currentNode = newNode;
                    lastMovingDirection = direction;
                }
                //If node cant be moved to, keep going in current direction
                else
                {
                    direction = lastMovingDirection;
                    newNode = currentNodeController.GetNodeFromDirection(direction);
                    if (newNode != null)
                    {
                        currentNode = newNode;
                    }
                }
            }

        }
        //Not in the center of a node (ensures player doesnt infinitely warp)
        else
        {
            canWarp = true; 
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }


    public void SetDirection(string newDirection)
    {
        direction = newDirection;

    }

}
