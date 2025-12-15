using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class FlyLittleBread : MonoBehaviour
{

    [SerializeField] AudioSource playerAudio;


    [Header ( "Audio Clip" ) ]

    public AudioClip BgSaund;
    public AudioClip FlySaund;
    public AudioClip DeadSaund;

    

    [Header("Player Movment")]

    public GameManager gameManager;
    public float Velocity = 1;
    private Rigidbody2D rb;
    
    


    void Start()
    {
        playerAudio = this.GetComponent<AudioSource>();

        playerAudio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }

   
    
    void Update()
    {
       

        if (Input.GetMouseButtonDown(0))
        {
            //jump
            rb.velocity = Vector2.up * Velocity;
            playerAudio.PlayOneShot(FlySaund, 0.1f);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //jump
            rb.velocity = Vector2.up * Velocity;
            playerAudio.PlayOneShot(FlySaund, 0.1f);
        }


    }

    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        playerAudio.Stop();
        playerAudio.PlayOneShot(DeadSaund, 0.1f);
        gameManager.GameOver();

    }
}
