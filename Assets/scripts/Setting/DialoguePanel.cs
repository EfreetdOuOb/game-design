using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialoguePanel : MonoBehaviour
{
    public Text dialogueText; // 拖UI Text進來
    public GameObject nextTip; // 「按空白鍵繼續」提示，可選

    public IEnumerator ShowDialogue(string[] lines)
    {
        gameObject.SetActive(true);
        for (int i = 0; i < lines.Length; i++)
        {
            dialogueText.text = lines[i];
            if (nextTip != null) nextTip.SetActive(true);

            // 先等玩家放開按鍵
            yield return new WaitWhile(() => Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0));
            // 再等玩家新一次按下
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

            if (nextTip != null) nextTip.SetActive(false);
        }
        // 等待玩家最後確認（可選）
        dialogueText.text = "按空白鍵或左鍵關閉";
        if (nextTip != null) nextTip.SetActive(true);

        yield return new WaitWhile(() => Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0));
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

        if (nextTip != null) nextTip.SetActive(false);
        gameObject.SetActive(false);
    }
} 