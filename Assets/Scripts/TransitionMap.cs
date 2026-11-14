using UnityEngine;
using UnityEngine.SceneManagement;
public class TransitionMap : MonoBehaviour
{

    [SerializeField] private string targetSceneName; // 이동할 씬 이름
    [SerializeField] private Vector2 playerSpawnPosition; // 다음 씬에서 플레이어가 나타날 좌표

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 다음 씬에서 플레이어 위치를 전달하기 위한 정적(Static) 데이터 저장 클래스 활용 등 가능
            SpawnManager.Instance.nextSpawnPosition = playerSpawnPosition;
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
