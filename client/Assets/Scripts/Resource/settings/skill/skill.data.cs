
namespace game.resource.settings.skill
{
    public class Data
    {
        protected Skill self;
        protected resource.Map map;
        protected skill.SkillSetting skillSetting;
        protected skill.MissileSetting missileSetting;

        protected bool HasValidMissileSetting()
        {
            return this.missileSetting != null && this.missileSetting.IsValid();
        }
    }
}
