using BaseSystem.MyDelayedTaskScheduler;
using BaseSystem.PoolModule;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GunSoundClips
{
    public AudioClip shootSound;
    public AudioClip silencerShootSound;
    public AudioClip reloadSound;
    public AudioClip reloadOutOfAmmoSound;
    public AudioClip aimSound;
    public AudioClip shootEmpty;
    public AudioClip turnGunSound;
}

public enum ShootMode
{
    FullyAutomatic,SemiAutomatic,BurstFire
}


public class Weapon_AutomaticGun : Weapon
{
    [Header("ǹе����")]
    private float fireRange = 300f;
    private float fireRate = 0.12f;
    private float curSpreadFactor;                 //ɢ������
    private float aimSpreadFactor = 0f;
    private float noAimSpreadFactor = 0.01f;                 
    private float bulletForce = 200f;
    private ShootMode shootMode;
    private bool canFire;
    public bool isSilencer;

    private int curBulletNum;
    private int oneBulletCapacity = 31444;
    private int reserveBulletNum = 0;
    private float reloadTime = 1.4f;                       //������ʱ��
    private float reloadOutAmmoTime = 2.27f;
    private bool isReloading;

    private float initCrossExpandDegree = 0f;               //��ʼ׼�ǿ��϶�
    private float maxCrossExpandDegree = 50f;               //���׼�ǿ��϶�

    private float trunGunDuration = 1.4f;                   //��ǹ��ʱ��


    [Header("���������Ч && �ӵ����ӵ���")]
    private Light muzzleflashLight;
    private float muzzleLightDuration = 0.02f; 

    private ParticleSystem muzzleParticles;
    private ParticleSystem muzzleSparkParticles;
    private int minMuzzleSparkEmission = 1;
    private int maxMuzzleSparkEmission = 7;

    private string bulletPrefab;
    private string casingPrefab;
    private Transform BulletGeneratePoint;
    private Transform casingBulletSpawnPoint;

    [Header("����")]
    private AudioSource mainAudioSource;
    public GunSoundClips soundClips;
    private float initAudioVolume = 0.5f;

    [Header("UI")]
    private CombatPanel combatPanel;
    private Camera mainCamera;
    private Camera gunCamera;

    [Header("��׼")]
    private bool isAiming;
    private bool isAimIn;
    private bool isAimOut;
    private float aimInDuration = 0.3f; // ����ʱ��


    [Header("�����м����")]
    private RaycastHit hit;
    private Animator animator;


    private void Awake()
    {
        BulletGeneratePoint = transform.Find("Armature/weapon/Components/BulletGeneratePoint").transform;
        casingBulletSpawnPoint = transform.Find("Armature/weapon/Components/casingBulletSpawnPoint").transform;

        muzzleflashLight = transform.Find("Armature/weapon/Components/Muzzleflash Light").GetComponent<Light>();
        muzzleParticles = transform.Find("Armature/weapon/Components/Muzzleflash Particles").GetComponent<ParticleSystem>();
        muzzleSparkParticles = transform.Find("Armature/weapon/Components/SparkParticles").GetComponent<ParticleSystem>();

        bulletPrefab = "Weapons/Prefabs/BulletTail/Bullet_Prefab";
        casingPrefab = "Weapons/Prefabs/Casing_Prefabs/Big_Casing_Prefab";

        mainAudioSource = transform.GetComponent<AudioSource>();

        combatPanel = transform.Find("Canvas/CombatPanel").GetComponent<CombatPanel>();

        animator = GetComponent<Animator>();

        mainCamera = Camera.main;
        gunCamera = transform.Find("Armature/camera/Gun_Camera").GetComponent<Camera>();

    }

    private void Start()
    {
        muzzleflashLight.enabled = false;

        curBulletNum = oneBulletCapacity;
        reserveBulletNum = oneBulletCapacity * 5;

        isReloading = false;
        combatPanel.AmmoTextUIUpdate(curBulletNum, reserveBulletNum);
        combatPanel.ShootModeTextUIUpdate(ShootModeToString(shootMode));

        shootMode = ShootMode.FullyAutomatic;

        canFire = false;


        Kaiyun.Event.RegisterIn("moveStateChange", this, "moveStateChange");

    }

    string task1;
    string task2;
    public override void Init()
    {
        canFire = false;
        task1 = DelayedTaskScheduler.Instance.AddDelayedTask(trunGunDuration * 0.2f, () => {
            mainAudioSource.volume = initAudioVolume;
            mainAudioSource.clip = soundClips.turnGunSound;
            mainAudioSource.time = 0;
            mainAudioSource.Play();
        });
        task2 = DelayedTaskScheduler.Instance.AddDelayedTask(trunGunDuration, () => {
            canFire = true;
        });
        combatPanel.AmmoTextUIUpdate(curBulletNum, reserveBulletNum);
        combatPanel.ShootModeTextUIUpdate(ShootModeToString(shootMode));

        isAiming = false;
        isAimIn = false;
        isAimOut = false;
    }

