using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;

public class EntryPoint : MonoBehaviour
{
    private ILRuntime.Runtime.Enviorment.AppDomain m_appDomain;
    private HotFixLoader m_hotFixLoader;
    public object m_gameRoot;
    public IMethod m_updateMethod;
    // Start is called before the first frame update
    public void Init(bool loadFromDll)
    {
        m_hotFixLoader = gameObject.AddComponent<HotFixLoader>();
        m_hotFixLoader.Init(OnHotFixLoaded, loadFromDll);
    }
    
    private void OnHotFixLoaded(ILRuntime.Runtime.Enviorment.AppDomain appDomain)
    {
        Debug.Log("OnHotFixLoaded");
        m_appDomain = appDomain;
        IType hotFixGameRootType = m_appDomain.LoadedTypes["HotFix_Project.GameRoot"];
        m_gameRoot = ((ILType)hotFixGameRootType).Instantiate();
        UnityEngine.Object.Destroy(m_hotFixLoader);
        m_hotFixLoader = null;
        IMethod method = hotFixGameRootType.GetMethod("Start", 0);
        using (var ctx = appDomain.BeginInvoke(method))
        {
            ctx.PushObject(m_gameRoot);
            ctx.Invoke();
        }
        m_updateMethod = hotFixGameRootType.GetMethod("Update", 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_updateMethod != null)
        {
            using (var ctx = m_appDomain.BeginInvoke(m_updateMethod))
            {
                ctx.PushObject(m_gameRoot);
                ctx.Invoke();
            }
        }
    }
}
