
namespace game.resource.settings.skill
{
    public class Texture : skill.texture.Updates
    {
        public Texture()
        {
            this.spr = new texture.SPR();
            this.position = new texture.Position();
            this.cache = new texture.Cache();
            this.appearance = new texture.Appearance();

            this.updateRemaining = 0;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public UnityEngine.GameObject GetAppearance() => this.appearance.parent;

        public void SetSprPath(string sprPath) => this.spr.path = sprPath ?? string.Empty;

        public string GetSprPath() => this.spr.path;

        public void SetSprFrame(ushort frameIndex) => this.spr.frameIndex = frameIndex;

        public ushort GetSprFrame() => this.spr.frameIndex;

        public void SetMapPosition(int top, int left)
        {
            this.position.map.top = top;
            this.position.map.left = left;

            this.position.scene.y = top / -100f;
            this.position.scene.x = left / 100f;
        }

        public void SetMapPosition(map.Position position) => this.SetMapPosition(position.top, position.left);

        public map.Position GetMapPosition() => this.position.map;

        public UnityEngine.Vector3 GetScenePosition() => this.position.scene;

        public void Apply() => this.updateRemaining += 1;

        ////////////////////////////////////////////////////////////////////////////////

        public void Initialize()
        {
            this.appearance.Initialize();
        }

        public void Release()
        {
            this.appearance.Release();
        }
    }
}
