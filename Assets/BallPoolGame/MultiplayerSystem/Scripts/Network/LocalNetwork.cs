using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;

public class LocalNetwork : NetworkEngine
{
    public override void Inicialize()
    {

    }
    public override void Disconnect()
    {
        
    }
    public override void SendRemoteMessage(string message, params object[] args)
    {
        
    }
    public override void OnGoToPLayWithPlayer(PlayerProfile player)
    {
       
    }

    protected override void Update()
    {
        
    }
    public override void OnSendTime(float time01)
    {

    }
    public override void Connect()
    {
        
    }

    public override void CreateRoom()
    {

    }
    public override void LeftRoom()
    {

    }
    public override void Resset()
    {

    }
    public override void StartSimulate(string ballsState)
    {

    }
    public override void EndSimulate(string ballsState)
    {

    }

    public override void OnOpponenReadToPlay(string playerData, bool is3DGraphicMode)
    {

    }
    public override void OnOpponenStartToPlay(int turnId)
    {

    }
    public override void OnOpponenInGameScene()
    {

    }
    public override void OnOpponentForceGoHome()
    {

    }
    public override void StartUpdatePlayers()
    {

    }
    public override void LoadPlayers(ref PlayerProfile[] players)
    {
        players = null;
    }
    public override bool ChackIsFriend(string id)
    {
        return false;
    }
}
