using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SearchService;

public class GameEnding : MonoBehaviour
{
    public static float displayImageDuration = 1f;
    public static float fadeDuration = 1f;
    static float m_Timer;
    public static Image winImage;
    public static Image loseImage;
    public static bool End = false;
    public static string result = "lose"; 

    //Audio
    static bool m_HasAudioPlayed;
    public AudioSource escapeAudio;
    public AudioSource winAudio;


    private void Start()
    {
        winImage = this.transform.GetChild(6).gameObject.GetComponent<Image>();
        loseImage = this.transform.GetChild(5).gameObject.GetComponent<Image>();
        End = false;
    }

    private void Update()
    {
        if(winImage == null) { Debug.Log("NO HAY IMAGEN"); }
        if (End)
        {
            if(result == "win")
            {
                EndLevel(winAudio);
            }
            else
            {
                EndLevel(escapeAudio);
            }
            
           
        }
    }



    public static void EndLevel(AudioSource audioSource)
    {
        Spawner.sheeps.Clear();
        AGrid.feeders.Clear();
        Image imagen;
        if(result == "win")
        {
            imagen = winImage;
        }
        else
        {
            imagen = loseImage;
        }

        if (!m_HasAudioPlayed)
        {
            audioSource.Play();
            m_HasAudioPlayed = true;
        }

        m_Timer += Time.deltaTime;
        float alpha = m_Timer / fadeDuration;
        Color color = imagen.color;
        color.a = alpha;
        imagen.color = color ;

        if(m_Timer > fadeDuration + displayImageDuration)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
