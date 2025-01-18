using UnityEngine;

public class button_allergies : MonoBehaviour
{
    // 管理するパネルをそれぞれ登録
    public GameObject Panel_Help;
    public GameObject Panel_Account;
    public GameObject Panel_Allergies;

    /// <summary>
    /// ボタンAが押されたときの処理
    /// </summary>
    public void OnButton_AllergiesClicked()
    {
        
        Panel_Allergies.SetActive(true);

        Panel_Help.SetActive(false);
        Panel_Account.SetActive(false);
    }
}
