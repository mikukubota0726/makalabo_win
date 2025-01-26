using UnityEngine;
using UnityEngine.UI;

public class HelpPanelController : MonoBehaviour
{
    public GameObject helpPanel;  // 各プルダウンパネルを参照
    public Button toggleButton;   // 各ボタンを参照
    public GameObject helpContent; // HelpContentオブジェクトを参照
    //public Button topItemButton;  // 一番上の項目ボタン

    private bool isPanelVisible = false;

    void Start()
    {
        // ボタンのクリックイベントにメソッドを登録
        toggleButton.onClick.AddListener(TogglePanelVisibility);
        helpPanel.SetActive(false); // 初期状態ではパネルを非表示にする

        // 一番上の項目（ボタン）を固定する
        //SetTopItemFixed();
    }

    void TogglePanelVisibility()
    {
        // パネルの表示/非表示を切り替える
        isPanelVisible = !isPanelVisible;
        helpPanel.SetActive(isPanelVisible);

        // HelpContentのサイズを調整して、他のボタンが下に移動するようにする
        LayoutRebuilder.ForceRebuildLayoutImmediate(helpContent.GetComponent<RectTransform>());
    }
}
