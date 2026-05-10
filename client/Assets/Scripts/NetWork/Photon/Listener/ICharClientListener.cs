namespace game.network.listener
{
    public interface ICharClientListener
    {
        public void ChangeWorld();
        public CharacterClick FindPlayer(int id);
        public PlayerClick SpwanPlayer(int id, string name, bool sex, int dir, int mapX, int mapY);
        public void UpdateNpc(game.resource.settings.npcres.Controller npcController, int top, int left, bool isMain);
        public void DelPlayer(int id);
    }
}

