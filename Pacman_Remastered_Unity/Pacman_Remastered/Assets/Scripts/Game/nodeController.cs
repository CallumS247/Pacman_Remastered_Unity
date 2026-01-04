using UnityEngine;

public class nodeController : MonoBehaviour
{
    public GameManager gameManager;

    public bool canMoveLeft = false;
    public bool canMoveRight = false;
    public bool canMoveUp = false; 
    public bool canMoveDown = false;

    public GameObject nodeLeft;
    public GameObject nodeRight;
    public GameObject nodeUp;
    public GameObject nodeDown;

    public bool isWarpRightNode = false;
    public bool isWarpLeftNode = false;

    private bool isPelletNode = false;
    private bool hasPellet = false;
    public SpriteRenderer pelletSprite;

    public bool isGhostStartNode = false;
    public bool isSideNode = false;

    public bool isPowerPellet = false;
    private float powerPelletTimer = 0;

    void Awake()
    {
        //Initialize nodes and their sprites
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (transform.childCount > 0)
        {
            gameManager.gotPelletFromNodeController(this);
            hasPellet = true;
            isPelletNode=true;
            pelletSprite = GetComponentInChildren<SpriteRenderer>();
        }

        #region DownRaycast
        RaycastHit2D[] hitsDown;
        hitsDown = Physics2D.RaycastAll(transform.position, -Vector2.up);

        for (int i = 0; i < hitsDown.Length; i++)
        {
            float distance = Mathf.Abs(hitsDown[i].point.y - transform.position.y);
            if (distance < 0.4f && hitsDown[i].collider.tag == "Node")
            {
                canMoveDown = true;
                nodeDown = hitsDown[i].collider.gameObject;
            }
        }
        #endregion

        #region UpRaycast
        RaycastHit2D[] hitsUp;
        hitsUp = Physics2D.RaycastAll(transform.position, Vector2.up);

        for (int i = 0; i < hitsUp.Length; i++)
        {
            float distance = Mathf.Abs(hitsUp[i].point.y - transform.position.y);
            if (distance < 0.4f && hitsUp[i].collider.tag == "Node")
            {
                canMoveUp = true;
                nodeUp = hitsUp[i].collider.gameObject;
            }
        }
        #endregion

        #region LeftRaycast
        RaycastHit2D[] hitsLeft;
        hitsLeft = Physics2D.RaycastAll(transform.position, Vector2.left);

        for (int i = 0; i < hitsLeft.Length; i++)
        {
            float distance = Mathf.Abs(hitsLeft[i].point.x - transform.position.x);
            if (distance < 0.4f && hitsLeft[i].collider.tag == "Node")
            {
                canMoveLeft = true;
                nodeLeft = hitsLeft[i].collider.gameObject;
            }

        }
        #endregion

        #region RightRaycast
        RaycastHit2D[] hitsRight;
        hitsRight = Physics2D.RaycastAll(transform.position, Vector2.right);

        for (int i = 0; i < hitsRight.Length; i++)
        {
            float distance = Mathf.Abs(hitsRight[i].point.x - transform.position.x);
            if (distance < 0.4f && hitsRight[i].collider.tag == "Node")
            {
                canMoveRight = true;
                nodeRight = hitsRight[i].collider.gameObject;
            }
        }
        #endregion

        if (isGhostStartNode)
        {
            canMoveDown = true;
            nodeDown = gameManager.ghostNodeCentre;
        }
    }

    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }
        //Blinking effect for power pellets
        if (isPowerPellet && hasPellet)
        {
            powerPelletTimer += Time.deltaTime;
            if (powerPelletTimer >= 0.2f)
            {
                powerPelletTimer = 0;
                pelletSprite.enabled = !pelletSprite.enabled;
            }
        }
    }

    public GameObject GetNodeFromDirection(string Direction)
    { //Get adjacent nodes for player and ghost movement
        if (Direction == "left" && canMoveLeft)
        {
            return nodeLeft;
        }
        else if (Direction == "right" && canMoveRight)
        {
            return nodeRight;
        }
        else if (Direction == "up" && canMoveUp)
        {
            return nodeUp;
        }
        else if (Direction == "down" && canMoveDown)
        {
            return nodeDown;
        }
        else
        {
            return null;
        }
    }

    public void RespawnPellet()
    { //Resetting pellets after level clear or game over
        if (isPelletNode)
        {
            hasPellet = true;
            pelletSprite.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    { //Player eats a pellet
        if (collision.tag == "Player" && hasPellet)
        {
            hasPellet = false;
            pelletSprite.enabled = false;
            StartCoroutine(gameManager.eatenPellet(this));
        }
    }
}
