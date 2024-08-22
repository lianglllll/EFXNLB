using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��һ�˳������
/// </summary>
public class FPCamera : MonoBehaviour
{
    private Transform Assult_Rife_Arm;

    [Tooltip("������")] public float mouseSensitivity = 400f;
    private float yRotation = 0f;//�����������ת

    private void Start()
    {
        //�������
        Cursor.lockState = CursorLockMode.Locked;
        Assult_Rife_Arm = transform.Find("Assult_Rife_Arm").GetComponent<Transform>();
    }

    private void Update()
    {
        MouseLook();
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


}
