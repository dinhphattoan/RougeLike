using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum eItemType
{
    Resource, Tool
}
public enum eItemId
{
    Wood = 0, Stone = 1, Pickaxe = 2, Axe = 3
}

public class IItemSlot
{
    public int itemId;
    public eItemType itemType;
    public int count;
    public IItemSlot(int itemId, eItemType eItemType, int count)
    {
        this.itemId = itemId;
        this.count = count;
        this.itemType = eItemType;
    }
}
public class GameManager : MonoBehaviour
{
    [Header("Resources")]
    public GameObject ResourcesObject;
    public List<GameObject> listMaterial_Stone;
    public List<GameObject> listMaterial_Wood;
    //A list of sprite that represent the item from listItemId.
    public List<Sprite> listSpriteRefItemId;
    [Header("UI references")]
    ///////////////////////
    //UI
    public GameObject objectUIMenu;
    //Left Panel
    public GameObject objectUILeftPanel;
    [Header("UI theme modifier")]
    [SerializeField]
    public Color itemSlotColor;
    public Color UIMenuColor;
    List<GameObject> listGameObjectSlotUI;
    AnimateImageShow animateImageShow;
    PlayerManager playerManager;
    public Canvas canvasToolButtons;
    //Right Panel
    public GameObject objectUIRightPanel;
    //Matrix of inventory slots
    [HideInInspector]
    public GameObject[,] objectMenuSlots;
    public DragDropManager dragDropManager;
    //////////////////////

    public bool isOpenMenu = false;

    // Start is called before the first frame update
    private void Start()
    {
        //Initial item rule
        //Initialize variables
        listMaterial_Stone = new List<GameObject>();
        listMaterial_Wood = new List<GameObject>();
        listGameObjectSlotUI = new List<GameObject>();
        animateImageShow = objectUILeftPanel.GetComponent<AnimateImageShow>();
        playerManager = FindObjectOfType<PlayerManager>();
        for (int i = 0; i < ResourcesObject.transform.childCount; i++)
        {
            var g = ResourcesObject.transform.GetChild(i);
            if (g.name.Contains("Rock"))
            {
                for (int k = 0; k < g.transform.childCount; k++)
                {
                    listMaterial_Stone.Add(g.GetChild(k).gameObject);
                }
            }
            else if (g.name.Contains("Wood"))
            {
                for (int k = 0; k < g.transform.childCount; k++)
                {
                    listMaterial_Wood.Add(g.GetChild(k).gameObject);
                }
            }
        }
        objectMenuSlots = new GameObject[2, 5];
        var tempObject = objectUIMenu.transform;
        int trace = 0;
        for (int i = 0; i < tempObject.childCount; i++)
        {
            var itemslot = tempObject.GetChild(i).gameObject;
            if (itemslot.name.Contains("Slot"))
            {
                objectMenuSlots[trace / 5, trace % 5] = itemslot;
                trace++;
            }


        }
       canvasToolButtons.enabled = false;
        isOpenMenu = false;
    }

    private void Update()
    {
        LoadInventory();
    }
    /// <summary>
    /// Spawns a given number of resource material at a specified position.
    /// </summary>
    /// <param name="position">The position to spawn the resource material.</param>
    /// <param name="count">The number of resource material to spawn.</param>
    /// <param name="resourceType">The type of resource material to spawn. Must be "Wood" or "Rock".</param>
    public void SpawnResourceMaterial(Vector3 position, int count, string resourceType)
    {
        // Check if the resource type is "Wood"
        if (resourceType.Contains("Wood"))
        {
            // Spawn the given number of wood resource material at the specified position
            for (int i = 0; i < count; i++)
            {
                GameObject wood = listMaterial_Wood[UnityEngine.Random.Range(0, listMaterial_Wood.Count)].gameObject;
                Instantiate(wood, position, Quaternion.identity);
            }
        }
        // Check if the resource type is "Rock"
        else if (resourceType.Contains("Rock"))
        {
            // Spawn the given number of rock resource material at the specified position
            for (int i = 0; i < count; i++)
            {
                GameObject stone = listMaterial_Stone[UnityEngine.Random.Range(0, listMaterial_Stone.Count)].gameObject;
                Instantiate(stone, position, Quaternion.identity);
            }
        }
    }

    public void LoadInventory()
    {
        foreach(var ob in dragDropManager.AllObjects)
        {
            int index = int.Parse(ob.Id.Substring(3));
            var objectText = ob.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            objectText.text = playerManager.playerInventory._items[index].count.ToString();
        }

    }
    public void OpenPlayerMenu()
    {
        isOpenMenu = !isOpenMenu;
        canvasToolButtons.enabled = isOpenMenu;
        //hide all objectItem in player inventory but not for tool menu
        List<PanelSettings> panelInventorySetting = dragDropManager.AllPanels.FindAll(p=>p.Id.Contains("Slot"));
        foreach(var objectsetting in dragDropManager.AllObjects)
        {
            foreach(var panel in panelInventorySetting)
            {
                if(panel.ObjectId == objectsetting.Id)
                {
                    objectsetting.gameObject.SetActive(isOpenMenu);
                }
            }
        }
        if (isOpenMenu)
        {
            animateImageShow.Func_PlayUIAnim();
        }
        else animateImageShow.Func_StopUIAnim();
    }

}