using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MoveState
{
    Idle, Walk, Run, Courch
};


/// <summary>
/// 移动控制器
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    [Header("临时用")]
    private CharacterController characterController;
    private Animator animator;
    private Vector3 motionDir;                          //移动的方向

    [Header("move相关")]
    public float curSpeed;
    private float walkSpeed = 4f;
    private float runSpeed = 8f;
    private float crouchSpeed = 2f;
    private MoveState moveState;

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


    [Header("audio相关")]
    public AudioClip walkAudioClip;
    public AudioClip runAudioClip;
    private AudioSource audioSource;



    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = transform.Find("Assult_Rife_Arm/arms_assault_rifle_01").GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        moveState = MoveState.Idle;

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
        PlayerFootSounds();
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
                MoveStateChange(MoveState.Run);

                curSpeed = runSpeed;
            }else {
                MoveStateChange(MoveState.Walk);


                curSpeed = walkSpeed;
            }

            // 移动角色
            collisionFlags = characterController.Move(motionDir * curSpeed * Time.deltaTime);

        }
        else
        {
            MoveStateChange(MoveState.Idle);
        }

    }

    private void MoveStateChange(MoveState state)
    {
        if (moveState == state) return;
        moveState = state;
        //做点状态改变的后处理
        if(moveState == MoveState.Run)
        {
            animator.SetBool("isRun", true);
            animator.SetBool("isWalk", true);
        }else if(moveState == MoveState.Walk )
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isWalk", true);
        }
        else 
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isWalk", false);
        }


        Kaiyun.Event.FireIn("moveStateChange", moveState);
    }


    private void PlayerFootSounds()
    {
        if (isGround && (moveState == MoveState.Walk || moveState == MoveState.Run))
        {
            if(moveState == MoveState.Walk)
            {
                audioSource.clip = walkAudioClip;
            }else
            {
                audioSource.clip = runAudioClip;
            }

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    private void Jump()
    {
        if (moveState == MoveState.Courch) return;

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
            if (!isGround) return;

            isCrouching = true;
            moveState = MoveState.Courch;

            characterController.height = crouchHeight;
            characterController.center = new Vector3(0, crouchCenter, 0);

            Kaiyun.Event.FireIn("StandToCrouch");
        }
        else
        {
            if (isCrouching && CanCrouchToUp())
            {
                isCrouching = false;
                moveState = MoveState.Idle;

                characterController.height = standHeight;
                characterController.center = standCenter;

                Kaiyun.Event.FireIn("CrouchToStand");
            }

        }
    }

}
