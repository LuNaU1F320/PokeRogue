using UnityEngine;
using UnityEditor;
using System.IO;

public class SkillCSVImporter : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Import Skill CSV")]
    public static void OpenWindow()
    {
        GetWindow<SkillCSVImporter>("Import Skill CSV");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV Importer for Skills", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import"))
        {
            if (csvFile != null)
            {
                ImportCSV(csvFile);
            }
            else
            {
                Debug.LogError("Please assign a CSV file.");
            }
        }
    }

    private void ImportCSV(TextAsset csv)
    {
        string[] lines = csv.text.Split('\n');
        string[] headers = lines[0].Split(',');

        string folderPath = "Assets/Game/Resources/Skills"; // Resources 폴더 내 경로

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Game/Resources", "Skills");
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            SkillBase skill = ScriptableObject.CreateInstance<SkillBase>();

            int skillIndex = int.Parse(values[0]);
            skill.GetType().GetField("skillIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(skill, skillIndex);

            skill.GetType().GetField("skillName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(skill, values[1]);

            // skillDescription 필드 제거

            // skillType을 Enum으로 파싱
            skill.GetType().GetField("skillType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(skill, (PokemonType)System.Enum.Parse(typeof(PokemonType), values[2]));

            // categoryKey를 Enum으로 파싱
            skill.GetType().GetField("categoryKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(skill, (CategoryKey)System.Enum.Parse(typeof(CategoryKey), values[3], true));

            // skillPower, skillAccuracy, skillPP을 숫자로 파싱
            skill.GetType().GetField("skillPower", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(skill, int.Parse(values[4]));

            // skillAccuracy가 퍼센트 형식일 경우 % 기호를 제거하고 숫자로 파싱
            skill.GetType().GetField("skillAccuary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(skill, int.Parse(values[5].Replace("%", "")));

            // skillPP을 숫자 파싱
            skill.GetType().GetField("skillPP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(skill, int.Parse(values[6]));

            // 파일명 앞에 인덱스를 추가 (예: 000_막치기_Skill.asset)
            string fileName = $"{skillIndex:D3}_{values[1]}_Skill.asset"; // skillIndex를 3자리로 포맷
            string assetPath = Path.Combine(folderPath, fileName);
            AssetDatabase.CreateAsset(skill, assetPath);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Skill CSV Import Completed!");
    }
}
