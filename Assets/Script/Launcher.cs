using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public bool m_loadFromDll;
    void Start()
    {
        GameObject entryPoint = GameObject.Find("EntryPoint");
        if (entryPoint == null)
        {
            entryPoint = new GameObject();
            entryPoint.name = "EntryPoint";
            DontDestroyOnLoad(entryPoint);
            EntryPoint e = entryPoint.AddComponent<EntryPoint>();
            e.Init(m_loadFromDll);
        }
        GameObject.Destroy(gameObject);
    }
}
