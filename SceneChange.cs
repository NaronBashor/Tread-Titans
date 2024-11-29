using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void OnChangeSceneButton()
    {
        SceneManager.LoadScene(sceneName);
    }
}