using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[SerializeField]
public class PlayerInventory
{
    [SerializeField]
    public List<IItemSlot> _items { get; private set; }
    [SerializeField]
    private int SlotCount;
    public PlayerInventory(int slotcount)
    {
        SlotCount = slotcount; _items = new List<IItemSlot>();
        for (int i = 0; i < SlotCount; i++)
        {
            _items.Add(null);
        }
    }
    public int Count
    {
        get { return _items.Count; }
    }
    public int AddItem(IItemSlot itemSlot)
    {
        int indexNull = -1;

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] == null)
            {
                if (indexNull == -1)
                {
                    indexNull = i;
                }
            
            }
            else if (itemSlot.itemId == _items[i].itemId)
            {
                _items[i].count++;
                return i;
            }

        }
        if (indexNull != -1)
        {
            _items[indexNull] = itemSlot;
            return indexNull;
        }
        return -1;

    }
    public bool isSlotFull()
    {
        return _items.Count == SlotCount;
    }

    public bool RemoveItem(IItemSlot itemSlot)
    {
        return _items.Remove(itemSlot);
    }

    public IEnumerable<IItemSlot> GetItemsOfType(IItemSlot type)
    {
        return _items.Where(item => item.Equals(type));
    }

    public IItemSlot GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
        {
            return null;
        }
        return _items[index];
    }
    public void SwapSlot(IItemSlot itemSlot1, IItemSlot itemSlot2)
    {
        var temp = _items[_items.IndexOf(itemSlot1)];
        _items[_items.IndexOf(itemSlot1)] = itemSlot2;
        _items[_items.IndexOf(itemSlot2)] = temp;
    }
    //Migrate item from one list to another, ignore if the target item is not null and will be replace
    public void MigrateItemInList(IItemSlot itemSlot, int targetIndex)
    {
        if (_items[targetIndex] == null)
        {
            _items[targetIndex] = itemSlot;
            _items[_items.IndexOf(itemSlot)] = null;
        }
    }

}
public class PlayerManager : MonoBehaviour
{
    public GameObject playerObject;
    public GameObject panelUIHub;
    private InputManager inputManager;
    public Sprite[] spriteTools;
    public float rangeRadius = 10f;
    public int toolIndex = 0;
    public Sprite selectedSprite;
    public GameObject selectedObject;
    public List<Transform> listButtons;
    GameManager gameManager;
    ItemSlotScript itemSlotScript;
    [Header("UI Menu attributes")]
    public Color colorSelectBox;
    private Collider2D selectObjectCollider2D;
    public LayerMask sphereCastLayerMask;
    public float castRadius = 1f;
    public GameObject selectedGameObject;
    public List<GameObject> listInRangeGameObject;
    private Animator animator;
    [Header("Player Attributes")]
    public float attackSpeed = 1f;
    public float kinematicRange = 5f;
    public float maxHealth = 100;
    private float chargingTime = 0f;
    [Header("Player Menu Attributes")]
    public PlayerInventory playerInventory = new PlayerInventory(10);
    //Inner game attribute
    private float hitTimeFrame = 0.3f;
    private int hashNameHit;
    private PlayerLocomotion playerLocomotion;
    public bool isFreezePosition = false;
    public bool isFreezeRotation = false;
    [HideInInspector]
    //Position to the world space of the player action on the screen
    public float actionDelayTime = 0.5f;
    float actionDelayCounter = 0;
    public Vector2 actionPosition;
    // Start is called before the first frame update
    void Start()
    {
        //Initialize variables
        playerLocomotion = GetComponent<PlayerLocomotion>();
        animator = GetComponent<Animator>();
        hashNameHit = Animator.StringToHash("Hit");
        inputManager = FindObjectOfType<InputManager>();
        selectObjectCollider2D = selectedObject.GetComponent<Collider2D>();
        var tempList = panelUIHub.transform.GetChild(0);
        itemSlotScript = new ItemSlotScript();
        for (int i = 0; i < tempList.transform.childCount; i++)
        {
            listButtons.Add(tempList.GetChild(i));
        }
        Vector3 mousePos = MouseToWorldPoint();
        //A direction from player position to mouse position
        Vector3 direction = mousePos - transform.position;
        gameManager = FindObjectOfType<GameManager>();
        //Add button event listener 
        foreach (var item in listButtons)
        {
            Button button = item.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                selectedGameObject.GetComponent<SpriteRenderer>().sprite = item.GetComponent<SpriteRenderer>().sprite;
            });
        }
    }

    Vector3 GetObjectOnCircleOutlineWithAngle(Vector3 center, float radius, float angleInDegrees)
    {
        float distance = Vector2.Distance(center, MouseToWorldPoint()); // this.playerObject.transform.position
        if (radius > distance)
        {
            radius = distance;
        }
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        float x = center.x + radius * Mathf.Cos(angleInRadians);
        float y = center.y + radius * Mathf.Sin(angleInRadians);
        Vector3 positionOnCircle = new Vector3(x, y, -3f);
        // Place the new object at the calculated position
        return positionOnCircle;
    }
    float GetAngleFromMouseToPlayer()
    {
        Vector3 mousePos = MouseToWorldPoint();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
    private void Update()
    {
        HandleKinematic();
        if (chargingTime < attackSpeed)
        {
            chargingTime += Time.deltaTime;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        HandleToolMovement();
        HandleAction();
        OnToolTriggerEnter2D();
        HandlePlayerAction();

    }
    Vector3 MouseToWorldPoint()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane; // Set the z position to the near clip plane of the camera
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
    public void EquipTool(int i)
    {
        for (int item = 0; item < listButtons.Count; item++)
        {
            listButtons[item].GetComponent<Image>().color = Color.white;

            if (i == item)
            {
                listButtons[item].GetComponent<Image>().color = colorSelectBox;
                selectedSprite = listButtons[item].GetChild(0).GetComponent<Image>().sprite;
            }
        }
    }

    void HandleAction()
    {
        if (actionDelayCounter < actionDelayTime)
            actionDelayCounter += Time.deltaTime;
        if (Input.GetKey(KeyCode.Tab) && actionDelayCounter >= actionDelayTime)
        {
            actionDelayCounter = 0;
            gameManager.OpenPlayerMenu();
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            EquipTool(0);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            EquipTool(1);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            EquipTool(2);
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            EquipTool(3);
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            EquipTool(4);
        }
        else if (Input.GetKey(KeyCode.Alpha6))
        {
            EquipTool(5);
        }
        else if (Input.GetKey(KeyCode.Alpha7))
        {
            EquipTool(6);
        }
        else if (Input.GetKey(KeyCode.Alpha8))
        {
            EquipTool(7);
        }
        else if (Input.GetKey(KeyCode.Alpha9))
        {
            EquipTool(8);
        }



    }
    void HandleToolMovement()
    {
        selectedObject.transform.position = GetObjectOnCircleOutlineWithAngle(playerObject.transform.position, rangeRadius, GetAngleFromMouseToPlayer());
    }
    IEnumerator PlayerHit<T>(T script)
    {
        if (script is TreeScript)
        {
            animator.Play(hashNameHit);
            yield return new WaitForSeconds(hitTimeFrame);
            isFreezePosition = false;
            isFreezeRotation = false;
            (script as TreeScript).GetHit();
        }
        if (script is RockScript)
        {
            animator.Play(hashNameHit);
            yield return new WaitForSeconds(hitTimeFrame);
            isFreezePosition = false;
            isFreezeRotation = false;
            (script as RockScript).GetHit();
        }
    }
    void HandlePlayerAction()
    {
        if (Input.GetKey(KeyCode.E))
        {
            if (selectedGameObject != null)
            {
                actionPosition = MouseToWorldPoint();
                var treeScript = selectedGameObject.GetComponent<TreeScript>();

                if (treeScript != null && chargingTime >= attackSpeed)
                {
                    isFreezePosition = true;
                    isFreezeRotation = true;
                    chargingTime = 0;
                    StopCoroutine(PlayerHit(treeScript));
                    playerLocomotion.HandleRotation(GetActionDirection());
                    StartCoroutine(PlayerHit(treeScript));
                    return;
                }
                var rockScript = selectedGameObject.GetComponent<RockScript>();
                if (rockScript != null && chargingTime >= attackSpeed)
                {
                    isFreezePosition = true;
                    isFreezeRotation = true;
                    chargingTime = 0;
                    playerLocomotion.HandleRotation(GetActionDirection());
                    StopCoroutine(PlayerHit(rockScript));
                    StartCoroutine(PlayerHit(rockScript));
                    return;
                }
            }
        }
    }
    void HandleKinematic()
    {
        RaycastHit2D[] raycastHit2Ds = Physics2D.CircleCastAll(playerObject.transform.position, kinematicRange, Vector2.zero);
        if (raycastHit2Ds.Count() > 0)
        {
            foreach (var ray in raycastHit2Ds)
            {

                ItemScript itemScript = ray.transform.GetComponent<ItemScript>();
                if (itemScript != null)
                {

                    itemScript.StartTransaction(this.transform);
                }
            }
        }
    }
    public Vector2 GetActionDirection()
    {
        Vector2 direction = actionPosition - (Vector2)playerObject.transform.position;
        direction.Normalize();
        direction.x = Mathf.Round(direction.x);
        direction.y = Mathf.Round(direction.y);
        return -direction;
    }
    /// <summary>
    /// Draws a red sphere in the scene view at the position of the player object with a radius
    /// determined by the rangeRadius variable.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(playerObject.transform.position, rangeRadius);
        Gizmos.DrawRay(selectedObject.transform.position, (MouseToWorldPoint() - selectedObject.transform.position).normalized * castRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerObject.transform.position, kinematicRange);
    }

    void OnToolTriggerEnter2D()
    {
        RaycastHit2D[] tempraycastHit2D = Physics2D.RaycastAll(selectedObject.transform.position, (MouseToWorldPoint() - selectedObject.transform.position).normalized, castRadius);
        if (tempraycastHit2D.GetLength(0) > 0)
        {
            float distance = 0f;
            listInRangeGameObject.Clear();
            selectedGameObject = null;
            foreach (var ray in tempraycastHit2D)
            {

                if (ray.transform.tag == "Tree" || ray.transform.tag == "Rock")
                {
                    listInRangeGameObject.Add(ray.transform.gameObject);
                    float tempdistance = Vector2.Distance(selectedObject.transform.position, ray.transform.position);
                    if (tempdistance > distance)
                    {
                        distance = tempdistance;
                        selectedGameObject = ray.transform.gameObject;
                    }
                }
            }
        }
    }


}
