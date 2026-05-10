
namespace game.resource.settings.item
{
    public class Defination
    {
        public enum Type
        {
            normalEquip,
            goldEquip,
            platinaEquip,
            normalItem,
            unidentified,
        }

        public enum Genre
        {
            item_equip = 0,
            item_medicine,
            item_mine,
            item_materials,
            item_task,
            item_townportal,
            item_fusion,
            item_number,
        }

        public enum Detail
        {
            equip_meleeweapon = 0,
            equip_rangeweapon,
            equip_armor,
            equip_ring,
            equip_amulet,
            equip_boots,
            equip_belt,
            equip_helm,
            equip_cuff,
            equip_pendant,
            equip_horse,
            equip_mask,
            equip_pifeng,
            equip_yinjian,
            equip_shiping,
            equip_detailnum,
        }

        public enum Series
        {
            metal = 0,
            wood,
            water,
            fire,
            earth,
        }
    }
}
