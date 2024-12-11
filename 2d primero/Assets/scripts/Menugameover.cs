using UnityEngine;
using UnityEngine.SceneManagement;

public class Menugameover : MonoBehaviour
{
    private PlayerController player;
    [SerializeField] private GameObject menuMuerte;
    private bool shouldShowMenu = false;

    void Start()
    {
        player = PlayerController.Instance;
        HideMenu();
    }

    void Update()
    {
        if (shouldShowMenu)
        {
            ShowMenu();
        }
        else
        {
            HideMenu();
        }
    }

    public void ShowDeathMenu()
    {
        shouldShowMenu = true;
    }

    void ShowMenu()
    {
        menuMuerte.SetActive(true);
    }

    public void HideMenu()
    {
        menuMuerte.SetActive(false);
    }

    public void RestartGame()
    {
        GameManager.Instance.respawnplayer();
        shouldShowMenu = false;
        HideMenu();
    }

    public void HideDeathMenu()
    {
        shouldShowMenu = false;
        HideMenu();
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}