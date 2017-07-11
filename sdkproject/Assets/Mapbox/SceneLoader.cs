using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapbox
{
	public class SceneLoader : MonoBehaviour
	{
		int _currentSceneIndex = 1;

		void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		public void LoadNextScene()
		{
			SceneManager.LoadScene(_currentSceneIndex);
			_currentSceneIndex++;
		}
	}
}
