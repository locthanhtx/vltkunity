
using System.Collections.Generic;

namespace game.resource.settings.item
{
    public class Datafield
    {
        protected item.Defination.Type type = Defination.Type.unidentified;

        // dữ liệu nền tảng của vật phẩm
        // lấy từ settings/item/...<equipment>
        protected settings.item.EquipmentBase equipmentBase;

        // dũ liệu nền tảng của vật phẩm
        // lấy từ settings/item/magicscript.txt
        protected settings.item.MagicScriptBase magicScriptBase;

        // dữ liệu nền tảng của vật phẩm không phải trang bị
        // lấy từ potion/mine/questkey/fusion/townportal giống Axmol KBasPropTbl
        protected settings.item.SimpleItemBase simpleItemBase;

        // thuộc tính ma pháp của vật phẩm
        // được tạo ra từ thông số chỉ định
        protected List<settings.skill.SkillSettingData.KMagicAttrib> magicAttrib;

        // mô tả thuộc tính cơ bản
        protected string basicAttribDesc;

        // mô tả toàn bộ thuộc tính ma pháp của vật phẩm
        // lấy từ this.magicAttrib
        protected List<string> magicAttribDesc;

        // id vật phẩm trong database
        protected uint databaseId;

        // [rowindex] => name
        // danh sách các trang bị chung một bộ
        protected Dictionary<int, string> setItemList;

        // hệ ngũ hành của vật phẩm
        // từ thông số chỉ định
        protected int series = 0;

        // đẳng cấp của vật phẩm
        // từ thông số chỉ định
        protected int level = 1;

        // tổng số lượng vật phẩm xếp chống lên nhau
        protected int stack = 1;

        // thời hạn sử dụng đến thời điểm chỉ định, tính bằng unix time seconds 
        // System.DateTimeOffset.UtcNow.ToUnixTimeSeconds
        // hoặc số lần sử dụng còn lại
        // tùy thuộc vào cấu hình của vật phẩm
        protected long timeUse = 0;
    }
}
