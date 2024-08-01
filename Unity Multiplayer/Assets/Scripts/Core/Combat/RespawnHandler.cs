using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private ZPlayer playerPrefab;
    [SerializeField] private float keptCoinPercentage;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        ZPlayer[] players = FindObjectsByType<ZPlayer>(FindObjectsSortMode.None);
        foreach (ZPlayer player in players)
        {
            HandlePlayerSpawned(player);
        }

        ZPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        ZPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        ZPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        ZPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(ZPlayer player)
    {
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawned(ZPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(ZPlayer player)
    {
        int keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));

        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
    {
        yield return null;

        ZPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        playerInstance.Wallet.TotalCoins.Value += keptCoins;
    }
}
