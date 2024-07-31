using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private Player playerPrefab;
    [SerializeField] private float keptCoinPercentage;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player player in players)
        {
            HandlePlayerSpawned(player);
        }

        Player.OnPlayerSpawned += HandlePlayerSpawned;
        Player.OnPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        Player.OnPlayerSpawned -= HandlePlayerSpawned;
        Player.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(Player player)
    {
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawned(Player player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(Player player)
    {
        int keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));

        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
    {
        yield return null;

        Player playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        playerInstance.Wallet.TotalCoins.Value += keptCoins;
    }
}
