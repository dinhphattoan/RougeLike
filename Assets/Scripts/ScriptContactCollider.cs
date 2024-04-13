using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ScriptContactCollider : MonoBehaviour
{
    // public GameObject selectedGameObject;
    // // Start is called before the first frame update
    // void Start()
    // {

    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetMouseButton(0))
    //     {
    //         if (selectedGameObject != null)
    //         {
    //             if (selectedGameObject.tag == "Tree")
    //             {
    //                 var TreeScript = selectedGameObject.GetComponent<TreeScript>();
    //                 TreeScript.GetHit();
    //             }
    //         }
    //     }
    // }
    // List<GameObject> listGameObjectStay = new List<GameObject>();
    // private void OnTriggerStay2D(Collider2D other)
    // {
    //     if (other.gameObject.tag == "Tree" || other.gameObject.tag == "Rock")
    //     {
    //         listGameObjectStay.Add(other.gameObject);
    //         SelectTransform();
    //     }
    // }
    // private void SelectTransform()
    // {
    //     float distance = 0;
    //     selectedGameObject=null;
    //     foreach (var ob in listGameObjectStay)
    //     {
    //         float tempdistance = Vector2.Distance(transform.position, ob.transform.position);
    //         if (distance > tempdistance)
    //         {
    //             distance = tempdistance;
    //             selectedGameObject = ob;
    //         }
    //     }

    // }
    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     if (other.gameObject.tag == "Tree" || other.gameObject.tag == "Rock")
    //     {
    //         if (selectedGameObject == other.gameObject)
    //         {
    //             SelectTransform();
    //         }
    //         listGameObjectStay.Remove(other.gameObject);

    //     }
    // }
}
