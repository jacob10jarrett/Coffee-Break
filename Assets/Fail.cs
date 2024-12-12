using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FailSceneManager : MonoBehaviour
{
    public string coffeeShopSceneName = "CoffeeShop"; 
    public float delayBeforeRetry = 2f;

    void Start()
    {

        StartCoroutine(ReloadCoffeeShopAfterDelay());
    }

    IEnumerator ReloadCoffeeShopAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeRetry);
        LoadCoffeeShopScene();
    }

    void LoadCoffeeShopScene()
    {
        if (!string.IsNullOrEmpty(coffeeShopSceneName))
        {
            SceneManager.LoadScene(coffeeShopSceneName);
        }
        else
        {
            Debug.LogError("CoffeeShop scene name is not set in the FailSceneManager.");
        }
    }

    public void ReloadSceneManually()
    {
        StopAllCoroutines(); // Stop any existing coroutines to prevent multiple triggers
        LoadCoffeeShopScene();
    }
}
