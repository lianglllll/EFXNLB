using UnityEngine;
using System.Collections;
using BaseSystem.PoolModule;
using BaseSystem.MyDelayedTaskScheduler;

public class ImpactScript : MonoBehaviour {

	[Header("Impact Despawn Timer")]
	//How long before the impact is destroyed
	public float despawnTimer = 10.0f;

	[Header("Audio")]
	public AudioClip[] impactSounds;
	public AudioSource audioSource;

	private string sId;

	public void Init(string id)
    {
		sId = id;
    }

    private void OnEnable()
    {
		// Start the despawn timer
		DelayedTaskScheduler.Instance.AddDelayedTask(despawnTimer, () => {
			DespawnTimer();
		});

		//Get a random impact sound from the array
		audioSource.clip = impactSounds[Random.Range(0, impactSounds.Length)];
		//Play the random impact sound
		audioSource.Play();
	}


	private void DespawnTimer() {
		//Wait for set amount of time
		//Destroy the impact gameobject
		//Destroy (gameObject);
		UnityObjectPoolFactory.Instance.RecycleItem(sId, gameObject);
		gameObject.SetActive(false);
	}
}