using System.Collections;
using UnityEngine;

public class RoomFlowController : MonoBehaviour
{
    public enum RoomStep
    {
        WaitForBoxOpen,
        ShowItemTip,
        ShowBattleTip,
        StartBattle,
        WaitForBattleEnd,
        RoomComplete,
        Transition
    }

    public RoomStep currentStep = RoomStep.WaitForBoxOpen;

    [Header("流程物件")]
    public GameObject box;
    public GameObject itemTipPanel;
    public GameObject battleTipPanel;
    public EnemySpawner enemySpawner;
    public float itemTipDuration = 2f;
    public float battleTipDuration = 2f;
    public GameObject boxHighlight; // 可選：高亮寶箱物件

    private bool boxOpened = false;
    private bool battleStarted = false;
    private bool roomCompleted = false;

    private void Start()
    {
        StartCoroutine(RoomFlow());
    }

    private IEnumerator RoomFlow()
    {
        // 1. 高亮寶箱（可選）
        currentStep = RoomStep.WaitForBoxOpen;
        if (boxHighlight != null) boxHighlight.SetActive(true);
        // 等待玩家開啟寶箱
        if (box != null)
        {
            yield return new WaitUntil(() => box.GetComponent<Box>()?.IsOpened == true);
        }
        if (boxHighlight != null) boxHighlight.SetActive(false);

        // 2. 顯示裝備提示
        currentStep = RoomStep.ShowItemTip;
        if (itemTipPanel != null) itemTipPanel.SetActive(true);
        yield return new WaitForSeconds(itemTipDuration);
        if (itemTipPanel != null) itemTipPanel.SetActive(false);

        // 3. 顯示戰鬥提示
        currentStep = RoomStep.ShowBattleTip;
        if (battleTipPanel != null) battleTipPanel.SetActive(true);
        yield return new WaitForSeconds(battleTipDuration);
        if (battleTipPanel != null) battleTipPanel.SetActive(false);

        // 4. 開始戰鬥
        currentStep = RoomStep.StartBattle;
        if (enemySpawner != null)
        {
            enemySpawner.gameObject.SetActive(true);
            battleStarted = true;
        }

        // 5. 等待戰鬥結束
        currentStep = RoomStep.WaitForBattleEnd;
        yield return new WaitUntil(() => enemySpawner != null && enemySpawner._allWavesCompleted && GameObject.FindObjectsOfType<Enemy>().Length == 0);

        // 6. 房間完成
        currentStep = RoomStep.RoomComplete;
        roomCompleted = true;
        // 可呼叫 UIManager 顯示房間完成UI
        UIManager.Instance?.ShowRoomCompleteMenu();
        yield return new WaitForSeconds(1.5f);

        // 7. 過場/切換房間
        currentStep = RoomStep.Transition;
        LevelFlowController.Instance?.OnLevelCompleted();
    }
} 