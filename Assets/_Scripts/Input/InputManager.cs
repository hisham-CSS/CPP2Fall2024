using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : Singleton<InputManager>
{
    ProjectActions inputs;
    public PlayerController controller;

    protected override void Awake()
    {
        base.Awake();
        inputs = new ProjectActions();
    }

    private void OnEnable()
    {
        inputs.Enable();
        inputs.Overworld.Move.performed += controller.OnMove;
        inputs.Overworld.Move.canceled += controller.MoveCancelled;
        inputs.Overworld.Drop.started += controller.DropWeapon;
        inputs.Overworld.Attack.started += controller.Attack;
        inputs.Overworld.Jump.started += controller.Jump;
        inputs.Overworld.Jump.canceled += controller.Jump;
    }

    private void OnDisable()
    {
        inputs.Disable();
        inputs.Overworld.Move.performed -= controller.OnMove;
        inputs.Overworld.Move.canceled -= controller.MoveCancelled;
        inputs.Overworld.Drop.started -= controller.DropWeapon;
        inputs.Overworld.Attack.started -= controller.Attack;
        inputs.Overworld.Jump.started -= controller.Jump;
        inputs.Overworld.Jump.canceled -= controller.Jump;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
