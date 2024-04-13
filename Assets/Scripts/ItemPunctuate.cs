using System.Collections;
using UnityEngine;

public class ItemPunctuate : MonoBehaviour
{
    public Vector3 defaultPosition;
    public float moveDistance = 0.5f;
    public float moveSpeed = 0f;

    private Vector3 startPos;
    private Vector3 endPos;
    bool isPunctuating=false;
    // Start is called before the first frame update
    void Start()
    {
        defaultPosition=transform.position;   
    }
    IEnumerator MoveLeftAndRight()
    {
            startPos = transform.position;
            endPos = startPos + Vector3.right * moveDistance;
            yield return StartCoroutine(MoveObject(transform, startPos, endPos, moveSpeed));

            startPos = transform.position;
            endPos = startPos - Vector3.right * moveDistance;
            yield return StartCoroutine(MoveObject(transform, startPos, endPos, moveSpeed));
            transform.position=defaultPosition;
            isPunctuating=false;
    }
    IEnumerator MoveObject(Transform objectToMove, Vector3 startPos, Vector3 endPos, float moveTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < moveTime)
        {
            objectToMove.position = Vector3.Lerp(startPos, endPos, (elapsedTime / moveTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        objectToMove.position = endPos;
    }
    public void Punctuate()
    {
        if(!isPunctuating)
        {
            isPunctuating=true;
            StartCoroutine(MoveLeftAndRight());
        }
        
    }
    // Update is called once per frame
    void Update()
    {

    }

}
