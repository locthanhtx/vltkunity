
namespace game.resource.settings.skill.texture
{
    public class Updates : skill.texture.Data
    {
        public void Update()
        {
            if (this.updateRemaining > 0)
            {
                this.updateRemaining--;
            }
            else
            {
                return;
            }

            skill.texture.SprCache.Data.SprFrame sprFrame;
            try
            {
                sprFrame = skill.texture.Cache.GetSprFrame(this.spr.path, this.spr.frameIndex);
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogWarning("Skill SPR update failed: " + this.spr.path +
                                             " frame=" + this.spr.frameIndex +
                                             " error=" + exception.GetBaseException().Message);
                this.appearance.spriteRendererComponent.sprite = null;
                return;
            }

            if(sprFrame == null)
            {
                this.appearance.spriteRendererComponent.sprite = null;
                return;
            }

            this.appearance.parent.transform.position = new UnityEngine.Vector3(this.position.scene.x, this.position.scene.y);
            this.appearance.rectTransformComponent.sizeDelta = sprFrame.sizeDelta;
            this.appearance.rectTransformComponent.anchoredPosition = sprFrame.anchoredPosition;
            this.appearance.spriteRendererComponent.sprite = sprFrame.sprite;
        }
    }
}
