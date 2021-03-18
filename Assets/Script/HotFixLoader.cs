using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

#pragma warning disable CS0618

public delegate void OnHotFixLoaded(ILRuntime.Runtime.Enviorment.AppDomain appDomain);
public class HotFixLoader : MonoBehaviour
{
    public void Init(OnHotFixLoaded callBack, bool LoadDll)
    {
        if (LoadDll)
        {
            StartCoroutine(LoadFromDll(callBack));
        }
        else
        {
            StartCoroutine(LoadFromAssetBundle(callBack));
        }
    }
    
    private IEnumerator LoadFromDll(OnHotFixLoaded callBack)
    {
        ILRuntime.Runtime.Enviorment.AppDomain appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        string dllPath = string.Format("{0}{1}{2}", "file://", Application.streamingAssetsPath, "/HotFix_Project.dll");
        string pdbPath = string.Format("{0}{1}{2}", "file://", Application.streamingAssetsPath, "/HotFix_Project.pdb");

        WWW www = new WWW(dllPath);
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] dll = www.bytes;
        www.Dispose();

        www = new WWW(pdbPath);
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] pdb = www.bytes;
        MemoryStream fs = new MemoryStream(dll);
        MemoryStream p = new MemoryStream(pdb);
        try
        {
            appDomain.LoadAssembly(fs, p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        catch
        {
            Debug.LogError("加载热更DLL失败，请确保已经通过VS打开Assets/Samples/ILRuntime/1.6/Demo/HotFix_Project/HotFix_Project.sln编译过热更DLL");
        }
        
        InitializeILRuntime(appDomain);
        if (callBack != null)
        {
            callBack(appDomain);
        }
        www.Dispose();
    }
    private IEnumerator LoadFromAssetBundle(OnHotFixLoaded callBack)
    {
        ILRuntime.Runtime.Enviorment.AppDomain appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        string dllPath = string.Format("{0}{1}", Application.streamingAssetsPath, "/hotfix_project.bundle");;
        Debug.Log("LoadDllFromStreamingAssetsPath dllPath: " + dllPath);
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(dllPath);
        yield return new WaitUntil(() => abcr.isDone && abcr.assetBundle != null);
        AssetBundle ab = abcr.assetBundle;
        AssetBundleRequest abr = ab.LoadAssetAsync<TextAsset>("hotfix_project");
        yield return new WaitUntil(() => abr.isDone && abr.asset != null);
        Debug.Log("LoadDllFromStreamingAssetsPath abr");
        TextAsset txt = abr.asset as TextAsset;
        ab.Unload(false);
        byte[] dll = txt.bytes;
        MemoryStream fs = new MemoryStream(dll);
        try
        {
            appDomain.LoadAssembly(fs);
        }
        catch
        {
            Debug.LogError("加载热更DLL失败，请确保已经通过VS打开Assets/Samples/ILRuntime/1.6/Demo/HotFix_Project/HotFix_Project.sln编译过热更DLL");
        }
        InitializeILRuntime(appDomain);
        if (callBack != null)
        {
            callBack(appDomain);
        }
    }
    
    private void InitializeILRuntime(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
    {
        Debug.Log("InitializeILRuntime");
        appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
        appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
        appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());

        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<Vector2>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<Vector2>((arg0) =>
            {
                ((Action<Vector2>)act)(arg0);
            });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
        {
            return new UnityEngine.Events.UnityAction(() =>
            {
                ((Action)act)();
            });
        });
        LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
    }
}
