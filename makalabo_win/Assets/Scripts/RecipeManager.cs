using SQLite;  // SQLiteを使用するために必要
using System.Collections.Generic;  // List<T> を使用するために必要
using UnityEngine;  // Unityの機能（MonoBehaviour, Debug.Log など）を使用するために必要

public class RecipeManager : MonoBehaviour
{
    private DatabaseManager dbManager;

    void Start()
    {
        // データベースのパス
        string dbPath = "Assets/StreamingAssets/makalabo_db.db";  // 実際のパスに変更してください

        // データベースマネージャを初期化
        dbManager = new DatabaseManager();
        dbManager.InitializeDatabase(dbPath);

        // ランダムで10個のレシピを取得
        List<Recipe> randomRecipe = dbManager.GetRandomRecipe(10);

        // 取得したレシピをコンソールに表示
        foreach (var recipe in randomRecipe)
        {
            Debug.Log("レシピID: " + recipe.RecipeID);
            Debug.Log("レシピ名: " + recipe.RecipeName);
            Debug.Log("画像パス: " + recipe.ImagePath);
            Debug.Log("詳細: " + recipe.Details);
            Debug.Log("------------------------");
        }

        // データベースを閉じる
        //dbManager.CloseDatabase();
    }
}