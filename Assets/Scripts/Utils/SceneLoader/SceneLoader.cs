using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
	private static Scene targetScene;
	public enum Scene
	{
		SCN_Menu,
		SCN_Game,
		SCN_Lobby,
		SCN_WaitLobby,
	}

	public static void Load(Scene p_targetScene)
	{
		SceneManager.LoadScene(p_targetScene.ToString(), LoadSceneMode.Single);
	}

	public static void LoadNetwork(Scene p_targetScene)
	{
		NetworkManager.Singleton.SceneManager.LoadScene(p_targetScene.ToString(), LoadSceneMode.Single);
	}

    public static void SceneLoaderCallback()
	{
        SceneManager.LoadScene(targetScene.ToString());
    }
}