using System.Collections.Generic;
using System.IO;
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

        // // Ensure the directory exists
        // if (!AssetDatabase.IsValidFolder("Assets/Game"))
        // {
        //     AssetDatabase.CreateFolder("Assets", "Game");
        // }
        // if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
        // {
        //     AssetDatabase.CreateFolder("Assets/Game", "Resources");
        // }
        // if (!AssetDatabase.IsValidFolder(folderPath))
        // {
        //     AssetDatabase.CreateFolder("Assets/Game/Resources", "Pokemons");
        // }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            PokemonBase pokemon = ScriptableObject.CreateInstance<PokemonBase>();

            // Set properties
            int index = int.Parse(values[0]); // CSV 첫 번째 열: PokemonIndex
            pokemon.GetType().GetField("pokemonIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, index);
            pokemon.GetType().GetField("pokemonName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, values[1]);
            pokemon.GetType().GetField("pokemonDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, values[2]);
            pokemon.GetType().GetField("pokemonType1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, (PokemonType)System.Enum.Parse(typeof(PokemonType), values[3]));
            pokemon.GetType().GetField("pokemonType2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, (PokemonType)System.Enum.Parse(typeof(PokemonType), values[4]));
            pokemon.GetType().GetField("maxHp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[5]));
            pokemon.GetType().GetField("attack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[6]));
            pokemon.GetType().GetField("defence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[7]));
            pokemon.GetType().GetField("spAttack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[8]));
            pokemon.GetType().GetField("spDefence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[9]));
            pokemon.GetType().GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, int.Parse(values[10]));

            // Load sprites
            string formattedIndex = index.ToString("D3"); // 001 형식으로 변환
            string frontImgPath = $"Assets/04.Images/PokemonSprite_Front/{formattedIndex}.png";
            string backImgPath = $"Assets/04.Images/PokemonSprite_Back/{formattedIndex}.png";

            // Debugging sprite paths
            Debug.Log($"Loading Front Sprite from path: {frontImgPath}");
            Debug.Log($"Loading Back Sprite from path: {backImgPath}");

            Sprite frontSprite = AssetDatabase.LoadAssetAtPath<Sprite>(frontImgPath);
            Sprite backSprite = AssetDatabase.LoadAssetAtPath<Sprite>(backImgPath);

            // Debug check if sprites are null
            if (frontSprite == null)
            {
                Debug.LogError($"Failed to load Front Sprite for Pokemon {index} at path: {frontImgPath}");
            }
            if (backSprite == null)
            {
                Debug.LogError($"Failed to load Back Sprite for Pokemon {index} at path: {backImgPath}");
            }

            pokemon.GetType().GetField("frontSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, frontSprite);
            pokemon.GetType().GetField("backSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pokemon, backSprite);

            // Save ScriptableObject
            string assetPath = $"{folderPath}/{formattedIndex}_{values[1]}.asset";
            AssetDatabase.CreateAsset(pokemon, assetPath);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("CSV Import Completed!");
    }

}
