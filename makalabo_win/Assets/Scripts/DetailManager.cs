using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DetailManager : MonoBehaviour
{
    public Text recipeNameText;       // レシピ名表示用
    public Text recipeDetailText;    // レシピの説明表示用
    public Text recipeIngredientsText; // 材料表示用
    public Text recipeStepsText;      // 作成手順表示用
    public Image recipeImage;        // レシピ画像表示用

    public GameObject detailPanel;   // 詳細表示パネル
    public GameObject Panel_Allergies; //アレルギーパネル
    public GameObject Panel_Favorite; //お気に入りパネル
    public GameObject Panel_MainMenu; //メニュ―パネル
    public GameObject Panel_Setting; //設定パネル
    public GameObject Panel_Account; //アカウントパネル
    public GameObject Panel_Help; //ヘルプパネル
    public Button exitButton; //戻るボタン

    void Start()
    {
        // 初期状態でDetailPanelを非表示
        detailPanel.SetActive(false);

        // Exitボタンにイベントリスナーを追加
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
    }

    // レシピ詳細を設定してパネルを表示
    public void ShowRecipeDetails(Recipe recipe)
    {
        Debug.Log("ShowRecipeDetails called with recipe: " + recipe.RecipeName);
        if (recipe == null) return;

        // 各UI要素に値を設定
        recipeNameText.text = recipe.RecipeName;
        recipeDetailText.text = recipe.Details;
        recipeIngredientsText.text = GetIngredientsText(recipe.RecipeID); // 材料取得
        recipeStepsText.text = GetStepsText(recipe.RecipeID); // 作成手順をデータに応じて設定
        StartCoroutine(LoadImage(recipe.ImagePath, recipeImage));

        // パネルを表示
        detailPanel.SetActive(true);
        //詳細以外の全パネルを非表示
        Panel_MainMenu.SetActive(false);
        Panel_Setting.SetActive(false);
        Panel_Favorite.SetActive(false);
        Panel_Allergies.SetActive(false);
        Panel_Account.SetActive(false);
        Panel_Help.SetActive(false);

    }

    private void OnExitButtonClicked()
    {
        // DetailPanelを非表示にする
        detailPanel.SetActive(false);
        Panel_MainMenu.SetActive(true);
    }

    // 材料テキストを取得（DatabaseManagerから情報を取得）
    // 材料テキストを取得（DatabaseManagerから情報を取得）
    private string GetIngredientsText(int recipeID)
    {
        // 材料情報を取得
        List<(string Name, string Quantity, string Unit)> ingredientsWithDetails = DatabaseManager.Instance.GetIngredientsWithDetailsByRecipeID(recipeID);

        // 表示用テキストを生成
        List<string> formattedIngredients = new List<string>();
        foreach (var ingredient in ingredientsWithDetails)
        {
            // 量や単位がない場合を考慮
            string quantity = string.IsNullOrEmpty(ingredient.Quantity) ? "" : ingredient.Quantity;
            string unit = string.IsNullOrEmpty(ingredient.Unit) ? "" : ingredient.Unit;

            string formattedText = $"{ingredient.Name}: {quantity} {unit}".Trim(); // 余計な空白を削除
            formattedIngredients.Add(formattedText);
        }
        return string.Join("\n", formattedIngredients);
    }

    private string GetStepsText(int recipeID)
    {
        // 手順情報を取得
        List<(int StepNumber, string Description)> steps = DatabaseManager.Instance.GetStepsByRecipeID(recipeID);

        // 表示用テキストを生成
        List<string> formattedSteps = new List<string>();
        foreach (var step in steps)
        {
            formattedSteps.Add($"Step {step.StepNumber}: {step.Description}");
        }
        return string.Join("\n\n", formattedSteps);
    }


    // 画像を非同期で読み込む
    private IEnumerator LoadImage(string imagePath, Image image)
    {
        if (string.IsNullOrEmpty(imagePath)) yield break;
        string resourcePath = System.IO.Path.GetFileNameWithoutExtension(imagePath);
        ResourceRequest request = Resources.LoadAsync<Texture2D>(resourcePath);
        yield return request;
        if (request.asset != null && image != null)
        {
            Texture2D texture = request.asset as Texture2D;
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
