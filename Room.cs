using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;

public class Room : MonoBehaviour
{
    [Header("Room Component")]
    [SerializeField] private GameObject roomTile;
    [SerializeField] private LightSetting lightSetting;
    public EnemyPool roomObject;
    [Header("Cinemachine")]
    public CinemachineVirtualCamera virtualCamera;

    [HideInInspector] public Vector2 pastVelocity= Vector2.zero;
    private Character character;
    private CinemachineVirtualCamera originVC;


    [Serializable]
    private struct LightSetting
    {
        public float globalIntensity;
        public float innerRadius;
        public float outerRadius;
    }
    private void Awake()
    {
        roomTile.SetActive(false);
        originVC = virtualCamera;
    }
    private void CameraReset()
    {
        virtualCamera.Follow = LevelManager.Instance.playerCharacter.transform;
    }
    public void FindPlayer(bool find)
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        
        if (find)
        {
            foreach (Room room in GameManager.Instance.rooms)
            {
                room.FindPlayer(false);
            }
            LevelManager.Instance.currentRoom = this;
            LevelManager.Instance.ChangeLightSettingEvent(lightSetting.globalIntensity, lightSetting.innerRadius, lightSetting.outerRadius);
            character.currentRoom = this;
            roomTile.SetActive(true);
            roomObject.gameObject.SetActive(true);
            virtualCamera.Priority = 10;

            character.characterCondition = Character.CharacterCondition.Stunned;
        }
        else
        {
            virtualCamera.Priority = 9;
        }
    }
    public void ReturnRoomCamera()
    {
        virtualCamera = originVC;
    }
    public void ChangeRoom()
    {
        foreach (Room room in GameManager.Instance.rooms)
        {
            room.SetRoomObject(false);
            room.virtualCamera.Follow = null;
        }

        CameraReset();
    }
    public void SetRoomObject(bool value)
    {
        if (this.roomObject != character.currentRoom.roomObject)
        {
            roomObject.gameObject.SetActive(value);
        }

        if (this.roomTile != character.currentRoom.roomTile)
        {
            roomTile.SetActive(value);
        }
    }
}
