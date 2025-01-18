using UnityEngine;

public class button_help : MonoBehaviour
{
    // 管理するパネルをそれぞれ登録
    public GameObject Panel_Help;
    public GameObject Panel_Account;
    public GameObject Panel_Allergies;

    /// <summary>
    /// ボタンAが押されたときの処理
    /// </summary>
    public void OnButton_HelpClicked()
    {
        
        Panel_Help.SetActive(true);

        
        Panel_Account.SetActive(false);
        Panel_Allergies.SetActive(false);
    }
}
