using UnityEngine;
using System.Collections;

public class TargetHealth : MonoBehaviour {

	public int startingHealth = 10;
	public int currentHealth;

	bool isDead;

	void Awake()
	{
		isDead = false;
		currentHealth = startingHealth;
	}

	void Update()
	{
		if (isDead) {
			gameObject.SetActive (false);
			//Destroy (gameObject, 0);
		}
	}

	public void TakeDamage(int amount)
	{
		if (isDead)
			return;

		currentHealth -= amount;

		if (currentHealth <= 0)
			isDead = true;
	}
}
