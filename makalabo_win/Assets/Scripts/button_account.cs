using UnityEngine;

public class button_account : MonoBehaviour
{
    // 管理するパネルをそれぞれ登録
    public GameObject Panel_Help;
    public GameObject Panel_Account;
    public GameObject Panel_Allergies;

    /// <summary>
    /// ボタンAが押されたときの処理
    /// </summary>
    public void OnButton_AccountClicked()
    {
        
        Panel_Account.SetActive(true);

        Panel_Help.SetActive(false);
        Panel_Allergies.SetActive(false);
    }
}
