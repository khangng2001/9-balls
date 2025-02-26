using NetworkManagement;

/// <summary>
/// Network and game adapter.
/// </summary>
public interface NetworkGameAdapter
{
    HomeMenuManager homeMenuManager
    {
        get;
    }
    void SetTurn(int turnId);
    void OnMainPlayerLoaded (int playerId, string name, int coins, object avatar, string avatarURL, int prize);
	void OnUpdateMainPlayerName (string name);
	void OnUpdatePrize (int prize);
    void GoToReplay();
    void GoToReplayFromSharedData();
	void OnGoToPlayWithAI (int playerId, string name, int coins, object avatar, string avatarURL);
	void OnGoToPlayHotSeatMode (int playerId, string name, int coins, object avatar, string avatarURL);
    void OnGoToPLayWithPlayer (PlayerProfile player);
}
