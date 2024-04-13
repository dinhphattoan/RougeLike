using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasEventTrigger : MonoBehaviour
{   
    public GameObject toolHub;
    PlayerManager playerManager;
    public List<Button> listToolbuttons;

    // Start is called before the first frame update
    void Start()
    {
        var temp  = toolHub.transform;
        foreach(Transform child in temp)
        {
            var Button = child.GetComponent<Button>();
            Button.onClick.AddListener(OnClick);
            listToolbuttons.Add(Button);
        }

        //Initialize variables
        playerManager = FindObjectOfType<PlayerManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnClick()
    {
        GameObject clickedButton= EventSystem.current.currentSelectedGameObject;
        if(clickedButton!=null)
        {
            Button button = clickedButton.GetComponent<Button>();
            if(button!=null)
            {
                playerManager.EquipTool(listToolbuttons.IndexOf(button));
            }
        }
    }
}