    public override void Close()
    {
        mainAudioSource.volume = 0f;
        mainAudioSource.Stop();
        DelayedTaskScheduler.Instance.RemoveDelayedTask(task1);
        DelayedTaskScheduler.Instance.RemoveDelayedTask(task2);
    }


    private void OnDestroy()
    {
        Kaiyun.Event.UnregisterIn("moveStateChange", this, "moveStateChange");
    }

    private void Update()
    {
        if (GameInputManager.Instance.ChangeShootMode)
        {
            ShootModeChange();
        }

        if (GameInputManager.Instance.InspectWeapon)
        {
            InspectWeapon();
        }

        if (GameInputManager.Instance.Reload)
        {
            ReloadBegin();
        }

        if (GameInputManager.Instance.RAttackSustain && !isReloading)
        {
            animator.SetBool("isAim", true);
        }
        else
        {
            animator.SetBool("isAim", false);
        }


        if (shootMode== ShootMode.FullyAutomatic && (GameInputManager.Instance.LAttack || GameInputManager.Instance.LAttackSustain))
        {
            GunFire();
        }else if (shootMode == ShootMode.SemiAutomatic && GameInputManager.Instance.LAttack)
        {
            GunFire();
        }
    }

    public override void GunFire()
    {
        if (!canFire || isReloading || isAimIn || isAimOut)
        {
            return;
        }
        if(curBulletNum <= 0)
        {
            canFire = false;
            mainAudioSource.clip = soundClips.shootEmpty;
            mainAudioSource.Play();
            DelayedTaskScheduler.Instance.AddDelayedTask(fireRate, () => {
                canFire = true;
            });
            return;
        }


        canFire = false;

        //��������
        BeginMuzzleFlashLight();
        muzzleParticles.Emit(1);    //����һ��ǹ�ڻ������ӡ�
        muzzleSparkParticles.Emit(Random.Range(minMuzzleSparkEmission,maxMuzzleSparkEmission));

        //����
        mainAudioSource.clip = isSilencer ? soundClips.silencerShootSound : soundClips.shootSound;
        mainAudioSource.Play();

        //����
        if (!isAiming)
        {
            animator.CrossFadeInFixedTime("fire", 0.1f);
            curSpreadFactor = noAimSpreadFactor;
        }
        else
        {
            animator.Play("aim_fire", 0, 0);
            curSpreadFactor = aimSpreadFactor;
        }


        // ��ƫ�ƺ����Ļ�㷢������
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        float offsetX = Random.Range(-curSpreadFactor, curSpreadFactor) * Screen.width;
        float offsetY = Random.Range(-curSpreadFactor, curSpreadFactor) * Screen.height;
        Vector3 offsetScreenPoint = new Vector3(screenCenter.x + offsetX, screenCenter.y + offsetY, screenCenter.z);
        Ray ray = mainCamera.ScreenPointToRay(offsetScreenPoint);
        Vector3 bulletDir;
        if (Physics.Raycast(ray, out hit))
        {
            // ���߻���Ŀ��
            bulletDir = (hit.point - BulletGeneratePoint.position).normalized;
        }
        else
        {
            // ����δ����Ŀ�꣬��ʹ�����߷����Զ����ΪĿ��
            Vector3 targetPosition = ray.GetPoint(fireRange); // �����߷����ϻ�ȡһ��Զ��
            bulletDir = (targetPosition - BulletGeneratePoint.position).normalized;
        }

        // �����ӵ��������䷽����ٶ�
        Transform bullet = UnityObjectPoolFactory.Instance.GetItem<GameObject>(bulletPrefab).transform;
        //bullet.SetParent(null);
        bullet.position = BulletGeneratePoint.position;
        bullet.rotation = Quaternion.LookRotation(bulletDir);
        bullet.GetComponent<Rigidbody>().velocity = bulletDir * bulletForce;
        bullet.gameObject.SetActive(true);
        bullet.GetComponent<BulletScript>().Init(bulletPrefab);

        //�ӵ��׿�
        Transform casing = UnityObjectPoolFactory.Instance.GetItem<GameObject>(casingPrefab).transform;
        //casing.SetParent(null);
        casing.position = casingBulletSpawnPoint.position;
        casing.rotation = casingBulletSpawnPoint.rotation;
        casing.gameObject.SetActive(true);
        casing.GetComponent<BulletCasing>().Init(casingPrefab);


        //info����
        DelayedTaskScheduler.Instance.AddDelayedTask(fireRate, () => {
            canFire = true;
        });
        --curBulletNum;
        if(curBulletNum == 0)
        {
            ReloadBegin();
        }

        //ui
        combatPanel.ShootExpandCross(initCrossExpandDegree);
        combatPanel.AmmoTextUIUpdate(curBulletNum, reserveBulletNum);
    }

