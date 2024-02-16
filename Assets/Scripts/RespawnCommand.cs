using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RespawnCommand : BetweenSceneManager.ICommand
{
    private EntityPiece curPiece;
    private MapNode spawnNode;

    public RespawnCommand(EntityPiece p)
    {
        curPiece = p;
    }

    public void Execute()
    {
        spawnNode = GameObject.FindWithTag("InputManager").GetComponent<GameplayTest>().spawnNode;
        curPiece.gameObject.GetComponent<PlayerStats>().curHealth = curPiece.gameObject.GetComponent<PlayerStats>().maxHealth;
        curPiece.occupiedNode = spawnNode;
        curPiece.transform.DOMove(spawnNode.transform.position, .25f)
            .SetEase(Ease.OutQuint);
    }
}
