﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shield manager of the Player, it adds recover option. Inherits from ShieldManager.
/// </summary>
public class PlayerShieldManager : ShieldManager {
    [Header("Recover Effect")]
    public Color recoverColor;          // Color to flick when recovering the shield.   
    [HideInInspector]
    public bool inBarrelRoll = false;   // If the Player's ship is performing a barrel roll.

    private GameObject player;          // Player GameObject.

    new void Start()
    {
        base.Start();
        if(PlayerHUDManager.Instance != null)
            currentShield = PlayerHUDManager.Instance.GetPlayerShieldBarWidth(); // For changes between scenes set health by getting playerHUDManager value.
        player = GameManager.Instance.player;
    }

    /// <summary>
    /// Overriden function to take damage. It adds a sound effect, hit animation on the HUD and updates shield bar on the HUD.
    /// </summary>
    /// <param name="amount"> Amount of damage to take.</param>
    public override void TakeDamage(int amount)
    {
        if (!invulnerable)
        {
            AudioManager.Instance.Play("Hurt");
            PlayerHUDManager.Instance.StartCoroutine("HitEffect");
        }
        base.TakeDamage(amount);           
        PlayerHUDManager.Instance.SetPlayerShieldBarWidth(currentShield);
    }

    /// <summary>
    /// Function to recover an amount of shield.
    /// </summary>
    /// <param name="amount"> Amount of shield to recover.</param>
    public void RecoverShield(int amount)
    {
        if (amount < 0)     // Avoid getting values lower than 1.
        {
            Debug.LogError("Negative numbers not allowed, if you want to low current shield you should use TakeDamage() instead");
            return;
        }
        var newShieldAmount = currentShield + amount;
        if (newShieldAmount < maxShield)
            currentShield = newShieldAmount;
        else
            currentShield = maxShield;

        StartCoroutine(FlickeringColor(recoverColor));                      // Play flick animation.
        PlayerHUDManager.Instance.SetPlayerShieldBarWidth(currentShield);   // Update shield bar on the HUD.
    }

    /// <summary>
    /// Overriden function for death.
    /// </summary>
    protected override void Die()
    {
        if (player == null)     // Avoid executing code if player variable is null.
        {
            Debug.LogWarning("Player couldn't be found. Searching again...");
            player = GameManager.Instance.player;    // Set player variable.
            if (player != null)
                Debug.LogWarning("Player found!");
            else
                return;
        }

        base.Die();
        player.SetActive(false);
        LevelManager.Instance.PauseLevel();                     // Pause the level for respawning the player or showing GameOver screen.
        GameManager.Instance.playerInfo.isDead = true;          // Set Player dead.
        AudioManager.Instance.StopSoundEffects();               // Stop any sound effect playing.

        if (GameManager.Instance.playerInfo.lives-1 >= 0)       // See if Player has lives left.
        {
            Invoke("RespawnPlayer", 2.0f);                      // Respawn the player after 2 seconds.
            LevelManager.Instance.Invoke("ContinueLevel", 2.5f);// Set the level back in Play mode after 2.5 seconds.
        }
        else
        {
            // Game Over screen.
            LevelManager.Instance.LevelGameOver();
        }
        GameManager.Instance.SubstractPlayerLives(1);
    } 

    /// <summary>
    /// Function to set Player's shield back to default, also update ir on the HUD.
    /// </summary>
    void ResetPlayerShield()
    {
        currentShield = maxShield;
        PlayerHUDManager.Instance.SetPlayerShieldBarWidth(currentShield);
    }

    /// <summary>
    /// Function to Respawn the Player and set it ready to start play mode.
    /// </summary>
    private void RespawnPlayer()
    {
        if (player == null)         // Avoid executing code if player variable is null.
        {
            Debug.LogWarning("Player couldn't be found. Searching again...");
            player = GameManager.Instance.player;    // Set player variable.
            if (player != null)
                Debug.LogWarning("Player found!");
            else
                return;
        }

        if (!GameManager.Instance.playerInfo.isDead)                            // Check if player is dead
            return;

        player.GetComponent<ShipController>().ResetBoost();                     // Reset boost bar and make available the boost/brake again.
        player.GetComponent<BarrelRollController>().inBarrelRoll = false;       // Reset barrel roll state in case it died during barrel roll.
        player.GetComponent<BarrelRollController>().ResetBarrelRollShield();    // Reset barrel roll shield effect.
        player.SetActive(true);                                                 // Show player again.
        ResetPlayerShield();                                                    // Reset Player Shield values.
        player.SendMessageUpwards("ResetPosition");                             // Reset Player position on GameplayPlane.
        player.SendMessageUpwards("ResetRotation");                             // Reset Player rotation on GameplayPlane.
        invulnerable = true;                                                    // Make Player invulnerable untill flickering effect has dissapear.
        StartCoroutine(FlickeringColor(recoverColor));                          // Play flickering effect.
        GameManager.Instance.playerInfo.isDead = false;                         // Update die status.
    }
}
