using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class PokemonCSVImporter : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Import Pokemon CSV")]
    public static void OpenWindow()
    {
        GetWindow<PokemonCSVImporter>("Import Pokemon CSV");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV Importer", EditorStyles.boldLabel);
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

        string folderPath = "Assets/Game/Resources/Pokemons";

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Game/Resources", "Pokemons");
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            PokemonBase pokemon = ScriptableObject.CreateInstance<PokemonBase>();

            // Set properties
            int index = int.Parse(values[0]);
            pokemon.GetType().GetField("pokemonIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, index);
            pokemon.GetType().GetField("pokemonName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, values[1]);
            pokemon.GetType().GetField("pokemonDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, values[2]);
            pokemon.GetType().GetField("pokemonType1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, (PokemonType)System.Enum.Parse(typeof(PokemonType), values[3]));
            // pokemonType2가 비어있거나 지정되지 않았을 때 처리
            PokemonType type2 = values[4].Trim() == "" ? PokemonType.None : (PokemonType)System.Enum.Parse(typeof(PokemonType), values[4]);
            pokemon.GetType().GetField("pokemonType2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, type2);

            pokemon.GetType().GetField("maxHp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[5]));
            pokemon.GetType().GetField("attack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[6]));
            pokemon.GetType().GetField("defence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[7]));
            pokemon.GetType().GetField("spAttack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[8]));
            pokemon.GetType().GetField("spDefence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[9]));
            pokemon.GetType().GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[10]));

            pokemon.GetType().GetField("expYield", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[11]));
            pokemon.GetType().GetField("growthRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, (GrowthRate)System.Enum.Parse(typeof(GrowthRate), values[12]));
            pokemon.GetType().GetField("catchRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[13]));

            // Parse and set LearnableSkills
            List<LearnableSkill> learnableSkills = ParseLearnableSkills(values[14]);
            pokemon.GetType().GetField("learnableSkills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, learnableSkills);

            string formattedIndex = index.ToString("D3");

            // Save ScriptableObject
            string assetPath = $"{folderPath}/{formattedIndex}_{values[1]}.asset";
            AssetDatabase.CreateAsset(pokemon, assetPath);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("CSV Import Completed!");
    }
    private List<LearnableSkill> ParseLearnableSkills(string skillData)
    {
        List<LearnableSkill> skills = new List<LearnableSkill>();

        // |로 스킬을 나누기
        string[] skillEntries = skillData.Split('|');

        foreach (string entry in skillEntries)
        {
            // 공백 제거 후 항목 처리
            string trimmedEntry = entry.Trim();
            if (string.IsNullOrEmpty(trimmedEntry)) continue;

            // '1:몸통박치기' 형식으로 나누기
            string[] parts = trimmedEntry.Split(':');

            if (parts.Length == 2)
            {
                int level = int.Parse(parts[0].Trim());
                string skillName = parts[1].Trim();

                // 해당 스킬을 SkillBase로 로드
                SkillBase skillBase = Resources.Load<SkillBase>($"Skills/{skillName}_Skill");
                if (skillBase != null)
                {
                    skills.Add(new LearnableSkill(skillBase, level));
                }
                else
                {
                    Debug.LogWarning($"SkillBase not found at path: Skills/{skillName}_Skill");
                }
            }
        }
        return skills;
    }
}
