using SQLite;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; 


public class RecipeManager2 : MonoBehaviour
{
    private DatabaseManager dbManager;

    public GameObject recipeItemPrefab; // レシピのUI Prefab
    public GameObject bookmarkItemPrefab;
    public GameObject Panel_MainMenu;
    public Transform contentParent;    // ScrollViewのContent
    public Button refreshButton;       // 更新ボタン
    public InputField searchInputField;
    public Button searchButton;

    public Sprite check_1;  // 空の星アイコン
    public Sprite check_2; // 塗りつぶしアイコン

    public GameObject bookmarkPanel; // ブックマーク専用パネル
    public Transform bookmarkContent; // ScrollViewのContent
    public Button backButton; // 戻るボタン

    public Toggle[] allergyToggles;
    
    public class RecipeUIState
    {
        public int RecipeID { get; set; }
        public bool IsBookmarked { get; set; }
        public Image BookmarkIcon { get; set; }
    }

    void Start()
    {
        // データベースのパス
        string dbPath = "Assets/StreamingAssets/makalabo_db.db";
        dbManager = new DatabaseManager();
        dbManager.InitializeDatabase(dbPath);

        // 初期レシピを表示
        DisplayRandomRecipe();

        if (searchButton !=null)
        {
            searchButton.onClick.AddListener(OnSearchButtonClicked);
        }

        // 更新ボタンにイベントを登録
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshRecipe);
        }

        ///Bookmark関係///
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseBookmarkPanel);
        }

        bookmarkPanel.SetActive(false);

        ///bookmark関係終了///
    }

    void OnApplicationQuit()
    {
        if (dbManager != null)
        {
            dbManager.CloseDatabase();
            Debug.Log("Database connection closed on application quit.");
        }
    }

    private void OnSearchButtonClicked()
    {
        // 入力フィールドから検索キーワードを取得
        string keyword = searchInputField.text;

        if (!string.IsNullOrEmpty(keyword))
        {
            // データベースを再接続
            dbManager.InitializeDatabase("Assets/StreamingAssets/makalabo_db.db");

            // アレルギー除外トグルの状態を取得
            bool excludeAllergies = allergyToggles.Any(t => t.isOn); // いずれかのトグルがONならtrue

            // 検索メソッドの選択
            List<Recipe> searchResults = excludeAllergies
                ? dbManager.SearchRecipeExcludingAllergies(keyword) // アレルギーを除外した検索
                : dbManager.SearchRecipe(keyword);                 // 通常の検索

            // 検索結果をUIに表示
            DisplayRecipe(searchResults, contentParent);

            // 必要であればデータベースをクローズ（任意）
            // dbManager.CloseDatabase();
        }
        else
        {
            Debug.LogWarning("検索キーワードが空です。");
        }
    }


    // ランダムなレシピを取得して表示
    private void DisplayRandomRecipe()
    {
        List<Recipe> randomRecipe = dbManager.GetRandomRecipe(4);
        DisplayRecipe(randomRecipe, contentParent);
    }

    // 検索結果またはランダム取得結果を表示
    private void DisplayRecipe(List<Recipe> recipes, Transform contentParent)
    {
        // Content内の既存アイテムを削除
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var recipeData in recipes)  // 'recipe'を'recipeData'に変更
        {
            // レシピUIアイテムの生成
            GameObject recipeItem = Instantiate(recipeItemPrefab, contentParent);

            // UI要素を取得
            Text recipeNameText = recipeItem.transform.Find("RecipeName").GetComponent<Text>();
            Text recipeDetailText = recipeItem.transform.Find("RecipeDetail").GetComponent<Text>();
            if (recipeNameText != null) recipeNameText.text = recipeData.RecipeName;
            if (recipeDetailText != null) recipeDetailText.text = recipeData.Details;

            // アイコンやボタンの初期設定
            Image recipeImage = recipeItem.transform.Find("RecipeImage").GetComponent<Image>();
            if (recipeImage != null)
            {
                StartCoroutine(LoadImage(recipeData.ImagePath, recipeImage));
            }

            Button bookmarkButton = recipeItem.transform.Find("BookmarkButton").GetComponent<Button>();
            Image bookmarkIcon = bookmarkButton.GetComponent<Image>();

            // ステートを初期化
            RecipeUIState uiState = new RecipeUIState
            {
                RecipeID = recipeData.RecipeID,
                IsBookmarked = dbManager.IsBookmarked(recipeData.RecipeID),
                BookmarkIcon = bookmarkIcon
            };

            UpdateBookmarkIcon(uiState.BookmarkIcon, uiState.IsBookmarked); // アイコンを初期状態に更新

            // ボタンのクリックイベントを登録
            bookmarkButton.onClick.AddListener(() =>
            {
                // ブックマーク状態を切り替える
                if (uiState.IsBookmarked)
                {
                    dbManager.RemoveBookmark(uiState.RecipeID);
                }
                else
                {
                    dbManager.AddBookmark(uiState.RecipeID);
                }

                // 状態を更新
                uiState.IsBookmarked = !uiState.IsBookmarked;
                UpdateBookmarkIcon(uiState.BookmarkIcon, uiState.IsBookmarked);
            });

            // レシピアイテム全体のクリックイベントを登録
            Button button = recipeItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnRecipeItemClick(recipeData));
            }
        }
    }


    private void UpdateBookmarkIcon(Image icon, bool isBookmarked)
    {
        // 塗りつぶしアイコンと空のアイコンを切り替える
        icon.sprite = isBookmarked ? check_2 : check_1;
    }
    
    

    ////// ブックマーク表示 //////
    public void DisplayBookmarkPanel()
    {
        // パネルをアクティブ化
        bookmarkPanel.SetActive(true);

        // Content内の既存アイテムを削除
        foreach (Transform child in bookmarkContent)
        {
            Destroy(child.gameObject);
        }

        // ブックマークされたレシピIDを取得
        List<int> bookmarkedRecipeIDs = dbManager.GetAllBookmarkedRecipe();

        // レシピ情報をUIに表示
        foreach (int id in bookmarkedRecipeIDs)
        {
            Recipe recipe = dbManager.GetRecipeByID(id);
            if (recipe == null)
            {
                Debug.LogError($"Recipe with ID {id} not found in database.");
                continue;
            }

            Debug.Log($"Recipe ID: {recipe.RecipeID}, Name: {recipe.RecipeName}, ImagePath: {recipe.ImagePath}");

            GameObject bookmarkItem = Instantiate(bookmarkItemPrefab, bookmarkContent);

            // UI要素の設定
            Text bookmarkNameText = bookmarkItem.transform.Find("BookmarkName").GetComponent<Text>();
            Text bookmarkDetailText = bookmarkItem.transform.Find("BookmarkDetail").GetComponent<Text>();
            if (bookmarkNameText != null) bookmarkNameText.text = recipe.RecipeName;
            if (bookmarkDetailText != null) bookmarkDetailText.text = recipe.Details;

            // デバッグ: ImagePath をログ出力
            Debug.Log($"Recipe ID: {id}, Name: {recipe.RecipeName}, ImagePath: {recipe.ImagePath}");

            // 画像のロード
            Image bookmarkImage = bookmarkItem.transform.Find("BookmarkImage")?.GetComponent<Image>();
            if (bookmarkImage != null)
            {
                if (!string.IsNullOrEmpty(recipe.ImagePath))
                {
                    StartCoroutine(LoadImage(recipe.ImagePath, bookmarkImage));
                }
                else
                {
                    Debug.LogError($"ImagePath is empty or null for recipe ID: {id}");
                }
            }
            else
            {
                Debug.LogError("BookmarkImage not found in BookmarkItemPrefab.");
            }
    
        
            // ブックマーク削除ボタンの設定
            Button removeButton = bookmarkItem.transform.Find("BookmarkDeleteButton").GetComponent<Button>();
            removeButton.onClick.AddListener(() =>
            {
                dbManager.RemoveBookmark(recipe.RecipeID);
                DisplayBookmarkPanel(); // UIを再描画
            });

            // レシピ詳細表示
            Button button = bookmarkItem.GetComponent<Button>();
            button.onClick.AddListener(() => OnRecipeItemClick(recipe));
        }

        // 戻るボタンの設定
        Transform backButtonTransform = bookmarkPanel.transform.Find("BackButton");
        if (backButtonTransform != null)
        {
            Button backButton = backButtonTransform.GetComponent<Button>();
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() =>
                {
                    bookmarkPanel.SetActive(false); // パネルを閉じる
                    Panel_MainMenu.SetActive(true); //メインパネルの展開
                });
            }
            else
            {
                Debug.LogError("BackButton component not found in bookmarkPanel.");
            }
        }
        else
        {
            Debug.LogError("BackButton not found in bookmarkPanel.");
        }
    }


    private void CloseBookmarkPanel() ///bookmarkパネルを閉じる処理 ///
    {
        bookmarkPanel.SetActive(false);
    }


    // 更新ボタンを押したときに呼ばれるメソッド
    public void RefreshRecipe()
    {
        dbManager.InitializeDatabase("Assets/StreamingAssets/makalabo_db.db"); // 再接続
        DisplayRandomRecipe();  // レシピを更新
        //dbManager.CloseDatabase();
    }

    private void OnRecipeItemClick(Recipe recipe)
    {
        // DetailManagerに詳細を表示するよう依頼
        DetailManager detailManager = FindObjectOfType<DetailManager>();
        if (detailManager != null)
        {
            detailManager.ShowRecipeDetails(recipe);
        }
        else
        {
            Debug.LogError("DetailManagerがシーンに存在しません");
        }
    }

    // 画像を非同期で読み込む（既存のコルーチンを再利用）
    private IEnumerator LoadImage(string imagePath, Image image)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("Image path is null or empty.");
            yield break; // 画像の読み込みを停止
        }

        Debug.Log("RecipeManager最下部 Loading image for bookmark: " + imagePath);
        string resourcePath = System.IO.Path.GetFileNameWithoutExtension(imagePath); // 拡張子を削除
        ResourceRequest request = Resources.LoadAsync<Texture2D>(resourcePath);
        yield return request;

        if (request.asset != null)
        {
            Texture2D texture = request.asset as Texture2D;
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Debug.Log("Image successfully loaded!");
        }
        else
        {
            Debug.LogError("Failed to load image: " + imagePath);
        }
    }
}

