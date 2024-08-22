using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动控制器
/// </summary>
public class PlayerMovementController : MonoBehaviour
{

    private CharacterController characterController;

    [Header("玩家数值")]
    [Tooltip("当前速度")] private float curSpeed;
    [Tooltip("行走速度")] private float walkSpeed = 4f;
    [Tooltip("奔跑速度")] private float runSpeed = 6f;
    [Tooltip("下蹲速度")] private float crouchSpeed = 2f;

    [Tooltip("行动方向")] private Vector3 motionDir;




    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {

    }

    private void OnDestroy()
    {
    }

    void Update()
    {
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

            //根据条件设置当前speed
            curSpeed = walkSpeed;

            // 移动角色
            characterController.Move(motionDir * curSpeed * Time.deltaTime);

        }

    }



}
