using game.basemono;
using game.config;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Download : BaseMonoBehaviour
{
    public Slider curentProgress;
    public Slider totalProgress;

    private game.resource.DataController dataController;
    private bool isEnteringGame;

    private void Start()
    {
        game.resource.dataController.Config.InitializeRuntimePaths();

        this.dataController = new game.resource.DataController();
        this.curentProgress.value = 0;
        this.totalProgress.value = 0;

        Debug.Log("game.resource.DataController >> full storage path: " + game.resource.dataController.Config.GetLocalStogareFullPath());

        StartCoroutine(this.dataController.Fetch());
    }

    private void Update()
    {
        if (!this.isEnteringGame && this.dataController.IsCompleted() == true)
        {
            this.curentProgress.value = 1;
            this.totalProgress.value = 1;
            this.isEnteringGame = true;

            if (Game.Initialize())
            {
                Debug.Log("Update is completed. Entering the login scene");
                SceneManager.LoadScene(ConfigGame.characterScreen);
            }
            else
            {
                Debug.LogError("Download completed, but game resource initialization failed.");
            }

            return;
        }

        this.curentProgress.value = this.dataController.GetCurentProgress();
        this.totalProgress.value = this.dataController.GetTotalProgress();
    }
}
