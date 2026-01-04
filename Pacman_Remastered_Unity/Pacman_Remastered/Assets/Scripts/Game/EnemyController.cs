using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    public enum GhostNodeStates
    {
        respawning,
        leftNode,
        rightNode,
        centerNode,
        startNode,
        movingNodes
    }

    public GhostNodeStates ghostState;
    public GhostNodeStates ghostStartState;
    public GhostNodeStates respawnState;

    public enum GhostType
    {
        red,
        blue,
        pink,
        orange
    }

    public GhostType ghostType;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCentre;
    public GameObject ghostNodeStart;

    public GameManager gameManager;
    public MovementController movementController;

    public GameObject startingNode;
    public bool readyToLeaveHome = false;

    public bool isFrightened = false;

    public GameObject[] scatterNodes;
    public int scatterNodeIndex;

    public bool leftHomeBefore = false;

    public bool isVisible = true;

    public SpriteRenderer ghostSprite;
    public SpriteRenderer ghostEyesSprite;
    public Sprite eyesLeft;
    public Sprite eyesRight;
    public Sprite eyesUp;
    public Sprite eyesDown;

    public Animator animator;

    public Color colour;

    public bool isFrozen = false;

    public bool ImmuneHazardOn = false;
    

    void Awake()
    {
        animator = GetComponent<Animator>();
        ghostSprite = GetComponent<SpriteRenderer>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController =  GetComponent<MovementController>();
        if(ghostType == GhostType.red) //RedGhost - starts above ghost home
        {
            ghostStartState = GhostNodeStates.startNode;
            respawnState = GhostNodeStates.centerNode;
            startingNode = ghostNodeStart;
        }
        else if(ghostType == GhostType.pink) //PinkGhost - starts in centre node
        {
            ghostStartState = GhostNodeStates.centerNode;
            startingNode = ghostNodeCentre;
            respawnState = GhostNodeStates.centerNode;
        }
        else if (ghostType == GhostType.blue) //BlueGhost - strarts in left node
        {
            ghostStartState = GhostNodeStates.leftNode;
            startingNode = ghostNodeLeft;
            respawnState = GhostNodeStates.leftNode;
        }
        else if (ghostType == GhostType.orange) //OrangeGhost - starts in right node
        {
            ghostStartState = GhostNodeStates.rightNode;
            startingNode = ghostNodeRight;
            respawnState = GhostNodeStates.rightNode;
        }
    }

    void Update()
    {
        if (ghostState != GhostNodeStates.movingNodes || !gameManager.isPowerPelletRunning)
        {
            isFrightened = false;
        }

        if (isVisible) //Show sprites
        {
            if (ghostState != GhostNodeStates.respawning)
            {
                ghostSprite.enabled = true;
            }
            else
            {
                ghostSprite.enabled = false;
            }

            ghostEyesSprite.enabled = true;
        }
        else //Hide sprites
        {
            ghostSprite.enabled = false;
            ghostEyesSprite.enabled = false;
        }

        //Power pellet is picked up, changed to frightened mode
        if (isFrightened)
        {
            animator.SetBool("Frightened", true);
            ghostEyesSprite.enabled = false;
            ghostSprite.color = new Color(255, 255, 255, 255);
        }
        else
        {
            animator.SetBool("Frightened", false);
            animator.SetBool("frightenedBlinking", false);
            ghostSprite.color = colour;
        }

        if (!gameManager.gameIsRunning)
        {
            return;
        }

        //Ghost flashes when power pellet is about to run out
        if (gameManager.powerPelletTimer - gameManager.currentPowerPelletTime <= 3)
        {
            animator.SetBool("frightenedBlinking", true);
        }
        else
        {
            animator.SetBool("frightenedBlinking", false);
        }

        animator.SetBool("Moving", true);

        //Slow ghost when moving to warp nodes
        if (movementController.currentNode.GetComponent<nodeController>().isSideNode)
        {
            movementController.SetSpeed(1f);
        }
        else
        {
            if (isFrightened)
            {
                movementController.SetSpeed(1f);
            }
            else if (ghostState == GhostNodeStates.respawning)
            { //Increase speed for fast respawning
                movementController.SetSpeed(5f);
            }
            else if(isFrozen)
            { //Freeze ability - stop movement
                movementController.SetSpeed(0f);
            }
            else
            { //Regular moving speed
                movementController.SetSpeed(1.85f);
            }
        }

        //Change eye sprite depending on direction
        if (movementController.lastMovingDirection == "left")
        {
            ghostEyesSprite.sprite = eyesLeft;
        }
        else if (movementController.lastMovingDirection == "right")
        {
            ghostEyesSprite.sprite = eyesRight;
        }
        else if (movementController.lastMovingDirection == "up")
        {
            ghostEyesSprite.sprite = eyesUp;
        }
        else if (movementController.lastMovingDirection == "down")
        {
            ghostEyesSprite.sprite = eyesDown;
        }

    }

    public void Setup()
    {

        animator.SetBool("Moving", false);

        ghostState = ghostStartState;
        readyToLeaveHome = false;
        leftHomeBefore = false;
        isFrozen = false;

        //Reset ghosts to starting positions
        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;
        movementController.direction = "";
        movementController.lastMovingDirection = "";

        //Reset scatter node index
        scatterNodeIndex = 0;

        //Turn off frightened mode
        isFrightened = false;

        //Reset ghosts
        if (ghostType == GhostType.red)
        {
            readyToLeaveHome = true;
            leftHomeBefore = true;
        }
        if (ghostType == GhostType.pink)
        {
            readyToLeaveHome = true;
        }

        SetVisible(true);
    }


    public void CentreOfNodeReached (nodeController nodeController)
    {
        if (ghostState == GhostNodeStates.movingNodes)
        {
            leftHomeBefore = true;
            //Scatter Mode
            if (gameManager.currentGhostMode == GameManager.GhostMode.Scatter)
            {
                ghostScatterModeDirection();
            }
            //Frightened mode
            else if (isFrightened)
            {
                string direction =  getRandomDirection();
                movementController.SetDirection(direction);
                isFrozen = false;
            }
            //Chase mode
            else
            {
                //Determine next node to go to depending on ghost colour
                if (ghostType == GhostType.red)
                {
                    DetermineRedDirection();
                }
                else if (ghostType == GhostType.pink)
                {
                    DeterminePinkDirection();
                }
                else if (ghostType == GhostType.blue)
                {
                    DetermineBlueDirection();
                }
                else if (ghostType == GhostType.orange)
                {
                    DetermineOrangeDirection();
                }
            }
        }
        else if (ghostState == GhostNodeStates.respawning)
        {
            isFrozen = false;
            string direction = "";
            //Reached start node, move to centre of the home
            if (transform.position.x == ghostNodeStart.transform.position.x && transform.position.y == ghostNodeStart.transform.position.y)
            {
                direction = "down";
            }
            //Reached centre node, finish respawn or move left/right
            else if (transform.position.x == ghostNodeCentre.transform.position.x && transform.position.y == ghostNodeCentre.transform.position.y)
            {
                if (respawnState == GhostNodeStates.centerNode)
                {
                    ghostState = respawnState;
                }
                else if (respawnState == GhostNodeStates.leftNode)
                {
                    direction = "left";
                }
                else if (respawnState == GhostNodeStates.rightNode)
                {
                    direction = "right";
                }
            }
            //if respawn is in left/right node and it has been reached, leave home again.
            else if (
                (transform.position.x == ghostNodeLeft.transform.position.x && transform.position.y == ghostNodeLeft.transform.position.y) ||
                (transform.position.x == ghostNodeRight.transform.position.x && transform.position.y == ghostNodeRight.transform.position.y)
                )
            {
                ghostState = respawnState;
            }
            //Ghost is still on the map
            else
            {
                //Determine quickest direction to home
                direction = GetClosestDirection(ghostNodeStart.transform.position);
            }
            movementController.SetDirection(direction);
        }
        else
        {
            //If ghost is ready to leave home
            if (readyToLeaveHome)
            {
                //if in left home node, move to centre
                if (ghostState == GhostNodeStates.leftNode)
                {
                    ghostState = GhostNodeStates.centerNode;
                    movementController.SetDirection("right");
                }
                //if in right home node, move to centre
                else if (ghostState == GhostNodeStates.rightNode)
                {
                    ghostState  = GhostNodeStates.centerNode;
                    movementController.SetDirection("left");
                }
                //if in the centre node, then move to the start node
                else if (ghostState == GhostNodeStates.centerNode)
                {
                    ghostState = GhostNodeStates.startNode;
                    movementController.SetDirection("up");
                }
                //if in the start node, then start moving in the regular game nodes.
                else if (ghostState == GhostNodeStates.startNode)
                {
                    ghostState = GhostNodeStates.movingNodes;
                    switch (Random.Range(0, 2))
                    {
                        case 0:
                            movementController.SetDirection("left");
                            break;
                        case 1:
                            movementController.SetDirection("right");
                            break;
                    }
                }
            }
        }
    }

    //RED GHOST MOVEMENT
    void DetermineRedDirection()
    { //Chases the current player position
        string direction = GetClosestDirection(gameManager.PacMan.transform.position);
        movementController.SetDirection(direction);
    }

    //PINK GHOST MOVEMENT
    void DeterminePinkDirection()
    { //Aims to move 2 nodes infront of the player depending on the direction they're facing
        string playerDirection = gameManager.PacMan.GetComponent<MovementController>().lastMovingDirection;
        float nodeDistance = 0.35f;

        Vector2 target = gameManager.PacMan.transform.position;

        if (playerDirection == "left")
        {
            target.x -= nodeDistance * 2;
        }
        else if (playerDirection == "right")
        {
            target.x += nodeDistance * 2;
        }
        else if (playerDirection == "up")
        {
            target.y += nodeDistance * 2;
        }
        else if (playerDirection == "down")
        {
            target.y -= nodeDistance * 2;
        }

        string direction = GetClosestDirection(target);
        movementController.SetDirection(direction);
    }

    //BLUE GHOST MOVEMENT
    void DetermineBlueDirection()
    { //Gets node 2 ahead of player. Finds distance of red ghost from target, and doubles it
        string playerDirection = gameManager.PacMan.GetComponent<MovementController>().lastMovingDirection;
        float nodeDistance = 0.35f;

        Vector2 target = gameManager.PacMan.transform.position;

        if (playerDirection == "left")
        {
            target.x -= nodeDistance * 2;
        }
        else if (playerDirection == "right")
        {
            target.x += nodeDistance * 2;
        }
        else if (playerDirection == "up")
        {
            target.y += nodeDistance * 2;
        }
        else if (playerDirection == "down")
        {
            target.y -= nodeDistance * 2;
        }

        GameObject redGhost = gameManager.redGhost;
        float xDist = target.x - redGhost.transform.position.x;
        float yDist = target.y - redGhost.transform.position.y;

        Vector2 blueTarget = new Vector2 (target.x + xDist, target.y + yDist);

        string direction = GetClosestDirection(blueTarget);
        movementController.SetDirection(direction);
    }

    //ORANGE GHOST MOVEMENT
    void DetermineOrangeDirection()
    { //If within 8 nodes of player, chase them. Defaults to scatter mode if not.
        float distance = Vector2.Distance(gameManager.PacMan.transform.position, transform.position);
        float nodeDistance = 0.35f;
        //Ensure no negative values
        if (distance < 0)
        {
            distance *= -1;
        }

        if (distance <= nodeDistance * 8)
        {
            DetermineRedDirection();
        }
        else
        {
            ghostScatterModeDirection();
        }
    }

    //SCATTER MODE DIRECTION
    void ghostScatterModeDirection()
    {
        //If scatter node is reached, increment index
        if (transform.position.x == scatterNodes[scatterNodeIndex].transform.position.x && transform.position.y == scatterNodes[scatterNodeIndex].transform.position.y)
        {
            scatterNodeIndex++;

            if (scatterNodeIndex == scatterNodes.Length - 1)
            {
                scatterNodeIndex = 0;
            }
        }

        string direction = GetClosestDirection(scatterNodes[scatterNodeIndex].transform.position);

        movementController.SetDirection(direction);
    }

    //RANDOM DIRECTION
    string getRandomDirection()
    {
        List<string> possibleDirections = new List<string>();
        nodeController nodecontroller = movementController.currentNode.GetComponent<nodeController>();

        if (nodecontroller.canMoveDown && movementController.lastMovingDirection != "up")
        {
            possibleDirections.Add("down");
        }
        if (nodecontroller.canMoveUp && movementController.lastMovingDirection != "down")
        {
            possibleDirections.Add("up");
        }
        if (nodecontroller.canMoveRight && movementController.lastMovingDirection != "left")
        {
            possibleDirections.Add("right");
        }
        if (nodecontroller.canMoveLeft && movementController.lastMovingDirection != "right")
        {
            possibleDirections.Add("left");
        }

        string direction = "";
        if (possibleDirections.Count == 0)
        {
            return movementController.lastMovingDirection; //Default to last direction to prevent being stuck
        }

        int randomDirectionIndex = UnityEngine.Random.Range(0, possibleDirections.Count);

        direction = possibleDirections[randomDirectionIndex];
        return direction;
    }

    //CLOSEST DIRECTION PATHFINDING
    string GetClosestDirection(Vector2 target)
    {
        float shortestDistance = 0;
        string lastMovingDirection = movementController.lastMovingDirection;
        string newDirection = "";
        nodeController nodeController = movementController.currentNode.GetComponent<nodeController>();

        #region up node
        //If ghost can move up and isnt reversing
        if (nodeController.canMoveUp && lastMovingDirection != "down")
        {
            //get node above
            GameObject nodeUp = nodeController.nodeUp;
            //Distance between top node and target
            float distance = Vector2.Distance(nodeUp.transform.position, target);

            //if this is the shortest distance, set direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }
        #endregion

        #region left node
        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            //get left node
            GameObject nodeLeft = nodeController.nodeLeft;
            //Distance between top node and target
            float distance = Vector2.Distance(nodeLeft.transform.position, target);

            //if this is the shortest distance, set direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }
        #endregion

        #region right node
        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            //get right node
            GameObject nodeRight = nodeController.nodeRight;
            //Distance between top node and target
            float distance = Vector2.Distance(nodeRight.transform.position, target);

            //if this is the shortest distance, set direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }
        #endregion

        #region down node
        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            //get node below
            GameObject nodeDown = nodeController.nodeDown;
            //Distance between top node and target
            float distance = Vector2.Distance(nodeDown.transform.position, target);

            //if this is the shortest distance, set direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }
        #endregion

        return newDirection;
    }

    public void SetVisible(bool newIsVisible)
    {
        isVisible = newIsVisible;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && ghostState != GhostNodeStates.respawning)
        {
            //Ghost is eaten
            if (isFrightened && !ImmuneHazardOn)
            {
                gameManager.ghostEaten();
                ghostState = GhostNodeStates.respawning;
            }
            //Ghost eats player
            else
            {
                StartCoroutine(gameManager.playerEaten());
            }
        }
    }

    public void SetFrightened(bool newIsFrightened)
    {
        isFrightened = newIsFrightened;
    }
}
