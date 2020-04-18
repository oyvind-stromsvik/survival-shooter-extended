using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour {

	// The amount of health the player starts the game with.
	public int startingHealth = 100;  
	// The current health the player has.
	public int currentHealth;
	// The time in seconds after we last took damage before we can be damaged again.
	public float invulnerabilityTime = 1f;
	// The time in seconds before the background healthbar goes down after we last took damage.
	public float timeAfterWeLastTookDamage = 1f;  
	// Reference to the UI's green health bar.
	public Slider healthSliderForeground;   
	// Reference to the UI's red health bar.
	public Slider healthSliderBackground;   
	// Reference to an image to flash on the screen on being hurt.
	public Image damageImage;      
	// The audio clip to play when the player dies.
	public AudioClip deathClip;         
	// The speed the damageImage will fade at.
	public float flashSpeed = 5f;     
	// The colour the damageImage is set to, to flash.
	public Color flashColour = new Color(1f, 0f, 0f, 0.1f);     

	// Reference to the Animator component.
	Animator anim;            
	// Reference to the AudioSource component.
	AudioSource playerAudio;      
	// Reference to the player's movement.
	PlayerMovement playerMovement; 
	// Reference to the PlayerShooting script.
	PlayerShooting playerShooting;  
	// Whether the player is dead.
	bool isDead;            
	// True when the player gets damaged.
	bool damaged;          
	// The damage accumulated for the current time frame.
	float timer;
	SkinnedMeshRenderer myRenderer;
    // The rim color for our shader. We change this to simulate a red hit effect.
    Color rimColor;
    // Changing the rim power as well produces a better effect.
    float rimPower;

    void Awake() {
		// Setting up the references.
		anim = GetComponent<Animator>();
		playerAudio = GetComponent<AudioSource>();
		playerMovement = GetComponent<PlayerMovement>();
		playerShooting = GetComponentInChildren<PlayerShooting>();
		
		// Set the initial health of the player.
		currentHealth = startingHealth;

		// Get the Player Skinned Mesh Renderer.
		SkinnedMeshRenderer[] meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer meshRenderer in meshRenderers) {
			if (meshRenderer.gameObject.name == "Player") {
				myRenderer = meshRenderer;
				break;
			}
		}
	}

	void Start() {
        // Get the current rim color and rim power from our material.
        rimColor = myRenderer.materials[0].GetColor("_RimColor");
        rimPower = myRenderer.materials[0].GetFloat("_RimPower");
    }
	
	void Update() {
		// If the player has just been damaged...
		if (damaged) {
			// ... set the colour of the damageImage to the flash colour.
			damageImage.color = flashColour;
		}
		// Otherwise...
		else {
			// ... transition the colour back to clear.
			damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
		}

		// Add the time since Update was last called to the timer.
		timer += Time.deltaTime;
		
		// If the timer exceeds the time between attacks, the player is in range and this enemy is alive attack.
		if (timer >= timeAfterWeLastTookDamage) {
			healthSliderBackground.value = Mathf.Lerp(healthSliderBackground.value, healthSliderForeground.value, 2 * Time.deltaTime);
		}

		// Reset the damaged flag.
		damaged = false;
	}
	
	
	public void TakeDamage(int amount) {
		if (timer < invulnerabilityTime) {
			return;
		}

        StopCoroutine("Ishit");
        StartCoroutine("Ishit");

		// Set the damaged flag so the screen will flash.
		damaged = true;
		
		// Reduce the current health by the damage amount.
		currentHealth -= amount;

		if (currentHealth > startingHealth) {
			currentHealth = startingHealth;
		}
		
		// Set the health bar's value to the current health.
		healthSliderForeground.value = currentHealth;

		// Accumulate damage.
		timer = 0;
		
		// Play the hurt sound effect.
		playerAudio.Play ();
		
		// If the player has lost all it's health and the death flag hasn't been set yet...
		if (currentHealth <= 0 && !isDead) {
			// ... it should die.
			Death();
		}
	}

    IEnumerator Ishit() {
        Color newColor = new Color(10, 0, 0, 0);
        float newPower = 0.5f;

        myRenderer.materials[0].SetColor("_RimColor", newColor);
        myRenderer.materials[0].SetFloat("_RimPower", newPower);

        float time = 0.25f;
        float elapsedTime = 0;
        while (elapsedTime < time) {
            newColor = Color.Lerp(newColor, rimColor, elapsedTime / time);
            myRenderer.materials[0].SetColor("_RimColor", newColor);
            newPower = Mathf.Lerp(newPower, rimPower, elapsedTime / time);
            myRenderer.materials[0].SetFloat("_RimPower", newPower);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        myRenderer.materials[0].SetColor("_RimColor", rimColor);
        myRenderer.materials[0].SetFloat("_RimPower", rimPower);
    }

    public void AddHealth(int amount) {
		currentHealth += amount;
		
		if (currentHealth > startingHealth) {
			currentHealth = startingHealth;
		}
		
		// Set the health bar's value to the current health.
		healthSliderForeground.value = currentHealth;
	}
	
	
	void Death() {
		// Set the death flag so this function won't be called again.
		isDead = true;
		
		// Turn off any remaining shooting effects.
		playerShooting.DisableEffects ();
		
		// Tell the animator that the player is dead.
		anim.SetTrigger ("Die");
		
		// Set the audiosource to play the death clip and play it (this will stop the hurt sound from playing).
		playerAudio.clip = deathClip;
		playerAudio.Play ();
		
		// Turn off the movement and shooting scripts.
		playerMovement.enabled = false;
		playerShooting.enabled = false;
	}	
}
