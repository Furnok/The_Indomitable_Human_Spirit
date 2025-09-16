using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class S_Utils
{
    #region COROUTINE
    public static IEnumerator Delay(float delay, Action action = null)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public static IEnumerator DelayFrame(Action action = null)
    {
        yield return null;
        action?.Invoke();
    }
    #endregion

    #region SCENE
    public static IEnumerator LoadSceneAsync(int sceneIndex, LoadSceneMode loadMode, Action action = null)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, loadMode);

        yield return new WaitUntil(() => asyncLoad.isDone);

        action?.Invoke();
    }

    public static IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode loadMode, Action action = null)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadMode);

        yield return new WaitUntil(() => asyncLoad.isDone);

        action?.Invoke();
    }

    public static IEnumerator UnloadSceneAsync(string sceneName, Action action = null)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(sceneName);

        yield return new WaitUntil(() => asyncLoad.isDone);

        action?.Invoke();
    }

    public static IEnumerator UnloadSceneAsync(int sceneIndex, Action action = null)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(sceneIndex);

        yield return new WaitUntil(() => asyncLoad.isDone);

        action?.Invoke();
    }
    #endregion
}