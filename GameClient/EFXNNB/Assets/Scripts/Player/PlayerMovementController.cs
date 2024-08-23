using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动控制器
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    enum MotionState
    {
        Idle, Walk, Run, Courch
    };

    [Header("杂七杂八")]
    private CharacterController characterController;
    private Vector3 motionDir;                          //移动的方向

    [Header("move相关")]
    public float curSpeed;
    private float walkSpeed = 4f;
    private float runSpeed = 8f;
    private float crouchSpeed = 2f;
    private MotionState motionState;

    [Header("jump相关")]
    public float curJumpForce;
    public float initJumpForce = 5f;
    public float gravity = -9.8f;
    private CollisionFlags collisionFlags;  //配合cc.move()用于描述碰撞的结果
    public bool isGround;

    [Header("crouch相关")]
    private float crouchHeight = 1f;
    private float standHeight;
    private float crouchCenter = 0.5f;
    private Vector3 standCenter;
    private bool isCrouching;
    private LayerMask crouchLayerMask;



    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        motionState = MotionState.Idle;

        curJumpForce = -2f;
        isGround = false;

        isCrouching = false;
        standHeight = characterController.height;
        standCenter = (standHeight / 2.0f) *Vector3.up;
        crouchLayerMask = LayerMask.GetMask("Ground","Wall", "Tunnel");
    }

    private void OnDestroy()
    {
    }

    void Update()
    {
        Crouch();
        Jump();
        Move();
    }

    private void Move()
    {

        float h = GameInputManager.Instance.Movement.x;
        float v = GameInputManager.Instance.Movement.y;
        if (h != 0 || v != 0)
        {
            // 计算移动方向
            motionDir = (transform.right * h + transform.forward * v).normalized;

            //根据条件设置当前motionState
            if (isCrouching)
            {
                curSpeed = crouchSpeed;
            }
            else if (GameInputManager.Instance.Run)
            {
                motionState = MotionState.Run;
                curSpeed = runSpeed;
            }else {
                motionState = MotionState.Walk;
                curSpeed = walkSpeed;
            }

            // 移动角色
            collisionFlags = characterController.Move(motionDir * curSpeed * Time.deltaTime);

        }

    }

    private void Jump()
    {
        if (motionState == MotionState.Courch) return;

        if (GameInputManager.Instance.Jump && isGround)
        {
            isGround = false;
            curJumpForce = initJumpForce;

        }

        curJumpForce = curJumpForce + gravity * Time.deltaTime;
        Vector3 jumpV3 = new Vector3(0, curJumpForce * Time.deltaTime, 0);
        collisionFlags = characterController.Move(jumpV3);

        if (collisionFlags == CollisionFlags.Below)
        {
            isGround = true;
            curJumpForce = -2f;
        }

    }


    private bool CanCrouchToUp()
    {
        Vector3 headPos = transform.position + Vector3.up * standHeight;
        int count =  (Physics.OverlapSphere(headPos, characterController.radius, crouchLayerMask)).Length;
        return count <= 0;
    }

    private void Crouch()
    {
        if (GameInputManager.Instance.Crouch)
        {
            isCrouching = true;
            motionState = MotionState.Courch;

            characterController.height = crouchHeight;
            characterController.center = new Vector3(0, crouchCenter, 0);

            Kaiyun.Event.FireIn("StandToCrouch");
        }
        else
        {
            if (isCrouching && CanCrouchToUp())
            {
                isCrouching = false;
                motionState = MotionState.Idle;

                characterController.height = standHeight;
                characterController.center = standCenter;

                Kaiyun.Event.FireIn("CrouchToStand");
            }

        }
    }

}
