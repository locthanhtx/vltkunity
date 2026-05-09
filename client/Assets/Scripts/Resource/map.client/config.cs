
namespace game.resource.map
{
    public struct Config
    {
        public struct Textures
        {
            /**
            *	@brief bán kính tầm nhìn xung quanh vị trí hiển thị bản đồ theo chiều ngang
            *	tính bằng pixel
            */
            public int radiusHorizontalVisibility;

            /**
            *	@brief bán kính tầm nhìn xung quanh vị trí hiển thị theo chiều dọc
            *	tính bằng pixel
            */
            public int radiusVerticalVisibility;

            /**
            *	@brief số node/region cần preload thêm quanh vùng nhìn hiện tại
            */
            public int nodePrefetchRadius;

            /**
            *	@brief vẽ nền đất của bản đồ
            *	@param 1: có
            *	@param 0: không
            */
            public int drawGroundNode;

            /**
            *	@brief vẽ object thuộc nền đất của bản đồ
            *	@param 1: có
            *	@param 0: không
            */
            public int drawGroundObject;

            /**
            *	@brief vẽ nhà cửa
            *	@param 1: có
            *	@param 0: không
            */
            public int drawBuilding;

            /**
            *	@brief vẽ cây cối
            *	@param 1: có
            *	@param 0: không
            */
            public int drawTree;

            /**
            *	@brief vẽ lưới chướng ngại vật
            *	@param 1: có
            *	@param 0: không
            */
            public int drawObstacleGrid;
        }

        public struct Identification
        {
            /**
            *   @brief danh hiệu nhân vật
            */
            public bool npcTitle;

            /**
            *   @brief tên bang hội, danh hiệu bang hội của nhân vật
            */
            public bool npcTong;

            /**
            *   @brief tên nhân vật và hệ của nhân vật nếu có
            */
            public bool npcName;

            /**
            *   @brief thanh máu nhân vật
            */
            public bool npcHealth;

            /**
            *  @brief vị trí bản đồ của nhân vật
            */
            public bool npcMapPos;
        }
    }
}
