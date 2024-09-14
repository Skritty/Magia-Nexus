using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Skritty.Tools.Utilities
{
	[DefaultExecutionOrder(-50)]
	public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance { get; private set; }

		[field: SerializeField]
#if ODIN_INSPECTOR
		[field: Sirenix.OdinInspector.LabelText("Use DontDestroyOnLoad")]
#endif
		public bool UseDontDestroyOnLoad { get; private set; } = true;

		protected virtual void Awake()
		{
			if(Instance != null && Instance != this as T)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this as T;
			if(UseDontDestroyOnLoad)
			{
				DontDestroyOnLoad(transform.root);
			}
		}

		protected virtual void OnApplicationQuit()
		{
			Instance = null;
			Destroy(gameObject);
		}
	}
}