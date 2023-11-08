using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public class CreateMagicOnionClass : EditorWindow
{
    private enum TabState
    {
        Service = 0,
        StreamingHub
    }

    private TabState tabIndex = TabState.Service;
    private List<string> tabNames = new List<string>();
    
    [MenuItem("MagicOnion/CreateClass")]
    private static void ShowWindow()
    {
        var window = CreateInstance<CreateMagicOnionClass>();
        window.Show();
        window.Init();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("MagicOnion Class", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            tabIndex = (TabState)GUILayout.Toolbar((int)tabIndex, tabNames.ToArray(), new GUIStyle(EditorStyles.toolbarButton), GUI.ToolbarButtonSize.FitToContents);
        }

        switch (tabIndex)
        {
            case TabState.Service:
                CreateService();
                break;
            case TabState.StreamingHub:
                CreateStreamingHub();
                break;
        }
    }

    // template path
    private static readonly string ClientTemplatePath = "{0}/Framework/Scripts/Template/{1}";
    private static readonly string ServerTemplatePath = "{0}/MagicOnionServer/MagicOnionServer/Template/{1}";
    
    // streaming hub template file
    private static readonly string TemplateIStreamingHubClassName = "SharedInterface/ITemplateHub.cs";
    private static readonly string TemplateStreamingHubClientClassName = "TemplateHubClient.cs";
    private static readonly string TemplateStreamingHubServerClassName = "TemplateHub.cs";
    
    // service template file
    private static readonly string TemplateIServiceClassName = "SharedInterface/ITemplateService.cs";
    private static readonly string TemplateServiceServerClassName = "TemplateService.cs";
    
    // StreamingHub
    private static readonly string IStreamingHubClassName = "/I{0}Hub.cs";
    private static readonly string StreamingHubClientClassName = "/{0}HubClient.cs";
    private static readonly string StreamingHubServerClassName = "/{0}Hub.cs";

    // Service
    private static readonly string IServiceClassName = "/I{0}Service.cs";
    private static readonly string ServiceServerClassName = "/{0}Service.cs";

    // Create base path
    private static readonly string InterfaceBasePath = "{0}/Scripts/Shared";
    private static readonly string ClientScriptBasePath = "{0}/Scripts";
    private static readonly string ServerScriptBasePath = "{0}/MagicOnionServer/MagicOnionServer";

    private string className = "";
    private string interfacePath = "";
    private string clientScriptPath = "";
    private string serverScriptPath = "";

    private void Init()
    {
        tabNames.Clear();
        foreach (var state in Enum.GetValues(typeof(TabState)))
        {
            tabNames.Add(state.ToString());
        }
        
        interfacePath = string.Format(InterfaceBasePath, Application.dataPath);
        clientScriptPath = string.Format(ClientScriptBasePath, Application.dataPath);
        serverScriptPath = string.Format(ServerScriptBasePath, Application.dataPath.Replace("/Assets", ""));
    }
    
    /// <summary>
    /// ServiceClass作成（API）
    /// </summary>
    private void CreateService()
    {
        ClassNameView("Service Name");

        var iClassName = string.Format(IServiceClassName, className);
        ReferencePathView("Interface Folder", iClassName, ref interfacePath);

        var serverClassName = string.Format(ServiceServerClassName, className);
        ReferencePathView("Server Folder", string.Format(serverClassName, className), ref serverScriptPath);

        if (GUILayout.Button("Create Class"))
        {
            if (string.IsNullOrEmpty(className))
            {
                Debug.LogError("クラス名が未入力です。");
                return;
            }
            var interfaceTemplatePath = string.Format(ClientTemplatePath, Application.dataPath, TemplateIServiceClassName);
            GenerateScriptFile(interfaceTemplatePath, interfacePath + iClassName);

            var serverTemplatePath = string.Format(ServerTemplatePath, Application.dataPath.Replace("/Assets", ""), TemplateServiceServerClassName);
            GenerateScriptFile(serverTemplatePath, serverScriptPath + serverClassName);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Close();
        }
    }

    private void CreateStreamingHub()
    {
        ClassNameView("StreamingHub Name");

        var iClassName = string.Format(IStreamingHubClassName, className);
        ReferencePathView("Interface Folder", iClassName, ref interfacePath);
        
        var serverClassName = string.Format(StreamingHubServerClassName, className);
        ReferencePathView("Server Folder", string.Format(serverClassName, className), ref serverScriptPath);
        
        var clientClassName = string.Format(StreamingHubClientClassName, className);
        ReferencePathView("Client Folder", clientClassName, ref clientScriptPath);

        if (GUILayout.Button("Create Class"))
        {
            if (string.IsNullOrEmpty(className))
            {
                Debug.LogError("クラス名が未入力です。");
                return;
            }
            var interfaceTemplatePath = string.Format(ClientTemplatePath, Application.dataPath, TemplateIStreamingHubClassName);
            GenerateScriptFile(interfaceTemplatePath, interfacePath + iClassName);
            
            var serverTemplatePath = string.Format(ServerTemplatePath, Application.dataPath.Replace("/Assets", ""), TemplateStreamingHubServerClassName);
            GenerateScriptFile(serverTemplatePath, serverScriptPath + serverClassName);
            
            var clientTemplatePath = string.Format(ClientTemplatePath, Application.dataPath, TemplateStreamingHubClientClassName);
            GenerateScriptFile(clientTemplatePath, clientScriptPath + clientClassName);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Close();
        }
    }

    /// <summary>
    /// クラス名入力
    /// </summary>
    private void ClassNameView(string label)
    {
        GUILayout.Space(10);
        using (new EditorGUILayout.VerticalScope(new GUIStyle(GUI.skin.box)))
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Name");
                className = EditorGUILayout.TextField(className);
            }
        }
    }
    
    /// <summary>
    /// 生成場所を指定
    /// </summary>
    private void ReferencePathView(string label, string fileName, ref string path)
    {
        using (new EditorGUILayout.VerticalScope(new GUIStyle(GUI.skin.box)))
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                path = EditorGUILayout.TextField(path);
                
                if (GUILayout.Button("Reference"))
                {
                    var openPath = EditorUtility.OpenFolderPanel("Select Folder", path, "");
                    path = string.IsNullOrEmpty(openPath) ? path : openPath;
                }
            }
            GUILayout.Space(10);
            GUILayout.Label(path + fileName, EditorStyles.boldLabel);
        }
        GUILayout.Space(5);
    }

    /// <summary>
    /// クラスを生成
    /// テンプレートをコピー->コピーした物をクラス名に置換->.cs内の「Template」をクラス名に置換
    /// </summary>
    /// <param name="templateFilePath"></param>
    /// <param name="copyPath"></param>
    private async void GenerateScriptFile(string templateFilePath, string copyPath)
    {
        if (File.Exists(copyPath))
        {
            Debug.LogError($"作成しようとしたクラスは既に存在します。\n{copyPath}");
            return;
        }
        File.Copy(templateFilePath, copyPath);
        
        await UniTask.NextFrame();
        
        var str = await File.ReadAllTextAsync(copyPath, Encoding.UTF8);
        
        using (var sr = new StreamWriter(copyPath, false))
        {
            str = str.Replace("Template", className);
            await sr.WriteAsync(str);
            sr.Close();
        }

        Debug.Log($"作成完了\n{copyPath}");
    }
    
}
