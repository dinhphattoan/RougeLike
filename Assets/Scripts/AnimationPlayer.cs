using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    public InputManager inputManager;
    public Animator animator;
    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();
        animator = FindObjectOfType<Animator>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AnimationSnapping();
    }
    void AnimationSnapping()
    {
        Vector2 snappedVector2=inputManager.movementInput;
        if(inputManager.movementInput != Vector2.zero)
        {
            snappedVector2=Vector2.one;
        }
        animator.SetFloat("Horizontal", snappedVector2.x, 0.1f, Time.deltaTime);
        animator.SetFloat("Vertical", snappedVector2.y, 0.1f, Time.deltaTime);
        animator.SetBool("Dodge", inputManager.jumpInput);
    }
}
