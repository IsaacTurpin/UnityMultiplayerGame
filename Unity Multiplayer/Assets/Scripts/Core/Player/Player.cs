using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.Collections;
using System;

public class Player : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private SpriteRenderer minimapIconRenderer;
    [field: SerializeField] public Health Health {  get; private set; }
    [field: SerializeField] public CoinWallet Wallet { get; private set; }

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;
    [SerializeField] private Color ownerColour = Color.cyan;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

            PlayerName.Value = userData.userName;

            OnPlayerSpawned?.Invoke(this);
        }

        if(IsOwner)
        {
            virtualCamera.Priority = ownerPriority;

            minimapIconRenderer.color = ownerColour;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
