using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void LoadAudioRooms()
    {
        SceneManager.LoadScene("AudioRooms");
    }

}
