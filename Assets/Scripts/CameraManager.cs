using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Tilemaps;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform playerTransform;
    public Bounds cameraBounds;
    float orthographicSize ;
    float aspectRatio;
    float horizontalSize ;
    public Tilemap tilemap;
    private Bounds bounds;
    private void Awake()
    {
        
    }
    void Start()
    {
        bounds = tilemap.localBounds;
        orthographicSize = GetComponent<Camera>().orthographicSize;
        aspectRatio = Screen.width / Screen.height;
        horizontalSize = orthographicSize * aspectRatio;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }
    void HandleMovement()
    {
        Vector2 targetVector = playerTransform.position - transform.position; 
        
        this.transform.Translate(targetVector);
        this.transform.LookAt(playerTransform);
    }
}
