using UnityEngine;

public class button_setting : MonoBehaviour
{
    // 管理するパネルをそれぞれ登録
    public GameObject Panel_MainMenu;
    public GameObject Panel_Favorite;
    public GameObject Panel_Setting;
    public GameObject Panel_Help;
    public GameObject Panel_Account;
    public GameObject Panel_Allergies;
    public GameObject Panel_Details;


    /// <summary>
    /// ボタンAが押されたときの処理
    /// </summary>
    public void OnButton_SettingClicked()
    {
        
        Panel_Setting.SetActive(true);

        Panel_MainMenu.SetActive(false);
        Panel_Favorite.SetActive(false);
        Panel_Help.SetActive(false);
        Panel_Account.SetActive(false);
        Panel_Allergies.SetActive(false);
        Panel_Details.SetActive(false);

    }
}
