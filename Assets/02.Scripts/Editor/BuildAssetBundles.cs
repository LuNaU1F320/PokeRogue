using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string bundleDir = "Assets/AssetBundles";

        // 폴더가 없으면 생성
        if (!Directory.Exists(bundleDir))
        {
            Directory.CreateDirectory(bundleDir);
        }

        // 빌드 실행
        BuildPipeline.BuildAssetBundles(bundleDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        // 파일 확인
        string path = Path.Combine(bundleDir, "pokemonsprites");
        if (File.Exists(path))
        {
            Debug.Log("✅ AssetBundle 빌드 완료: " + path);
        }
        else
        {
            Debug.LogError("❌ AssetBundle 파일이 생성되지 않았습니다.");
        }
    }
}
