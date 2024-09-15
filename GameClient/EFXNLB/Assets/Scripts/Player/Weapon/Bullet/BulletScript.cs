using UnityEngine;
using System.Collections;
using BaseSystem.PoolModule;
using BaseSystem.MyDelayedTaskScheduler;

public class BulletScript : MonoBehaviour {

	[Range(5, 100)]
	[Tooltip("After how long time should the bullet prefab be destroyed?")]
	public float destroyAfter;
	[Tooltip("If enabled the bullet destroys on impact")]
	public bool destroyOnImpact = false;
	[Tooltip("Minimum time after impact that the bullet is destroyed")]
	public float minDestroyTime;
	[Tooltip("Maximum time after impact that the bullet is destroyed")]
	public float maxDestroyTime;

	[Header("Impact Effect Prefabs")]
	public Transform [] bloodImpactPrefabs;
	public Transform [] metalImpactPrefabs;
	public Transform [] dirtImpactPrefabs;
	public Transform []	concreteImpactPrefabs;

	private bool isDestroy;

	private void OnEnable()
    {
		isDestroy = false;
		DelayedTaskScheduler.Instance.AddDelayedTask(destroyAfter, () => {
			RecycleItem();
		});
	}

	//If the bullet collides with anything
	private void OnCollisionEnter (Collision collision) 
	{

		//Ignore collision if bullet collides with "Player" tag
		if (collision.gameObject.tag == "Player") 
		{
			//Physics.IgnoreCollision (collision.collider);
			Debug.LogWarning("Collides with player");
			//Physics.IgnoreCollision(GetComponent<Collider>(), GetComponent<Collider>());

			Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());

		}

		//If bullet collides with "Blood" tag
		else if (collision.transform.tag == "Blood") 
		{
			//Instantiate random impact prefab from array
			var obj = UnityObjectPoolFactory.Instance.GetItem<GameObject>("Weapons/Prefabs/Bullet_Impacts/Blood Impact Prefab");
			var script = obj.GetComponent<ImpactScript>();
			script.Init("Weapons/Prefabs/Bullet_Impacts/Blood Impact Prefab");
			obj.SetActive(true);
			obj.transform.position = transform.position;
			obj.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);

			//Destroy bullet object
			RecycleItem();
		}

		//If bullet collides with "Metal" tag
		else if(collision.transform.tag == "Metal") 
		{
			//Instantiate random impact prefab from array
			var obj = UnityObjectPoolFactory.Instance.GetItem<GameObject>("Weapons/Prefabs/Bullet_Impacts/Metal Impact Prefab");
			var script = obj.GetComponent<ImpactScript>();
			script.Init("Weapons/Prefabs/Bullet_Impacts/Metal Impact Prefab");
			obj.SetActive(true);
			obj.transform.position = transform.position;
			obj.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);

			//Destroy bullet object
			RecycleItem();
		}

		//If bullet collides with "Dirt" tag
		else if(collision.transform.tag == "Dirt") 
		{
			//Instantiate random impact prefab from array
			var obj = UnityObjectPoolFactory.Instance.GetItem<GameObject>("Weapons/Prefabs/Bullet_Impacts/Dirt Impact Prefab");
			var script = obj.GetComponent<ImpactScript>();
			script.Init("Weapons/Prefabs/Bullet_Impacts/Dirt Impact Prefab");
			obj.SetActive(true);
			obj.transform.position = transform.position;
			obj.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
			//Destroy bullet object
			RecycleItem();
		}

		//If bullet collides with "Concrete" tag
		else if(collision.transform.tag == "Concrete") 
		{
			//Instantiate random impact prefab from array
			var obj = UnityObjectPoolFactory.Instance.GetItem<GameObject>("Weapons/Prefabs/Bullet_Impacts/Concrete Impact Prefab");
			var script = obj.GetComponent<ImpactScript>();
			script.Init("Weapons/Prefabs/Bullet_Impacts/Concrete Impact Prefab");
			obj.SetActive(true);
			obj.transform.position = transform.position;
			obj.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
			//Destroy bullet object
			RecycleItem();
		}

		//If bullet collides with "Target" tag
		else if(collision.transform.tag == "Target") 
		{
			//Toggle "isHit" on target object
			collision.transform.gameObject.GetComponent
				<TargetScript>().isHit = true;
			//Destroy bullet object
			RecycleItem();
		}

		//If bullet collides with "ExplosiveBarrel" tag
		else if(collision.transform.tag == "ExplosiveBarrel") 
		{
			//Toggle "explode" on explosive barrel object
			collision.transform.gameObject.GetComponent
				<ExplosiveBarrelScript>().explode = true;
			//Destroy bullet object
			RecycleItem();
		}

		//If bullet collides with "GasTank" tag
		else if(collision.transform.tag == "GasTank") 
		{
			//Toggle "isHit" on gas tank object
			collision.transform.gameObject.GetComponent
				<GasTankScript> ().isHit = true;
			//Destroy bullet object
			RecycleItem();
		}


		//If destroy on impact is false, start 
		//coroutine with random destroy timer
		if (!destroyOnImpact)
		{
			DelayedTaskScheduler.Instance.AddDelayedTask(Random.Range(minDestroyTime, maxDestroyTime), () => {
				RecycleItem();
			});
		}
		//Otherwise, destroy bullet on impact
		else
		{
			RecycleItem();
		}

	}

	private void RecycleItem()
    {
		if (isDestroy) return;
		isDestroy = true;
		gameObject.SetActive(false);
		UnityObjectPoolFactory.Instance.RecycleItem("Weapons/Prefabs/BulletTail/Bullet_Prefab", gameObject);
	}

}