/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Interface for resettable classes
/// </summary>
public interface ResetInterface
{
	bool Reset();
}

/// <summary>
/// Basic generic singleton class for C#. This class is meant to simplify declaring 
/// singletons in the code (i.e. classes where there is a unique instance, such as managers)
/// </summary>
/// <typeparam name="T">Type for the singleton</typeparam>
public class CSingleton<T> : ResetInterface
    where T : ResetInterface, new()
{
	/// <summary>
	/// Boolean flag which indicates whether this singleton may be reset
	/// Resetting will clear all data stored within the object and reset them
	/// to their default values.
	/// </summary>
	public bool CanBeReset = false;

	private static T m_sInstance;
	public static T instance
	{
		get
		{
			if (m_sInstance == null)
			{
				m_sInstance = new T();
			}
			return m_sInstance;
		}
	}

	/// <summary>
	/// If the existing singleton object can be reset, will reset the object
	/// Otherwise, does nothing.
	/// </summary>
	/// <returns>True upon successful reset; false otherwise</returns>
	public virtual bool Reset()
	{
		if (m_sInstance == null) return false;
		if (!CanBeReset) return false;

		m_sInstance = default(T);
		return true;
	}
}

/// <summary>
/// Basic generic singleton class for Unity monobehaviours. This class is meant to simplify declaring 
/// singletons in the code (i.e. classes where there is a unique instance, such as managers)
/// </summary>
/// <typeparam name="T">Type for the singleton</typeparam>
public class CSingletonMono<T> : MonoBehaviour
	where T : MonoBehaviour
{
	/// <summary>
	/// Boolean flag which indicates whether this singleton may be reset
	/// Resetting will clear all data stored within the object and reset them
	/// to their default values.
	/// </summary>
	public bool CanBeReset = true;

	private static T m_sInstance;
	public static T instance
	{
		get
		{
			if (m_sInstance == null)
			{
				m_sInstance = FindObjectOfType(typeof(T)) as T;

				if (m_sInstance == null)
				{
					GameObject obj = new GameObject(typeof(T).Name);
					m_sInstance = obj.AddComponent<T>();
					DontDestroyOnLoad(obj);
				}
			}

			return m_sInstance;
		}
	}

	/// <summary>
	/// If the existing singleton object can be reset, will reset the object
	/// Otherwise, does nothing.
	/// </summary>
	/// <returns>True upon successful reset; false otherwise</returns>
	public virtual bool Reset()
	{
		if (m_sInstance == null) return false;
		if (!CanBeReset) return false;

		Destroy(m_sInstance.gameObject);
		m_sInstance = default(T);
		return true;
	}

	/// <summary>
	/// Returns whether there exists a valid instance for this monobehavior class
	/// </summary>
	public static bool IsValid()
	{
		return (m_sInstance != null);
	}

	/// <summary>
	/// Ensures the uniqueness of the Monobehaviour singleton
	/// </summary>
	protected virtual void Awake()
	{
		if (m_sInstance == null)
		{
			m_sInstance = this as T;
		}
		else if (m_sInstance != this as T)
		{
			Destroy(gameObject);
			return;
		}
	}

	/// <summary>
	/// Resets singleton object upon object destruction
	/// </summary>
	protected virtual void OnDestroy()
	{
		if (m_sInstance == this)
		{
			m_sInstance = default(T);
		}
	}
}
