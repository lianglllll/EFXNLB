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
    [Header("枪械属性")]
    private float fireRange = 300f;
    private float fireRate = 0.12f;
    private float curSpreadFactor;                 //散射因子
    private float aimSpreadFactor = 0f;
    private float noAimSpreadFactor = 0.01f;                 
    private float bulletForce = 200f;
    private ShootMode shootMode;
    private bool canFire;
    public bool isSilencer;

    private int curBulletNum;
    private int oneBulletCapacity = 31;
    private int reserveBulletNum;
    private float reloadTime = 1.4f;                       //换弹的时间
    private float reloadOutAmmoTime = 2.27f;
    private bool isReloading;

    private float initCrossExpandDegree = 0f;               //初始准星开合度
    private float maxCrossExpandDegree = 50f;               //最大准星开合度

    private float trunGunDuration = 1.4f;                   //切枪的时间


    [Header("火光粒子特效 && 子弹和子弹壳")]
    private Light muzzleflashLight;
    private float muzzleLightDuration = 0.02f; 

    private ParticleSystem muzzleParticles;
    private ParticleSystem muzzleSparkParticles;
    private int minMuzzleSparkEmission = 1;
    private int maxMuzzleSparkEmission = 7;

    private GameObject bulletPrefab;
    private GameObject casingPrefab;
    private Transform BulletGeneratePoint;
    private Transform casingBulletSpawnPoint;

    [Header("声音")]
    private AudioSource mainAudioSource;
    public GunSoundClips soundClips;

    [Header("UI")]
    private CombatPanel combatPanel;
    private Camera mainCamera;
    private Camera gunCamera;

    [Header("瞄准")]
    private bool isAim;
    private bool AimEnd;
    private Vector3 initRiflePosion;
    public Vector3 aimRiflePosion;


    [Header("其他中间变量")]
    private RaycastHit hit;
    private Animator animator;


    private void Awake()
    {
        BulletGeneratePoint = transform.Find("Armature/weapon/Components/BulletGeneratePoint").transform;
        casingBulletSpawnPoint = transform.Find("Armature/weapon/Components/casingBulletSpawnPoint").transform;

        muzzleflashLight = transform.Find("Armature/weapon/Components/Muzzleflash Light").GetComponent<Light>();
        muzzleParticles = transform.Find("Armature/weapon/Components/Muzzleflash Particles").GetComponent<ParticleSystem>();
        muzzleSparkParticles = transform.Find("Armature/weapon/Components/SparkParticles").GetComponent<ParticleSystem>();

        bulletPrefab = Resources.Load<GameObject>("Player/Prefabs/BulletTail/Bullet_Prefab");
        casingPrefab = Resources.Load<GameObject>("Player/Prefabs/Casing_Prefabs/Small_Casing_Prefab");

        mainAudioSource = transform.GetComponent<AudioSource>();

        combatPanel = transform.Find("Canvas/CombatPanel").GetComponent<CombatPanel>();

        animator = GetComponent<Animator>();

        mainCamera = Camera.main;
        gunCamera = transform.Find("Armature/camera/Gun_Camera").GetComponent<Camera>();

    }

    private void Start()
    {
        muzzleflashLight.enabled = false;

        canFire = false;
        GameTimerManager.Instance.TryUseOneTimer(trunGunDuration*0.2f, () => {
            mainAudioSource.clip = soundClips.turnGunSound;
            mainAudioSource.Play();
        });
        GameTimerManager.Instance.TryUseOneTimer(trunGunDuration, () => {
            canFire = true;
        });
        curBulletNum = oneBulletCapacity;
        reserveBulletNum = oneBulletCapacity * 5;
        isReloading = false;

        shootMode = ShootMode.FullyAutomatic;
        
        combatPanel.AmmoTextUIUpdate(curBulletNum, reserveBulletNum);
        combatPanel.ShootModeTextUIUpdate(ShootModeToString(shootMode));

        isAim = false;
        AimEnd = true;
        initRiflePosion = transform.localPosition;

        Kaiyun.Event.RegisterIn("moveStateChange", this, "moveStateChange");


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
            isAim = true;
            animator.SetBool("isAim", isAim);
            transform.localPosition = aimRiflePosion;

            AimEnd = false;
        }
        else
        {
            isAim = false;
            animator.SetBool("isAim", isAim);
            transform.localPosition = initRiflePosion;
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
        if (!canFire || isReloading)
        {
            return;
        }
        if(curBulletNum <= 0)
        {
            canFire = false;
            mainAudioSource.clip = soundClips.shootEmpty;
            mainAudioSource.Play();
            GameTimerManager.Instance.TryUseOneTimer(fireRate, () => {
                canFire = true;
            });
            return;
        }


        canFire = false;

        //火光和粒子
        BeginMuzzleFlashLight();
        muzzleParticles.Emit(1);    //发射一个枪口火焰粒子。
        muzzleSparkParticles.Emit(Random.Range(minMuzzleSparkEmission,maxMuzzleSparkEmission));

        //声音
        mainAudioSource.clip = isSilencer ? soundClips.silencerShootSound : soundClips.shootSound;
        mainAudioSource.Play();

        //动画
        if (!isAim)
        {
            animator.CrossFadeInFixedTime("fire", 0.1f);
            curSpreadFactor = noAimSpreadFactor;
        }
        else
        {
            animator.Play("aim_fire", 0, 0);
            curSpreadFactor = aimSpreadFactor;
        }


        // 从偏移后的屏幕点发射射线
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        float offsetX = Random.Range(-curSpreadFactor, curSpreadFactor) * Screen.width;
        float offsetY = Random.Range(-curSpreadFactor, curSpreadFactor) * Screen.height;
        Vector3 offsetScreenPoint = new Vector3(screenCenter.x + offsetX, screenCenter.y + offsetY, screenCenter.z);
        Ray ray = mainCamera.ScreenPointToRay(offsetScreenPoint);
        Vector3 bulletDir;
        if (Physics.Raycast(ray, out hit))
        {
            // 射线击中目标
            bulletDir = (hit.point - BulletGeneratePoint.position).normalized;
        }
        else
        {
            // 射线未击中目标，则使用射线方向的远点作为目标
            Vector3 targetPosition = ray.GetPoint(fireRange); // 在射线方向上获取一个远点
            bulletDir = (targetPosition - BulletGeneratePoint.position).normalized;
        }

        // 生成子弹并设置其方向和速度
        Transform bullet = Instantiate(bulletPrefab, BulletGeneratePoint.position, Quaternion.LookRotation(bulletDir)).transform;
        bullet.GetComponent<Rigidbody>().velocity = bulletDir * bulletForce;

        //子弹抛壳
        Instantiate(casingPrefab, casingBulletSpawnPoint.position, casingBulletSpawnPoint.rotation);


        //info更新
        GameTimerManager.Instance.TryUseOneTimer(fireRate, () => {
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

    public override void AimIn()
    {
        //隐藏准星
        combatPanel.setCross(false);

        //瞄准声音
        PlaySound(soundClips.aimSound);

        //视野拉近
        mainCamera.fieldOfView = 30f;

    }
    public override void AimOut()
    {
        combatPanel.setCross(true);
        //视野拉远
        mainCamera.fieldOfView = 60f;
        //瞄准声音
        PlaySound(soundClips.aimSound);

        AimEnd = true;
    }

    public override void ReloadBegin()
    {
        if (curBulletNum >= oneBulletCapacity || reserveBulletNum <= 0) return;
        if (isReloading) return;
        if (!AimEnd) return;

        isReloading = true;

        if (curBulletNum == 0)
        {
            animator.Play("reload_out_of_ammo",0,0);
            mainAudioSource.clip = soundClips.reloadOutOfAmmoSound;
            GameTimerManager.Instance.TryUseOneTimer(reloadOutAmmoTime, () => {
                Reload();
                isReloading = false;
            });
        }
        else
        {
            animator.Play("reload_ammo_left",0,0);
            mainAudioSource.clip = soundClips.reloadSound;
            GameTimerManager.Instance.TryUseOneTimer(reloadTime, () => {
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
            return "全自动";
        }else if(mode == ShootMode.SemiAutomatic)
        {
            return "半自动";
        }else if(mode == ShootMode.BurstFire)
        {
            return "连发";
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
    /// 准星开合度改变
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
        GameTimerManager.Instance.TryUseOneTimer(muzzleLightDuration, () => {
            muzzleflashLight.enabled = false;
        });
    }

    private void PlaySound(AudioClip clip)
    {
        mainAudioSource.clip = clip;
        mainAudioSource.Play();
    }

}
