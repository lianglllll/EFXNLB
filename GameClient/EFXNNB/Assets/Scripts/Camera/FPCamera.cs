using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��һ�˳������
/// </summary>
public class FPCamera : MonoBehaviour
{
    private Transform Assult_Rife_Arm;
    private CharacterController characterController;

    public float mouseSensitivity = 300f;   //��ת������
    private float yRotation = 0f;           //�����������ת
    
    private float curHight;                 //��Ը������cur�߶�
    public float interpolationSpeed = 12f; //�߶ȱ任��ƽ��ֵ
    private bool isHightChangeing;


    private void Awake()
    {
        Assult_Rife_Arm = transform.Find("Assult_Rife_Arm").GetComponent<Transform>();
        characterController = transform.GetComponent<CharacterController>();
    }
    private void Start()
    {
        //�������
        Cursor.lockState = CursorLockMode.Locked;
        Kaiyun.Event.RegisterIn("StandToCrouch", this, "BeginHightChange");
        Kaiyun.Event.RegisterIn("CrouchToStand", this, "BeginHightChange");
        isHightChangeing = true;
    }

    private void OnDestroy()
    {
        Kaiyun.Event.UnregisterIn("StandToCrouch", this, "BeginHightChange");
        Kaiyun.Event.UnregisterIn("CrouchToStand", this, "BeginHightChange");

    }

    private void Update()
    {
        MouseLook();
        if (isHightChangeing)
        {
            HightChange();
        }
    }

    private void MouseLook()
    {
        float mouseX = GameInputManager.Instance.CameraLook.x;
        float mouseY = GameInputManager.Instance.CameraLook.y;
        if(mouseX != 0f || mouseY != 0f)
        {
            mouseX = mouseX * mouseSensitivity * Time.deltaTime; 
            mouseY = mouseY * mouseSensitivity * Time.deltaTime;
            yRotation -= mouseY;
            yRotation = Mathf.Clamp(yRotation, -60f, 60f);

            //����
            Assult_Rife_Arm.localRotation = Quaternion.Euler(yRotation,0f,0f);
            //��ת��
            transform.Rotate(Vector3.up * mouseX);

        }


    }

    public void BeginHightChange()
    {
        isHightChangeing = true;
    }

    private void HightChange()
    {
        float heightTarget = characterController.height * 0.9f;
        curHight = Mathf.Lerp(curHight,  heightTarget,interpolationSpeed*Time.deltaTime);
        Assult_Rife_Arm.localPosition = Vector3.up * curHight;
        if(Mathf.Abs(heightTarget - curHight) < 0.01f)
        {
            isHightChangeing = false;
        }
    }

}
