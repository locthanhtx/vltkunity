using System;
using System.Collections.Generic;

namespace game.resource
{
    public struct ObjSpr
    {
        public string Name;
        public byte Layer;
        public int TotalFrame;
        public int CurFrame;
        public int TotalDir;
        public int CurDir;
    }

    struct Cache
    {
        public static IntPtr resourcePackageHandler;

        public struct Settings
        {
            public struct Music
            {
                // [map.id] => music file path
                public static Dictionary<int, string> musicset;
            }

            public struct MagicDesc
            {
                public static Dictionary<int, resource.settings.MagicDesc.Table> id; // [magic.id] => <...>
                public static Dictionary<string, resource.settings.MagicDesc.Table> key; // [magic.key] => <...>
            }

            public struct NpcRes
            {
                public struct Kind
                {
                    public static Dictionary<string, string> mainManHeaderValueMapping; // header_key => value
                    public static Dictionary<string, string> mainLadyHeaderValueMapping; // header_key => value
                }

                public static Dictionary<string, resource.Table> mainManTableMapping; // header_key => table
                public static Dictionary<string, resource.Table> mainLadyTableMapping; // header_key => table

                public static Dictionary<string, resource.Table> mainManPartPropertiesTableMapping; // header_key => table
                public static Dictionary<string, resource.Table> mainLadyPartPropertiesTableMapping; // header_key => table

                public static Dictionary<string, resource.Ini> mainManIniMapping; // header_key => ini
                public static Dictionary<string, resource.Ini> mainLadyIniMapping; // header_key => ini

                public struct Shadow
                {
                    public static Dictionary<string, settings.npcres.Structures.PartSprInfo> mainManAnimationMapping; // animation_name => <...>
                    public static Dictionary<string, settings.npcres.Structures.PartSprInfo> mainLadyAnimationMapping; // animation_name => <...>
                }

                public struct NormalNpc
                {
                    public struct PartInfo
                    {
                        public settings.npcres.Structures.PartSprInfo fullBody;
                        public settings.npcres.Structures.PartSprInfo shadow;
                    }

                    public static Dictionary<string, Dictionary<string, NormalNpc.PartInfo>> animationMapping; // npc_name => action_name => <...>
                }

                public static Dictionary<string, game.resource.settings.skill.texture.SprCache.Data> textures; // <spr_path> => <...>
            }

            public struct Item
            {
                // file => declare_line => npcres_line
                public static Dictionary<string, Dictionary<int, int>> appearanceParsedMapping;

                // ["genre, detail, particular, level"] => <...>
                // items: resource.settings.item.Listing.Equipment
                public static Dictionary<string, settings.item.EquipmentBase> equipmentBaseMapping;

                // ["detail, axmol_record_index"] => <...>
                public static Dictionary<string, settings.item.EquipmentBase> equipmentBaseRowMapping;

                // ["genre, detail, particular"] => <...>
                public static Dictionary<string, settings.item.EquipmentBase> maskEquipBase;

                // ["genre, detail, particular"] => <...>
                public static Dictionary<string, settings.item.SimpleItemBase> itemBaseMapping;

                // ["genre, detail, particular, level"] => <...>
                public static Dictionary<string, settings.item.SimpleItemBase> itemBaseLevelMapping;

                // ["genre, axmol_record_index"] => <...>
                public static Dictionary<string, settings.item.SimpleItemBase> itemBaseRowMapping;

                // ["detail, series, position"] => [propType] => [level] => <...>
                // position :: 0 - hide, 1 - show
                public static Dictionary<string, Dictionary<int, Dictionary<int, settings.item.MagicattribBase>>> magicAttribBaseMapping;

                // [rowIndex] => <...>
                public static Dictionary<int, settings.item.GoldEquipBase> goldEquipBase;

                // [rowIndex] => <...>
                public static Dictionary<int, settings.item.GoldEquipBase> platinaEquipBase;

                // [goldEquipBase.rowIndex> => <...>
                public static Dictionary<int, settings.item.GoldResBase> goldEquipRes;

                // [rowIndex] => <...>
                public static Dictionary<int, settings.item.GoldMagicBase> goldMagicBase;

                // [idSet] => array[(goldEquipBase.rowIndex * 100 + equipmentBase.detail) in set]
                public static Dictionary<int, List<int>> goldEquipSet;

                // ["genre, detail, particular] => <...>
                public static Dictionary<string, settings.item.MagicScriptBase> magicScriptBase;
            }

            public struct ObjData
            {
                public static Dictionary<int, string> declareRowIndexToResTypeMapping; // declare_row_index => NpcResType
                public static Dictionary<string, ObjSpr> declareRowIndexToStatureMapping; // NpcResType => stature
            }

            public struct Npcs
            {
                public static Dictionary<int, string> declareRowIndexToResTypeMapping; // declare_row_index => NpcResType
                public static Dictionary<int, int> declareRowIndexToStatureMapping; // declare_row_index => stature
                public static Dictionary<int, settings.Npcs.MotionProfile> declareRowIndexToMotionProfileMapping; // declare_row_index => movement frames/speed
            }

            public struct Skill
            {
                public static resource.Table skillsTable;
                public static Dictionary<int, int> skillsIdToRowIndexMapping; // skill_id => declare_row_index
                public static Dictionary<int, resource.settings.skill.SkillSetting> skillsIdToDataMapping; // skill_id + skill_level => skill_data

                public static resource.Table missilesTable;
                public static Dictionary<int, int> missilesIdToRowIndexMapping; // missile_id => declare_row_index
                public static Dictionary<int, resource.settings.skill.MissileSetting> missilesIdToDataMapping; // missile_id => missile_data

                public static Dictionary<string, game.resource.settings.skill.texture.SprCache.Data> textures; // spr_path => <...>
                public static Dictionary<int, game.resource.settings.skill.StateSetting.Data> stateMagicTable; // state_id => <...>
            }
        }

        public struct Font
        {
            public static UnityEngine.Font font0;
            public static UnityEngine.Font font1;
            public static UnityEngine.Font font2;
        }
    }
}
