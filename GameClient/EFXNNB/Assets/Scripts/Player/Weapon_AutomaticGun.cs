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
}

public class Weapon_AutomaticGun : Weapon
{
    [Header("ǹе�������")]
    private Transform ShootRayPoint;
    private Transform BulletGeneratePoint;
    private Transform casingBulletSpawnPoint;

    [Header("ǹе����")]
    private float fireRange = 300f;
    private float fireRate = 0.15f;
    private float fireTimer;
    private float originFireRate;
    private float SpreadFactor;
    private float bulletForce = 100f;
    private int oneBulletCapacity = 3100;
    private int curBulletNum;
    private int reserveBulletNum;

    [Header("������Ч")]
    private Light muzzleflashLight;
    private float muzzleLightDuration = 0.02f;
    private ParticleSystem muzzleParticles;
    private ParticleSystem muzzleSparkParticles;
    private int minMuzzleSparkEmission = 1;
    private int maxMuzzleSparkEmission = 7;

    [Header("����")]
    private AudioSource audioSource;
    private GunSoundClips soundClips;

    [Header("UI")]
    private CombatPanel combatPanel;
    private float initCrossExpandDegree = 0f;            //��ʼ׼�ǿ��϶�
    private float maxCrossExpandDegree = 50f;            //���׼�ǿ��϶�


    [Header("�����м����")]
    private RaycastHit hit;


    private void Awake()
    {
        ShootRayPoint = transform.Find("Armature/weapon/Components/ShootRayPoint").transform;
        BulletGeneratePoint = transform.Find("Armature/weapon/Components/BulletGeneratePoint").transform;
        casingBulletSpawnPoint = transform.Find("Armature/weapon/Components/casingBulletSpawnPoint").transform;
        muzzleflashLight = transform.Find("Armature/weapon/Components/Muzzleflash Light").GetComponent<Light>();
        muzzleParticles = transform.Find("Armature/weapon/Components/Muzzleflash Particles").GetComponent<ParticleSystem>();
        muzzleSparkParticles = transform.Find("Armature/weapon/Components/SparkParticles").GetComponent<ParticleSystem>();
        audioSource = transform.GetComponent<AudioSource>();
        combatPanel = transform.Find("Canvas/CombatPanel").GetComponent<CombatPanel>();

    }

    private void Start()
    {
        soundClips = new GunSoundClips();
        soundClips.shootSound = Resources.Load<AudioClip>("Player/Sounds/Gun/Shoot/shoot");
        soundClips.silencerShootSound = Resources.Load<AudioClip>("Player/Sounds/Gun/Shoot/shoot_silencer");
        soundClips.reloadSound = Resources.Load<AudioClip>("Player/Sounds/Gun/Gun_Reloads/Assault_Rifle_01_Reloads/assault_rifle_01_reload_ammo_left");
        soundClips.reloadOutOfAmmoSound = Resources.Load<AudioClip>("Player/Sounds/Gun/Gun_Reloads/Assault_Rifle_01_Reloads/assault_rifle_01_reload_out_of_ammo");
        soundClips.aimSound = Resources.Load<AudioClip>("Player/Sounds/Gun/Aiming/aim_in");
        soundClips.shootEmpty = Resources.Load<AudioClip>("Player/Sounds/Gun/Shoot/shootEmpty");

        fireTimer = 0f;
        curBulletNum = oneBulletCapacity;
        reserveBulletNum = oneBulletCapacity * 5;
        muzzleflashLight.enabled = false;

        Kaiyun.Event.RegisterIn("moveStateChange", this, "moveStateChange");

    }

    private void OnDestroy()
    {
        Kaiyun.Event.UnregisterIn("moveStateChange", this, "moveStateChange");

    }

    private void Update()
    {
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (GameInputManager.Instance.LAttack || GameInputManager.Instance.LAttackSustain)
        {
            GunFire();
        }
    }

    public override void GunFire()
    {
        if (fireTimer < fireRate || curBulletNum <= 0)
        {
            return;
        }

        //��������
        BeginMuzzleFlashLight();
        muzzleParticles.Emit(1);    //����һ��ǹ�ڻ������ӡ�
        muzzleSparkParticles.Emit(Random.Range(minMuzzleSparkEmission,maxMuzzleSparkEmission));

        //����
        audioSource.clip = soundClips.shootSound;
        audioSource.Play();



        //�����ƫ��
        Vector3 shootDir = ShootRayPoint.forward;
        shootDir = shootDir + ShootRayPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor), 0));
        if (Physics.Raycast(ShootRayPoint.position,shootDir,out hit))
        {

        }

        //info����
        --curBulletNum;
        fireTimer = 0f;

        //ui
        combatPanel.ShootExpandCross(initCrossExpandDegree);
        combatPanel.AmmoTextUIUpdate(curBulletNum, reserveBulletNum);
    }

    public override void Reload()
    {
        throw new System.NotImplementedException();
    }

    public override void AimIn()
    {
        throw new System.NotImplementedException();
    }

    public override void AimOut()
    {
        throw new System.NotImplementedException();
    }

    public override void DoReloadAnimation()
    {
        throw new System.NotImplementedException();
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
        if(state == MoveState.Run)
        {
            ExpaningCrossUpdate(2 * maxCrossExpandDegree);
        }else if(state == MoveState.Walk)
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



}
