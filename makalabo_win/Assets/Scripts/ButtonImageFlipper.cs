using UnityEngine;
using UnityEngine.UI;

public class ButtonImageFlipper : MonoBehaviour
{
    public Image buttonImage; // 反転させたいボタンのImageコンポーネント
    private bool isFlipped = false; // 反転状態を記録

    void Start()
    {
        // 初期状態では反転しない
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>(); // 自動的にImageコンポーネントを取得
        }
    }

    // ボタンがクリックされたときに呼び出す
    public void ToggleImageFlip()
    {
        // 反転状態をトグルする
        isFlipped = !isFlipped;

        // 反転処理
        if (isFlipped)
        {
            buttonImage.rectTransform.localScale = new Vector3(1, -1, 1); // Y軸を反転
        }
        else
        {
            buttonImage.rectTransform.localScale = new Vector3(1, 1, 1); // 元の状態に戻す
        }
    }
}
