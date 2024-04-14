using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemScript : MonoBehaviour
{
    public int itemId;
    public eItemType itemType;
    public Sprite sprite;
    public int count;
    public bool isTransactioned = false;
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator MoveToPlayer(Transform player)
    {
        Vector3 endVector = player.position - this.transform.position;
        Vector2 velocity = Vector2.zero;
        while (Vector2.Distance(transform.position, player.position) > 0.1f)
        {
            transform.position = Vector2.SmoothDamp(transform.position, player.position, ref velocity, 0.1f);

            yield return null;
        }

        Destroy(this.gameObject);
    }
    public void StartTransaction(Transform player)
    {
        if (!isTransactioned)
        {
            isTransactioned = true;
            int index = player.GetComponent<PlayerManager>().playerInventory.AddItem(new IItemSlot(itemId, itemType, 1));
            StartCoroutine(MoveToPlayer(player));
            if (index != -1)
            {

                if (!DragDropManager.CheckObjectExistById("Ob:" + index))
                {
                    DragDropManager dragDropManager = FindObjectOfType<DragDropManager>();

                    foreach (var panel in dragDropManager.AllPanels)
                    {
                        if (panel.ObjectId == "")
                        {

                            GameObject slot = new GameObject();
                            GameObject textSlot = new GameObject();
                            textSlot.transform.SetParent(slot.transform);
                            RectTransform textRect = textSlot.AddComponent<RectTransform>();
                            textRect.position = new Vector3(0, -15, 0);
                            textRect.sizeDelta = new Vector2(40, 10);
                            TextMeshProUGUI textMeshPro = textSlot.AddComponent<TextMeshProUGUI>();
                            textMeshPro.text = 1.ToString();
                            textMeshPro.fontSize = 10;
                            textMeshPro.fontStyle = FontStyles.Bold;
                            textMeshPro.alignment = TextAlignmentOptions.MidlineRight;
                            slot.transform.SetParent(dragDropManager.FirstCanvas.transform);
                            RectTransform recttrans = slot.AddComponent<RectTransform>();
                            recttrans.sizeDelta = new Vector2(40, 40);
                            var obevent = slot.AddComponent<ObjectEvents>();
                            var objectsetting = slot.AddComponent<ObjectSettings>();
                            obevent.OS = objectsetting;
                            objectsetting.ReplaceSmoothly = true;
                            objectsetting.ReplacementSpeed = 2;
                            objectsetting.ReturnSpeed = 2;

                            objectsetting.ScaleOnDropped = true;
                            objectsetting.DragScale = new Vector3(1f, 1f, 1f);
                            //Mark id as a slot so it need to be unique
                            objectsetting.Id = "Ob:" + index;
                            objectsetting.SwitchObjects = true;
                            objectsetting.ReplaceSmoothly = true;
                            objectsetting.ReplacementSpeed = 2;
                            objectsetting.ReturnSpeed = 2;
                            dragDropManager.AllObjects.Add(objectsetting);
                            objectsetting.ScaleOnDrag = true;
                            objectsetting.DragScale = new Vector3(1f, 1f, 1f);
                            objectsetting.FirstScale = Vector3.one;
                            Image image = slot.AddComponent<Image>();
                            image.sprite = FindObjectOfType<GameManager>().listSpriteRefItemId[itemId];
                            image.color = Color.white;
                            AIDragDrop.DragDrop(objectsetting.Id, panel.Id, true);
                            if(panel.Id.Contains("Slot")){
                                objectsetting.gameObject.SetActive(false);
                            }
                            return;
                        }
                    }
                }

            }

        }

    }
}
