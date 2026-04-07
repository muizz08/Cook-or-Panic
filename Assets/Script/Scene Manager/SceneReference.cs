using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneReference
{
#if UNITY_EDITOR
    public SceneAsset sceneAsset; // drag & drop di inspector
#endif

    [SerializeField] private string sceneName; // dipakai saat build

    public string SceneName => sceneName;

#if UNITY_EDITOR
    public void UpdateSceneName()
    {
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
    }
#endif
}