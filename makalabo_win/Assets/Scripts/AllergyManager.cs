using UnityEngine;
using UnityEngine.UI;

public class AllergyManager : MonoBehaviour
{
    public GameObject AllergyPanel;
    public Toggle[] AllergyToggles;
    public Button backButton;
    public Button allergyButton;

    // アレルギー名の配列
    private int[] allergyID = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 }; // 各アレルギーに対応するID

    void Start()
    {
        allergyButton.onClick.AddListener(ShowAllergyPanel); //AllergyButtonにクリックイベントを登録
        backButton.onClick.AddListener(HideAllergyPanel); //Backボタンにクリックイベントを登録
        AllergyPanel.SetActive(false); //初期状態でAllergyPanelは非表示

        // トグルの状態変更時に呼び出すメソッドを設定
        for (int i = 0; i < AllergyToggles.Length; i++)
        {
            int index = i; // ローカル変数でインデックスをキャプチャ
            AllergyToggles[i].onValueChanged.AddListener((isOn) => ToggleAllergyByID(allergyID[index]));
        }
    }

    public void ToggleAllergyByID(int allergyID)
    {
        if (AllergyToggles[allergyID - 1].isOn)
        {
            DatabaseManager.Instance.AddUserAllergies(allergyID); // IDで登録
        }
        else
        {
            DatabaseManager.Instance.RemoveUserAllergies(allergyID); // IDで削除
        }
    }

    public void ShowAllergyPanel()
    {
        AllergyPanel.SetActive(true);
    }    

    public void HideAllergyPanel()
    {
        AllergyPanel.SetActive(false);
    }
}