using System.Collections;
using UnityEngine;

public class TreeScript : MonoBehaviour
{
    [Header("Child object")]
    public GameObject capsule;
    public GameObject treeUpper;
    public GameObject treeBase;
    //Tree Upper part Collider attribute
    /// <summary>
    /// Adjust the hitbox for the tree here,
    /// </summary> 
    private float treeColliderXOffset = 0.01937675f;
    private float treeColliderYOffset = -0.07750416f;
    private Vector2 treeColliderSize = new Vector2(0.9f, 1.844992f);
    private ItemPunctuate itemPunctuate;
    //Capsule tree holder attribute
    private float circleColliderXOffset = 0f;
    private float circleColliderYOffset = 0f;
    private float circleColliderRadius = 0.5f;
    // tree flunctuate when getting hit, the force will split two side of the tree
    [Header("Tree Attribute")]
    public int TreeHealth = 3;
    Rigidbody2D treeUpperRB;
    // Start is called before the first frame update
    void Start()
    {

        treeUpperRB = treeUpper.GetComponent<Rigidbody2D>();
        itemPunctuate = this.GetComponent<ItemPunctuate>();
    }

    // Update is called once per frame
    void Update()
    {
    }
    public float treeDecayTime = 2f;
    public void InstantiateUpperTreeResource()
    {

    }
    IEnumerator TreeDecay()
    {
        //tree upper base
        BoxCollider2D treeHolderCollider2D = treeUpper.AddComponent<BoxCollider2D>();
        treeHolderCollider2D.isTrigger = false;
        treeHolderCollider2D.size = this.treeColliderSize;
        treeHolderCollider2D.offset = new Vector2(treeColliderXOffset, treeColliderYOffset);
        //circle holder
        CircleCollider2D circleCollider2D = capsule.AddComponent<CircleCollider2D>();
        circleCollider2D.isTrigger = false;
        circleCollider2D.radius = this.circleColliderRadius;
        circleCollider2D.offset = new Vector2(circleColliderXOffset, circleColliderYOffset);
        //Ground holder
        GameObject gameObject =new GameObject();
        gameObject.transform.parent = this.transform;
        gameObject.transform.position= this.treeBase.transform.position;
        BoxCollider2D groundCollider2D = gameObject.AddComponent<BoxCollider2D>();
        groundCollider2D.isTrigger = false;
        
        groundCollider2D.size = new Vector2(3.5f, 0.05f);
        groundCollider2D.offset = new Vector2(0, -0.5f);
        //Add new collider
        
        treeUpperRB.constraints = RigidbodyConstraints2D.None;
        yield return new WaitForSeconds(treeDecayTime);
        Destroy(circleCollider2D);
        Destroy(treeUpper);
        Destroy(groundCollider2D);
        Destroy(gameObject);
        FindObjectOfType<GameManager>().SpawnResourceMaterial(treeUpper.transform.position,5,"Wood");
    }
    public void GetHit()
    {
        if (treeUpper != null)
        {
            itemPunctuate.Punctuate();
            TreeHealth -= 1;
            if (TreeHealth <= 0)
            {

                StartCoroutine(TreeDecay());
            }
        }

    }

}