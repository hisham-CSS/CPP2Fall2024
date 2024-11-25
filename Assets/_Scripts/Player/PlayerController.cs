using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

[RequireComponent(typeof(CharacterController), typeof(Animator), typeof(Health))]
public class PlayerController : MonoBehaviour
{
    //Controller components   
    CharacterController cc;
    Animator anim;
    Health health;

    //movement and rotation speed
    [Header("Movement Variables")]
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float maxSpeed = 15.0f;
    [SerializeField] private float moveAccel = 0.5f;
    [SerializeField] private float rotationSpeed = 30.0f;
    private float currentSpeed;

    //Weapon system variables
    [Header("Weapon Variables")]
    public Transform weaponAttachPoint;
    Weapon weapon = null;

    //test layer mask for raycast example
    public LayerMask testLayerMask;

    
    
    //character controller variables that are used to store movement input and create our current frames velocity value
    Vector2 direction; //set by keyboard or mouse input
    Vector3 velocity; //set every frame via direction and gravity
    Vector3 prevVel; //stored at the end of our frame for our next frame's calculation

    //we calculate this variable based on jump values that are set (max jump height and max jump time)
    float gravity;
    

    
    [Header("Jump Variables")]
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float jumpTime = 1f; //total time (both upward and downward)
    bool isJumpPressed = false;

    //values are calculated in start based on the jump height and max jump time
    float timeToApex; //max jump time / 2 - this comes from the assumption that our jump is a consistent arc
    float initalJumpVelocity; //calculated based on jump height and the time to the apex point

    #region Setup Functions
    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();

        currentSpeed = speed;

        InitalizeJump();
    }

    void InitalizeJump()
    {
        //Formula's were taken from this GDC talk: https://www.youtube.com/watch?v=hG9SzQxaCm8
        timeToApex = jumpTime / 2; // half way thru the jump we should be at apex point
        gravity = (-2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        initalJumpVelocity = -(gravity * timeToApex);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) Cursor.lockState = CursorLockMode.None; 
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion

    #region Input Functions
    public void OnMove(InputAction.CallbackContext ctx)
    {
        direction = ctx.ReadValue<Vector2>();
        
    }

    public void MoveCancelled(InputAction.CallbackContext ctx)
    {
        direction = Vector2.zero;
        
    }

    public void DropWeapon(InputAction.CallbackContext ctx)
    {
        if (weapon) {
            weapon.Drop(GetComponent<Collider>(), transform.forward);
            weapon = null;
        }    
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Attack")) return;

        if (weapon)
            anim.SetTrigger("Attack");
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        isJumpPressed = ctx.ReadValueAsButton();
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("speed", cc.velocity.magnitude);
            
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Debug.DrawLine(transform.position, transform.position + (transform.forward * 10), Color.red);
        if (Physics.Raycast(ray, out hit, 10.0f, testLayerMask)) {
            Debug.Log(hit.collider.gameObject.name);
        }
    }

    private void FixedUpdate()
    {
        //grab our projected move direction - this includes all necessary calculations to make it our frames velocity
        Vector3 projectedMoveDir = ProjectedMoveDirection();
        velocity.x = projectedMoveDir.x;
        velocity.z = projectedMoveDir.z;

        //our final state for this update
        if (!anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Attack"))
            cc.Move(DesiredMoveDirection());

        //Rotate the character so the player faces the direction of movement
        if (direction.magnitude > 0)
        {
            float timeStep = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(projectedMoveDir), timeStep);
        }
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Weapon") && weapon == null) {
            weapon = hit.gameObject.GetComponent<Weapon>();
            weapon.Equip(GetComponent<Collider>(), weaponAttachPoint);
        }

        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Heal"))
        {
            health.health++;
        }
    }

    #region Player Movement
    /// <summary>
    /// For camera relative movement
    /// </summary>
    /// <returns>Vector 3 - Projected Move Direction for camera relative movement</returns>
    private Vector3 ProjectedMoveDirection()
    {
        //Grab our camera forward and right vectors
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        //remove the yaw rotation
        cameraForward.y = 0;
        cameraRight.y = 0;

        //normalize the camera vectors so that we don't have any unecessary rotation
        //we only care about the direction - not the magnitude
        cameraForward.Normalize();
        cameraRight.Normalize();

        return cameraForward * direction.y + cameraRight * direction.x;
    }

    /// <summary>
    /// The desired move direction taking into account our speed
    /// TODO: Adjust speed with an acceleration
    /// </summary>
    /// <param name="projectedDirection">The result from ProjectedMoveDirection() - This allows us to return what the movement vector for this frame should look like - with camera relative movement</param>
    /// <returns></returns>
    private Vector3 DesiredMoveDirection()
    {
        if (direction == Vector2.zero) currentSpeed = speed;

        //ensure we aren't above our max speed or below our minSpeed
        currentSpeed = Mathf.Clamp(currentSpeed, speed, maxSpeed);
        
        //move along the projected axis via our speed - ignoring the y axis
        velocity = new Vector3(velocity.x * currentSpeed, velocity.y, velocity.z * currentSpeed);
        
        //Add acceleration for next frame
        currentSpeed += moveAccel * Time.fixedDeltaTime;

        //check what our y velocity should be
        if (!cc.isGrounded) velocity.y += gravity * Time.fixedDeltaTime;
        else velocity.y = CheckJump();

        //verlet integration
        Vector3 appliedVel = (prevVel + velocity) * 0.5f;

        //Debug.Log($"Previous Velocity is: {prevVel}");
        //Debug.Log($"Calculated Velocity is: {velocity}");
        //Debug.Log($"Applied Vel is: {appliedVel}");
        Debug.Log($"Controller Velocity is: {cc.velocity.magnitude}");
        //Debug.Log($"Position is: {transform.position}");

        prevVel = appliedVel;
        return appliedVel * Time.fixedDeltaTime;
    }

    private float CheckJump()
    {
        if (isJumpPressed) { Debug.LogError("Is Jumping"); return initalJumpVelocity; }

        //magic number to ensure that the character controller moves down a little every frame.  This ensures the ground check happens correctly.
        return -cc.skinWidth;
    }
    #endregion
}
