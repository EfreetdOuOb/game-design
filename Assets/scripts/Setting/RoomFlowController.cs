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
    public GameObject box; // 可為 null
    public GameObject itemTipPanel;
    public GameObject battleTipPanel;
    public EnemySpawner enemySpawner;
    public float itemTipDuration = 2f;
    public float battleTipDuration = 2f;
    public GameObject door; // 新增：對應大門
    public GameObject roomCompletePanel; // 新增：房間完成UI

    private bool hasStarted = false;

    private void Start()
    {
        StartCoroutine(RoomFlow());
    }

    public void StartRoomFlow()
    {
        if (hasStarted) return;
        hasStarted = true;
        StartCoroutine(RoomFlow());
    }

    private IEnumerator RoomFlow()
    {
        // 1. 等待玩家開啟寶箱（如果有寶箱）
        currentStep = RoomStep.WaitForBoxOpen;
        if (box != null)
        {
            yield return new WaitUntil(() => box.GetComponent<Box>()?.IsOpened == true);
        }

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
            enemySpawner.StartSpawning();
        }

        // 5. 等待戰鬥結束
        currentStep = RoomStep.WaitForBattleEnd;
        yield return new WaitUntil(() => enemySpawner != null 
            && enemySpawner._allWavesCompleted 
            && GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0);

        // 6. 房間完成
        currentStep = RoomStep.RoomComplete;
        if (door != null)
        {
            var doorComp = door.GetComponent<Door>();
            if (doorComp != null)
            {
                doorComp.Open();
            }
        }
        if (roomCompletePanel != null) roomCompletePanel.SetActive(true); // 顯示房間完成UI
        UIManager.Instance?.ShowRoomCompleteMenu();
        yield return new WaitForSeconds(1.5f);
        if (roomCompletePanel != null) roomCompletePanel.SetActive(false); // 自動隱藏

        // 7. 過場/切換房間
        currentStep = RoomStep.Transition;
        LevelFlowController.Instance?.OnLevelCompleted();
    }
} 