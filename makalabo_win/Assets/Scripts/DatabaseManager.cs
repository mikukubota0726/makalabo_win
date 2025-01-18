using SQLite;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Data;
using Mono.Data.Sqlite; // Android向けにはMono.Data.Sqliteを利用

public class DatabaseManager
{
    private SQLiteConnection dbConnection;
    private string databaseFilePath;

    public void EnsureDatabaseConnection()
    {
        if (dbConnection == null || !IsDatabaseOpen())
        {
            Debug.LogWarning("Database connection is closed or null. Reinitializing...");
            InitializeDatabase(databaseFilePath);
        }
    }

    private string connectionString = "Data Source=Assets/StreamingAssets/makalabo_db.db;";

    private bool IsDatabaseOpen()
    {
        try
        {
            dbConnection.ExecuteScalar<int>("SELECT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }

    void Start()
    {
        // データベースのファイルパスを設定
        databaseFilePath = Path.Combine(Application.streamingAssetsPath, "makalabo_db.db");   
        Debug.Log("Database file path: " + databaseFilePath);     
        // SQLiteConnectionを作成
        SQLiteConnection dbConnection = new SQLiteConnection(databaseFilePath);
        
        // ここでデータベース操作を行う
    }

    private static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DatabaseManager();
                string dbPath = System.IO.Path.Combine(Application.streamingAssetsPath, "makalabo_db.db");
                _instance.InitializeDatabase(dbPath);
            }
            return _instance;
        }
    }
    public void InitializeDatabase(string dbPath)
    {
        if (!System.IO.File.Exists(dbPath))
        {
            string sourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, "makalabo_db.db");
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    // Androidの場合の特殊なコピー処理
                    using (WWW www = new WWW(sourcePath))
                    {
                        while (!www.isDone) { }
                        System.IO.File.WriteAllBytes(dbPath, www.bytes);
                    }
                }
                else
                {
                    System.IO.File.Copy(sourcePath, dbPath);
                }
                Debug.Log("データベースをコピーしました: " + dbPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("データベースのコピーに失敗しました: " + ex.Message);
            }
        
        }

        dbConnection = new SQLiteConnection(dbPath);
        Debug.Log("データベース接続を開きました: " + dbPath);

            // データベース接続の確認
        if (dbConnection == null)
        {
            Debug.LogError("SQLiteConnection is null. Database path might be incorrect.");
        }
        if (dbConnection == null || dbConnection.Table<Recipe>().Count() == 0)
        {
            Debug.LogError("データベース接続が初期化されていません。");
        }
    }

    
    public List<Recipe> GetRandomRecipe(int count)
    {
        try
        {
            return dbConnection.Query<Recipe>("SELECT * FROM Recipe ORDER BY RANDOM() LIMIT ?", count);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("レシピの取得に失敗しました: " + ex.Message);
            return new List<Recipe>();
        }
    }

    // 料理名または材料名で検索するメソッド
    public List<Recipe> SearchRecipe(string keyword)
    {
        try
        {
            // 料理名検索
            var query = @"
                SELECT DISTINCT Recipe.*
                FROM Recipe
                LEFT JOIN RecipeIngredients ON Recipe.RecipeID = RecipeIngredients.RecipeID
                LEFT JOIN Ingredients ON RecipeIngredients.IngredientID = Ingredients.IngredientID
                WHERE Recipe.RecipeName LIKE '%' || ? || '%'
                OR Ingredients.Name LIKE '%' || ? || '%'
            ";
            return dbConnection.Query<Recipe>(query, keyword, keyword);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("検索に失敗しました: " + ex.Message);
            return new List<Recipe>();
        }
    }

    
    //アレルギー除外　検索メソッド
    public List<Recipe> SearchRecipeExcludingAllergies(string keyword)
    {
        try
        {
            // アレルギーに該当する材料を含むレシピを除外
            var query = @"
                SELECT DISTINCT Recipe.*
                FROM Recipe
                LEFT JOIN RecipeIngredients ON Recipe.RecipeID = RecipeIngredients.RecipeID
                LEFT JOIN Ingredients ON RecipeIngredients.IngredientID = Ingredients.IngredientID
                WHERE (Recipe.RecipeName LIKE '%' || ? || '%' OR Ingredients.Name LIKE '%' || ? || '%')
                AND Recipe.RecipeID NOT IN (
                    SELECT DISTINCT RecipeIngredients.RecipeID
                    FROM RecipeIngredients
                    JOIN Ingredients ON RecipeIngredients.IngredientID = Ingredients.IngredientID
                    JOIN UserAllergies ON Ingredients.AllergyID = UserAllergies.AllergyID
                )
            ";
            return dbConnection.Query<Recipe>(query, keyword, keyword);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("アレルギー除外検索に失敗しました: " + ex.Message);
            return new List<Recipe>();
        }
    }



    // 材料名、量、単位を取得するメソッド
    public List<(string Name, string Quantity, string Unit)> GetIngredientsWithDetailsByRecipeID(int recipeID)
    {
        try
        {
            var query = @"
                SELECT Ingredients.Name, RecipeIngredients.Quantity, RecipeIngredients.Unit
                FROM Ingredients
                JOIN RecipeIngredients ON Ingredients.IngredientID = RecipeIngredients.IngredientID
                WHERE RecipeIngredients.RecipeID = ?
            ";
            return dbConnection.Query<(string Name, string Quantity, string Unit)>(query, recipeID);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("材料の取得に失敗しました: " + ex.Message);
            return new List<(string Name, string Quantity, string Unit)>();
        }
    }

    // 手順を取得するメソッド
    public List<(int StepNumber, string Description)> GetStepsByRecipeID(int recipeID)
    {
        try
        {
            var query = @"
                SELECT StepNumber, Description
                FROM Steps
                WHERE RecipeID = ?
                ORDER BY StepNumber ASC
            ";
            return dbConnection.Query<(int StepNumber, string Description)>(query, recipeID);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("手順の取得に失敗しました: " + ex.Message);
            return new List<(int StepNumber, string Description)>();
        }
    }


    // 結果を格納するためのクラス
    private class IngredientResult
    {
        public string Name { get; set; }
    }

            //////以下ブックマーク関連のコード//////

    public void AddBookmark(int recipeID)
    {
        EnsureDatabaseConnection(); // 接続を確認または再初期化
        try
        {
            string query = "INSERT INTO Bookmarks (RecipeID) VALUES (?)";
            dbConnection.Execute(query, recipeID);
            Debug.Log("ブックマークに追加しました: RecipeID = " + recipeID);
        }
        catch (Exception ex)
        {
            Debug.LogError("ブックマーク追加に失敗しました: " + ex.Message);
        }
    }

    public void RemoveBookmark(int recipeID)    ///ブックマークからの削除用メソッド
    {
        string query = "DELETE FROM Bookmarks WHERE RecipeID = ?";
        dbConnection.Execute(query, recipeID);
        Debug.Log("ブックマークから削除しました: RecipeID = " + recipeID);
    }

    public bool IsBookmarked(int recipeID)  ///特定のレシピがブックマーク内に登録されているかを確認するメソッド
    {
        string query = "SELECT COUNT(*) FROM Bookmarks WHERE RecipeID = ?";
        int count = dbConnection.ExecuteScalar<int>(query, recipeID);
        return count > 0;
    }

    public List<int> GetAllBookmarkedRecipe()  ///ブックマーク済みの全レシピを取得するメソッド
    {
        List<int> bookmarkedIDs = new List<int>();
        string query = "SELECT RecipeID FROM Bookmarks";

        try
        {
            using (var conn = new SqliteConnection("Data Source=Assets/StreamingAssets/makalabo_db.db;"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookmarkedIDs.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error retrieving bookmarks: " + ex.Message);
        }

        return bookmarkedIDs;
    }

    public Recipe GetRecipeByID(int recipeID)
    {
        Recipe recipe = null;
        string query = "SELECT * FROM Recipe WHERE RecipeID = @RecipeID";
        Debug.Log($"Executing SQL query: {query}");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.Add(new SqliteParameter("@RecipeID", recipeID));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        recipe = new Recipe
                        {
                            RecipeID = reader.GetInt32(0),
                            RecipeName = reader.GetString(1),
                            Details = reader.GetString(2),
                            ImagePath = reader.GetString(4),
                            // 他のフィールドを必要に応じて追加
                        };
                    }
                }
            }
        }
        return recipe;
    }


    public void AddUserAllergies(int allergyID)
    {
        string query = "INSERT OR IGNORE INTO UserAllergies (AllergyID) VALUES (?)";
        dbConnection.Execute(query, allergyID);
    }

    public void RemoveUserAllergies(int allergyID)
    {
        string query = "DELETE FROM UserAllergies WHERE AllergyID = ?";
        dbConnection.Execute(query, allergyID);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        Application.quitting += () =>
        {
            if (DatabaseManager.Instance != null)
            {
                DatabaseManager.Instance.CloseDatabase();
            }
        };
    }

    public void CloseDatabase()
    {
        if (dbConnection != null)
        {
            Debug.Log("Closing database connection.");
            dbConnection.Close();
            dbConnection.Dispose();
            dbConnection = null; // 接続を null に設定して再利用を防ぐ
        }
        else
        {
            Debug.LogWarning("Database connection is already closed or null.");
        }
    }
}

// レシピデータモデル
public class Recipe
{
    public int RecipeID { get; set; }
    public string RecipeName { get; set; }
    public string ImagePath { get; set; }
    public string Details { get; set; }
}