    ///�����¼�����
    public void BeginAimIn()
    {
        isAiming = true;
        //��Ұ����
        //mainCamera.fieldOfView = 30f;
        DOTween.To(() => mainCamera.fieldOfView, x => mainCamera.fieldOfView = x, 30f, aimInDuration);
        isAimIn = true;
    }
    public override void AimIn()
    {
        //����׼��
        combatPanel.setCross(false);
        //��׼����
        PlaySound(soundClips.aimSound);
        isAimIn = false;
    }

    public void BeginAimOut()
    {
        combatPanel.setCross(true);
        //��Ұ��Զ
        //mainCamera.fieldOfView = 60f;
        DOTween.To(() => mainCamera.fieldOfView, x => mainCamera.fieldOfView = x, 60f, aimInDuration);

        //��׼����
        PlaySound(soundClips.aimSound);
        isAimOut = true;
    }
    public override void AimOut()
    {
        isAimOut = false ;
        isAiming = false;
    }

    public override void ReloadBegin()
    {
        if (curBulletNum >= oneBulletCapacity || reserveBulletNum <= 0) return;
        if (isReloading) return;
        if (isAiming || isAimIn || isAimOut) return;

        isReloading = true;

        if (curBulletNum == 0)
        {
            animator.Play("reload_out_of_ammo",0,0);
            mainAudioSource.clip = soundClips.reloadOutOfAmmoSound;
            DelayedTaskScheduler.Instance.AddDelayedTask(reloadOutAmmoTime, () => {
                Reload();
                isReloading = false;
            });
        }
        else
        {
            animator.Play("reload_ammo_left",0,0);
            mainAudioSource.clip = soundClips.reloadSound;
            DelayedTaskScheduler.Instance.AddDelayedTask(reloadTime, () => {
                Reload();
                isReloading = false;
            });
        }
        mainAudioSource.Play();
    }
    public override void Reload()
    {
        int needBulletCount = oneBulletCapacity - curBulletNum;
        if(reserveBulletNum >= needBulletCount)
        {
            reserveBulletNum -= needBulletCount;
            curBulletNum = oneBulletCapacity;
        }
        else
        {
            curBulletNum += reserveBulletNum;
            reserveBulletNum = 0;
        }
        combatPanel.AmmoTextUIUpdate(curBulletNum,reserveBulletNum);
        isReloading = false;
    }

    private void ShootModeChange()
    {
        if(shootMode == ShootMode.FullyAutomatic)
        {
            shootMode = ShootMode.SemiAutomatic;
        }
        else
        {
            shootMode = ShootMode.FullyAutomatic;
        }

        mainAudioSource.clip = soundClips.shootEmpty;
        mainAudioSource.Play();

        combatPanel.ShootModeTextUIUpdate(ShootModeToString(shootMode));
    }
    private string ShootModeToString(ShootMode mode)
    {
        if(mode == ShootMode.FullyAutomatic)
        {
            return "ȫ�Զ�";
        }else if(mode == ShootMode.SemiAutomatic)
        {
            return "���Զ�";
        }else if(mode == ShootMode.BurstFire)
        {
            return "����";
        }
        else
        {
            return "xxx";
        }
    }

    private void InspectWeapon()
    {
        animator.SetTrigger("isInspect");
    }

    /// <summary>
    /// ׼�ǿ��϶ȸı�
    /// </summary>
    /// <param name="expanDegree"></param>
    public override void ExpaningCrossUpdate(float tragetDegree)
    {
        combatPanel.BeginExpandCross(tragetDegree);
    }
    public void moveStateChange(MoveState state)
    {
        if (state == MoveState.Run)
        {
            ExpaningCrossUpdate(2 * maxCrossExpandDegree);
        }
        else if (state == MoveState.Walk)
        {
            ExpaningCrossUpdate(maxCrossExpandDegree);
        }
        else
        {
            ExpaningCrossUpdate(initCrossExpandDegree);
        }
    }

    private void BeginMuzzleFlashLight()
    {
        muzzleflashLight.enabled = true;
        DelayedTaskScheduler.Instance.AddDelayedTask(muzzleLightDuration, () => {
            muzzleflashLight.enabled = false;
        });
    }

    private void PlaySound(AudioClip clip)
    {
        mainAudioSource.clip = clip;
        mainAudioSource.Play();
    }


}
