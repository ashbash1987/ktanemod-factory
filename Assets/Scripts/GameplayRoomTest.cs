using UnityEngine;

[ExecuteInEditMode()]
public class GameplayRoomTest : MonoBehaviour
{
    private static readonly Vector3 PLAYER_SIZE = new Vector3(0.5f, 1.6f, 0.5f);
    private static readonly Color PLAYER_COLOR = new Color(0.2f, 0.5f, 1.0f, 0.5f);

    private static readonly Vector3 BOMB_SIZE = new Vector3(0.4f, 0.1f, 0.3f);
    private static readonly Color BOMB_COLOR = new Color(1.0f, 0.5f, 0.2f, 0.5f);
    private static readonly Color MULTIPLE_BOMB_COLOR = new Color(1.0f, 0.5f, 0.7f, 0.2f);

    private static readonly Vector3 ALARM_SIZE = new Vector3(0.15f, 0.1f, 0.05f);
    private static readonly Color ALARM_COLOR = new Color(1.0f, 0.2f, 1.0f, 0.5f);

    private static readonly Vector3 DOSSIER_SIZE = new Vector3(0.2f, 0.02f, 0.3f);
    private static readonly Color DOSSIER_COLOR = new Color(1.0f, 1.0f, 0.2f, 0.5f);

    private KMGameplayRoom _gameplayRoom = null;

    private void Awake()
    {
    }

    private void OnDrawGizmos()
    {
        if (_gameplayRoom == null)
        {
            _gameplayRoom = FindObjectOfType<KMGameplayRoom>();
            if (_gameplayRoom == null)
            {
                return;
            }
        }

        if (_gameplayRoom.PlayerSpawnPosition != null)
        {
            Gizmos.color = PLAYER_COLOR;

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = _gameplayRoom.PlayerSpawnPosition.localToWorldMatrix;
            Gizmos.DrawCube(new Vector3(0.0f, PLAYER_SIZE.y * 0.5f, 0.0f), PLAYER_SIZE);
            Gizmos.matrix = oldMatrix;
        }

        if (_gameplayRoom.BombSpawnPosition != null)
        {
            Gizmos.color = BOMB_COLOR;

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = _gameplayRoom.BombSpawnPosition.localToWorldMatrix;
            Gizmos.DrawCube(new Vector3(0.0f, BOMB_SIZE.y * 0.5f, 0.0f), BOMB_SIZE);
            Gizmos.matrix = oldMatrix;
        }

        int multipleBombIndex = 0;
        GameObject multipleBomb = null;
        do
        {
            Gizmos.color = MULTIPLE_BOMB_COLOR;

            multipleBomb = GameObject.Find(string.Format("MultipleBombs_Spawn_{0}", multipleBombIndex));
            if (multipleBomb != null)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = multipleBomb.transform.localToWorldMatrix;
                Gizmos.DrawCube(new Vector3(0.0f, BOMB_SIZE.y * 0.5f, 0.0f), BOMB_SIZE);
                Gizmos.matrix = oldMatrix;
            }

            multipleBombIndex++;
        }
        while (multipleBomb != null);

        if (_gameplayRoom.AlarmClockSpawn != null)
        {
            Gizmos.color = ALARM_COLOR;

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = _gameplayRoom.AlarmClockSpawn.localToWorldMatrix;
            Gizmos.DrawCube(new Vector3(0.0f, ALARM_SIZE.y * 0.5f, 0.0f), ALARM_SIZE);
            Gizmos.matrix = oldMatrix;
        }

        if (_gameplayRoom.DossierSpawn != null)
        {
            Gizmos.color = DOSSIER_COLOR;

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = _gameplayRoom.DossierSpawn.localToWorldMatrix;
            Gizmos.DrawCube(new Vector3(0.0f, DOSSIER_SIZE.y * 0.5f, 0.0f), DOSSIER_SIZE);
            Gizmos.matrix = oldMatrix;
        }
    }
}
