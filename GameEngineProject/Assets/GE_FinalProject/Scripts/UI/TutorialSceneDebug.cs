using UnityEngine;

/// <summary>
/// Tutorial Scene에서 플레이어가 제대로 로드되었는지 디버그하는 스크립트
/// </summary>
public class TutorialSceneDebug : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== TUTORIAL SCENE DEBUG START ===");

        // 씬에 있는 모든 플레이어 찾기
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"[TutorialDebug] Found {players.Length} Player(s) in scene");

        for (int i = 0; i < players.Length; i++)
        {
            GameObject p = players[i];
            Debug.Log($"[TutorialDebug] Player {i}: Name='{p.name}', Active={p.activeSelf}, Position={p.transform.position}");

            // 컴포넌트 확인
            PlayerController pc = p.GetComponent<PlayerController>();
            Debug.Log($"[TutorialDebug]   - Has PlayerController: {pc != null}");

            PersistentPlayer pp = p.GetComponent<PersistentPlayer>();
            Debug.Log($"[TutorialDebug]   - Has PersistentPlayer: {pp != null}");

            // 부모 확인
            if (p.transform.parent != null)
            {
                Debug.Log($"[TutorialDebug]   - Parent: {p.transform.parent.name}");
            }
            else
            {
                Debug.Log($"[TutorialDebug]   - No parent (DontDestroyOnLoad?)");
            }
        }

        // 이름으로 플레이어 찾기
        GameObject playerByName = GameObject.Find("Player");
        if (playerByName != null)
        {
            Debug.Log($"[TutorialDebug] Found Player by name: {playerByName.name}");
        }
        else
        {
            Debug.LogWarning("[TutorialDebug] No Player found by name!");
        }

        Debug.Log("=== TUTORIAL SCENE DEBUG END ===");
    }

    private void Update()
    {
        // 매 프레임마다 플레이어 수 확인 (첫 5초만)
        if (Time.timeSinceLevelLoad < 5f && Time.frameCount % 60 == 0) // 1초마다
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            Debug.Log($"[TutorialDebug] Frame {Time.frameCount}: {players.Length} Player(s) in scene");
        }
    }
}
