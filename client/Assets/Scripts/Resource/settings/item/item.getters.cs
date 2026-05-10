
using System.Collections.Generic;

namespace game.resource.settings.item
{
    public class Getter : item.Setters
    {
        private static UnityEngine.Sprite itemThumbnailUnidentifiedSprite;
        private static UnityEngine.Sprite itemFramedTypeWhiteSprite;
        private static UnityEngine.Sprite itemFramedTypeBlueSprite;
        private static UnityEngine.Sprite itemFramedTypeGreenSprite;
        private static readonly bool EnableNativeItemThumbnailSprites = false;
        private static readonly Dictionary<string, UnityEngine.Sprite> itemGeneratedSprites = new Dictionary<string, UnityEngine.Sprite>();
        private static readonly Dictionary<string, UnityEngine.Sprite> itemSprSprites = new Dictionary<string, UnityEngine.Sprite>();
        private static readonly HashSet<string> itemSprWarnings = new HashSet<string>();
        private static string[] itemSprExternalRoots;

        public static string GetRichText(string origin)
        {
            if (string.IsNullOrEmpty(origin))
            {
                return string.Empty;
            }

            string result = origin.Replace("<enter>", "\n");
            result = result.Replace("<color>", "</color>");

            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public class GridablePos
        {
            public float cB;
            public float cE;
            public float rB;
            public float rE;

            public void Set(float cB, float cE, float rB, float rE)
            {
                this.cB = cB;
                this.cE = cE;
                this.rB = rB;
                this.rE = rE;
            }
        }

        public Getter.GridablePos GetThumbnailGridable()
        {
            Getter.GridablePos result = new GridablePos();
            result.Set(1, 38, 1, 18);

            int itemStoredSize = 0;

            if (this.equipmentBase != null)
            {
                itemStoredSize = System.Math.Max(this.equipmentBase.height, this.equipmentBase.width);
            }
            else if (this.simpleItemBase != null)
            {
                itemStoredSize = System.Math.Max(this.simpleItemBase.height, this.simpleItemBase.width);
            }
            else if (this.magicScriptBase != null)
            {
                itemStoredSize = System.Math.Max(this.magicScriptBase.height, this.magicScriptBase.width);
            }

            if (itemStoredSize == 1)
            {
                result.Set(12, 27, 6, 13);
            }
            else if (itemStoredSize == 2)
            {
                result.Set(9, 30, 3, 16);
            }

            return result;
        }

        public UnityEngine.Sprite GetThumbnailSprite()
        {
            if (Item.itemThumbnailUnidentifiedSprite == null)
            {
                Item.itemThumbnailUnidentifiedSprite = Game.Resource("\\user.interface\\panel.equipment\\item.tab\\item.unidentified.png").Get<UnityEngine.Sprite>();
                if (Item.itemThumbnailUnidentifiedSprite == null)
                {
                    Item.itemThumbnailUnidentifiedSprite = CreateGeneratedItemSprite("thumbnail.unidentified", ResolveFallbackItemColor(), true);
                }
            }

            string imagePath = this.GetThumbnailImagePath();
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return Item.itemThumbnailUnidentifiedSprite;
            }

            if (imagePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
                imagePath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
                imagePath.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase))
            {
                UnityEngine.Sprite imageSprite = Game.Resource(imagePath).Get<UnityEngine.Sprite>();
                return imageSprite != null ? imageSprite : Item.itemThumbnailUnidentifiedSprite;
            }

            // Item icons use a managed SPR decoder here because native frame APIs crashed on item SPRs.
            UnityEngine.Sprite decodedSprSprite = GetSprThumbnailSprite(imagePath);
            if (decodedSprSprite != null)
            {
                return decodedSprSprite;
            }

            if (!EnableNativeItemThumbnailSprites)
            {
                return CreateGeneratedItemSprite(
                    "thumbnail." + this.GetItemType() + "." + this.GetGenre() + "." + this.GetDetail() + "." + this.GetParticular() + "." + this.GetSeries(),
                    ResolveFallbackItemColor(),
                    true);
            }

            if (this.equipmentBase != null)
            {
                UnityEngine.Sprite sprite;
                if ((sprite = Game.Resource(this.equipmentBase.imagePath).Get<UnityEngine.Sprite>(0)) == null)
                {
                    sprite = Item.itemThumbnailUnidentifiedSprite;
                }

                return sprite;
            }
            else if (this.simpleItemBase != null)
            {
                UnityEngine.Sprite sprite;
                if ((sprite = Game.Resource(this.simpleItemBase.imagePath).Get<UnityEngine.Sprite>(0)) == null)
                {
                    sprite = Item.itemThumbnailUnidentifiedSprite;
                }

                return sprite;
            }
            else if (this.magicScriptBase != null)
            {
                UnityEngine.Sprite sprite;
                if ((sprite = Game.Resource(this.magicScriptBase.image).Get<UnityEngine.Sprite>(0)) == null)
                {
                    sprite = Item.itemThumbnailUnidentifiedSprite;
                }

                return sprite;
            }

            return Item.itemThumbnailUnidentifiedSprite;
        }

        private static UnityEngine.Sprite GetSprThumbnailSprite(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)
                || imagePath.EndsWith(".spr", System.StringComparison.OrdinalIgnoreCase) == false)
            {
                return null;
            }

            string normalizedPath = NormalizeResourcePath(imagePath);
            string cacheKey = normalizedPath.ToLowerInvariant();
            if (itemSprSprites.TryGetValue(cacheKey, out UnityEngine.Sprite cachedSprite))
            {
                return cachedSprite;
            }

            UnityEngine.Sprite sprite = null;
            try
            {
                resource.Buffer buffer = ReadItemSprBuffer(normalizedPath);
                sprite = CreateSpriteFromSpr(buffer, normalizedPath, 0);
            }
            catch (System.Exception exception)
            {
                WarnItemSprOnce(cacheKey, "Item SPR decode exception: " + normalizedPath + " error=" + exception.Message);
            }

            if (sprite == null)
            {
                WarnItemSprOnce(cacheKey, "Item SPR decode failed: " + normalizedPath);
            }

            itemSprSprites[cacheKey] = sprite;
            return sprite;
        }

        private static string NormalizeResourcePath(string imagePath)
        {
            string result = imagePath.Trim().Replace('/', '\\');
            if (result.Length > 0 && result[0] != '\\')
            {
                result = "\\" + result;
            }

            return result;
        }

        private static resource.Buffer ReadItemSprBuffer(string normalizedPath)
        {
            if (resource.packageIni.ManagedPakReader.TryRead(normalizedPath, out resource.Buffer buffer) && buffer.size > 0)
            {
                return buffer;
            }

            return ReadExternalItemSprBuffer(normalizedPath);
        }

        private static resource.Buffer ReadExternalItemSprBuffer(string normalizedPath)
        {
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return null;
            }

            foreach (string root in GetItemSprExternalRoots())
            {
                string filePath = System.IO.Path.Combine(root, normalizedPath.TrimStart('\\'));
                if (System.IO.File.Exists(filePath))
                {
                    return System.IO.File.ReadAllBytes(filePath);
                }
            }

            return null;
        }

        private static string[] GetItemSprExternalRoots()
        {
            if (itemSprExternalRoots != null)
            {
                return itemSprExternalRoots;
            }

            List<string> roots = new List<string>();
            string localRoot = resource.dataController.Config.GetLocalStogareFullPath();
            if (!string.IsNullOrWhiteSpace(localRoot) && System.IO.Directory.Exists(localRoot))
            {
                roots.Add(localRoot);
            }

            System.IO.DirectoryInfo directory = System.IO.Directory.GetParent(UnityEngine.Application.dataPath);
            for (int depth = 0; directory != null && depth < 8; depth++, directory = directory.Parent)
            {
                string cocosRoot = System.IO.Path.Combine(directory.FullName, "cocos_v3");
                if (!System.IO.Directory.Exists(cocosRoot))
                {
                    continue;
                }

                AddRootIfExists(roots, System.IO.Path.Combine(cocosRoot, "JX1CocosMobile", "pak_file"));
                AddRootIfExists(roots, System.IO.Path.Combine(cocosRoot, "JX1CocosMobile", "pak_file", "kyuctruyenky"));
                AddRootIfExists(roots, System.IO.Path.Combine(cocosRoot, "JX1CocosMobile", "pak_file", "jxphongvan"));
                AddRootIfExists(roots, System.IO.Path.Combine(cocosRoot, "pak_file", "jxmobile"));
                AddRootIfExists(roots, System.IO.Path.Combine(cocosRoot, "pak_file", "jx1m"));
                break;
            }

            itemSprExternalRoots = roots.ToArray();
            return itemSprExternalRoots;
        }

        private static void AddRootIfExists(List<string> roots, string path)
        {
            if (System.IO.Directory.Exists(path) && roots.Contains(path) == false)
            {
                roots.Add(path);
            }
        }

        private static UnityEngine.Sprite CreateSpriteFromSpr(resource.Buffer buffer, string spriteName, int frameIndex)
        {
            if (buffer == null || buffer.data == null || buffer.size < 40)
            {
                return null;
            }

            byte[] data = buffer.data;
            int size = System.Math.Min(buffer.size, data.Length);
            if (data[0] != (byte)'S' || data[1] != (byte)'P' || data[2] != (byte)'R')
            {
                return null;
            }

            int frameCount = ReadUInt16(data, 12, size);
            int colorCount = ReadUInt16(data, 14, size);
            if (frameCount <= frameIndex || colorCount <= 0 || colorCount > 256)
            {
                return null;
            }

            const int headerSize = 32;
            int paletteOffset = headerSize;
            int offsetTable = paletteOffset + (colorCount * 3);
            int frameBase = offsetTable + (frameCount * 8);
            int frameOffsetEntry = offsetTable + (frameIndex * 8);
            if (frameOffsetEntry + 8 > size || frameBase > size)
            {
                return null;
            }

            int frameRelativeOffset = ReadInt32(data, frameOffsetEntry, size);
            int frameLength = ReadInt32(data, frameOffsetEntry + 4, size);
            if (frameLength < 8)
            {
                return null;
            }

            int frameStart = frameBase + frameRelativeOffset;
            if (frameStart < 0 || frameStart + frameLength > size)
            {
                // Some package readers may return frame offsets that are already absolute.
                frameStart = frameRelativeOffset;
                if (frameStart < 0 || frameStart + frameLength > size)
                {
                    return null;
                }
            }

            int width = ReadUInt16(data, frameStart, size);
            int height = ReadUInt16(data, frameStart + 2, size);
            if (width <= 0 || height <= 0 || width > 1024 || height > 1024)
            {
                return null;
            }

            UnityEngine.Color32[] pixels = new UnityEngine.Color32[width * height];
            int dataCursor = frameStart + 8;
            int dataEnd = frameStart + frameLength;
            int totalPixels = width * height;
            int pixelIndex = 0;
            bool useClearAlpha = ReadUInt16(data, 22, size) == 1 || ReadUInt16(data, 22, size) == 2;

            while (dataCursor + 2 <= dataEnd && pixelIndex < totalPixels)
            {
                int pixelNum = data[dataCursor++];
                int alpha = data[dataCursor++];
                if (pixelIndex + pixelNum > totalPixels)
                {
                    pixelNum = totalPixels - pixelIndex;
                }

                if (alpha == 0)
                {
                    pixelIndex += pixelNum;
                    continue;
                }

                for (int offset = 0; offset < pixelNum && dataCursor < dataEnd; offset++)
                {
                    int paletteIndex = data[dataCursor++];
                    UnityEngine.Color32 color = ReadPaletteColor(data, paletteOffset, colorCount, paletteIndex, alpha, useClearAlpha);
                    SetTopLeftPixel(pixels, width, height, pixelIndex + offset, color);
                }

                pixelIndex += pixelNum;
            }

            UnityEngine.Texture2D texture = new UnityEngine.Texture2D(width, height, UnityEngine.TextureFormat.RGBA32, false);
            texture.name = "item-spr-" + spriteName;
            texture.filterMode = UnityEngine.FilterMode.Point;
            texture.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            UnityEngine.Sprite sprite = UnityEngine.Sprite.Create(
                texture,
                new UnityEngine.Rect(0f, 0f, width, height),
                new UnityEngine.Vector2(0.5f, 0.5f));
            sprite.name = texture.name;
            return sprite;
        }

        private static UnityEngine.Color32 ReadPaletteColor(
            byte[] data,
            int paletteOffset,
            int colorCount,
            int paletteIndex,
            int alpha,
            bool useClearAlpha)
        {
            if (paletteIndex < 0 || paletteIndex >= colorCount)
            {
                return new UnityEngine.Color32(0, 0, 0, 0);
            }

            int offset = paletteOffset + (paletteIndex * 3);
            byte red = data[offset];
            byte green = data[offset + 1];
            byte blue = data[offset + 2];
            byte finalAlpha = (byte)alpha;

            if (useClearAlpha)
            {
                int maxValue = System.Math.Max(red, System.Math.Max(green, blue));
                finalAlpha = (byte)System.Math.Min(alpha, maxValue);
                red = ScaleColor(red, finalAlpha);
                green = ScaleColor(green, finalAlpha);
                blue = ScaleColor(blue, finalAlpha);
            }

            return new UnityEngine.Color32(red, green, blue, finalAlpha);
        }

        private static byte ScaleColor(byte value, byte alpha)
        {
            return (byte)((value * alpha + 127) / 255);
        }

        private static void SetTopLeftPixel(
            UnityEngine.Color32[] pixels,
            int width,
            int height,
            int topLeftIndex,
            UnityEngine.Color32 color)
        {
            if (topLeftIndex < 0 || topLeftIndex >= pixels.Length)
            {
                return;
            }

            int x = topLeftIndex % width;
            int topY = topLeftIndex / width;
            int unityY = height - 1 - topY;
            pixels[(unityY * width) + x] = color;
        }

        private static int ReadUInt16(byte[] data, int offset, int size)
        {
            if (offset < 0 || offset + 2 > size)
            {
                return 0;
            }

            return data[offset] | (data[offset + 1] << 8);
        }

        private static int ReadInt32(byte[] data, int offset, int size)
        {
            if (offset < 0 || offset + 4 > size)
            {
                return 0;
            }

            return data[offset]
                   | (data[offset + 1] << 8)
                   | (data[offset + 2] << 16)
                   | (data[offset + 3] << 24);
        }

        private static void WarnItemSprOnce(string key, string message)
        {
            if (itemSprWarnings.Contains(key))
            {
                return;
            }

            itemSprWarnings.Add(key);
            UnityEngine.Debug.LogWarning(message);
        }

        private string GetThumbnailImagePath()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.imagePath;
            }

            if (this.simpleItemBase != null)
            {
                return this.simpleItemBase.imagePath;
            }

            return this.magicScriptBase != null ? this.magicScriptBase.image : string.Empty;
        }

        public Getter.GridablePos GetTypeGridable()
        {
            Getter.GridablePos result = new GridablePos();
            result.Set(1, 38, 1, 18);
            return result;
        }

        public UnityEngine.Sprite GetTypeSprite()
        {
            UnityEngine.Sprite result = null;

            if (this.IsEquipment() == false)
            {
                if (this.simpleItemBase != null)
                {
                    if (Item.itemFramedTypeWhiteSprite == null)
                    {
                        Item.itemFramedTypeWhiteSprite = Game.Resource("\\user.interface\\panel.equipment\\item.tab\\item.framed.type.white.png").Get<UnityEngine.Sprite>();
                        if (Item.itemFramedTypeWhiteSprite == null)
                        {
                            Item.itemFramedTypeWhiteSprite = CreateGeneratedFrameSprite("frame.white", new UnityEngine.Color(0.94f, 0.94f, 0.86f, 1f));
                        }
                    }

                    result = Item.itemFramedTypeWhiteSprite;
                }
                else if (this.magicScriptBase != null)
                {
                    if (this.magicScriptBase.script.EndsWith(".lua") == false)
                    {
                        if (Item.itemFramedTypeWhiteSprite == null)
                        {
                            Item.itemFramedTypeWhiteSprite = Game.Resource("\\user.interface\\panel.equipment\\item.tab\\item.framed.type.white.png").Get<UnityEngine.Sprite>();
                            if (Item.itemFramedTypeWhiteSprite == null)
                            {
                                Item.itemFramedTypeWhiteSprite = CreateGeneratedFrameSprite("frame.white", new UnityEngine.Color(0.94f, 0.94f, 0.86f, 1f));
                            }
                        }

                        result = Item.itemFramedTypeWhiteSprite;
                    }
                    else
                    {
                        if (Item.itemFramedTypeGreenSprite == null)
                        {
                            Item.itemFramedTypeGreenSprite = Game.Resource("\\user.interface\\panel.equipment\\item.tab\\item.framed.type.green.png").Get<UnityEngine.Sprite>();
                            if (Item.itemFramedTypeGreenSprite == null)
                            {
                                Item.itemFramedTypeGreenSprite = CreateGeneratedFrameSprite("frame.green", new UnityEngine.Color(0.22f, 0.85f, 0.28f, 1f));
                            }
                        }

                        result = Item.itemFramedTypeGreenSprite;
                    }
                }

                return result;
            }

            if (this.HaveMagicAttrib() == false)
            {
                if (Item.itemFramedTypeWhiteSprite == null)
                {
                    Item.itemFramedTypeWhiteSprite = Game.Resource("\\user.interface\\panel.equipment\\item.tab\\item.framed.type.white.png").Get<UnityEngine.Sprite>();
                    if (Item.itemFramedTypeWhiteSprite == null)
                    {
                        Item.itemFramedTypeWhiteSprite = CreateGeneratedFrameSprite("frame.white", new UnityEngine.Color(0.94f, 0.94f, 0.86f, 1f));
                    }
                }

                result = Item.itemFramedTypeWhiteSprite;
            }
            else
            {
                if (Item.itemFramedTypeBlueSprite == null)
                {
                    Item.itemFramedTypeBlueSprite = Game.Resource("\\user.interface\\panel.equipment\\item.tab\\item.framed.type.blue.png").Get<UnityEngine.Sprite>();
                    if (Item.itemFramedTypeBlueSprite == null)
                    {
                        Item.itemFramedTypeBlueSprite = CreateGeneratedFrameSprite("frame.blue", new UnityEngine.Color(0.32f, 0.46f, 1f, 1f));
                    }
                }

                result = Item.itemFramedTypeBlueSprite;
            }

            return result;
        }

        private UnityEngine.Color ResolveFallbackItemColor()
        {
            if (this.GetItemType() == Defination.Type.goldEquip)
            {
                return new UnityEngine.Color(1f, 0.78f, 0.18f, 1f);
            }

            if (this.IsEquipment())
            {
                switch (this.GetDetail())
                {
                    case (int)Defination.Detail.equip_meleeweapon:
                    case (int)Defination.Detail.equip_rangeweapon:
                        return new UnityEngine.Color(0.78f, 0.74f, 0.68f, 1f);

                    case (int)Defination.Detail.equip_armor:
                    case (int)Defination.Detail.equip_helm:
                    case (int)Defination.Detail.equip_boots:
                    case (int)Defination.Detail.equip_belt:
                        return new UnityEngine.Color(0.58f, 0.68f, 0.86f, 1f);

                    case (int)Defination.Detail.equip_ring:
                    case (int)Defination.Detail.equip_amulet:
                    case (int)Defination.Detail.equip_cuff:
                    case (int)Defination.Detail.equip_pendant:
                        return new UnityEngine.Color(0.78f, 0.52f, 0.9f, 1f);
                }
            }

            switch (this.GetGenre())
            {
                case (int)Defination.Genre.item_medicine:
                    return new UnityEngine.Color(0.88f, 0.22f, 0.22f, 1f);

                case (int)Defination.Genre.item_mine:
                case (int)Defination.Genre.item_materials:
                    return new UnityEngine.Color(0.35f, 0.75f, 0.48f, 1f);

                case (int)Defination.Genre.item_task:
                    return new UnityEngine.Color(0.35f, 0.62f, 0.94f, 1f);

                case (int)Defination.Genre.item_townportal:
                    return new UnityEngine.Color(0.42f, 0.86f, 0.95f, 1f);
            }

            return new UnityEngine.Color(0.72f, 0.62f, 0.46f, 1f);
        }

        private static UnityEngine.Sprite CreateGeneratedFrameSprite(string key, UnityEngine.Color color)
        {
            return CreateGeneratedSprite(key, color, false);
        }

        private static UnityEngine.Sprite CreateGeneratedItemSprite(string key, UnityEngine.Color color, bool filled)
        {
            return CreateGeneratedSprite(key, color, filled);
        }

        private static UnityEngine.Sprite CreateGeneratedSprite(string key, UnityEngine.Color color, bool filled)
        {
            if (itemGeneratedSprites.TryGetValue(key, out UnityEngine.Sprite cachedSprite))
            {
                return cachedSprite;
            }

            const int size = 32;
            UnityEngine.Texture2D texture = new UnityEngine.Texture2D(size, size, UnityEngine.TextureFormat.RGBA32, false);
            texture.name = "generated-item-" + key;
            texture.filterMode = UnityEngine.FilterMode.Point;
            texture.wrapMode = UnityEngine.TextureWrapMode.Clamp;

            UnityEngine.Color transparent = new UnityEngine.Color(0f, 0f, 0f, 0f);
            UnityEngine.Color border = new UnityEngine.Color(
                UnityEngine.Mathf.Clamp01(color.r + 0.16f),
                UnityEngine.Mathf.Clamp01(color.g + 0.16f),
                UnityEngine.Mathf.Clamp01(color.b + 0.16f),
                1f);
            UnityEngine.Color shadow = new UnityEngine.Color(0f, 0f, 0f, 0.42f);

            UnityEngine.Color[] pixels = new UnityEngine.Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool outline = x == 2 || x == size - 3 || y == 2 || y == size - 3;
                    bool inside = x > 2 && x < size - 3 && y > 2 && y < size - 3;
                    bool shine = inside && y > 21 && x > 7 && x < 14;

                    UnityEngine.Color pixel = transparent;
                    if (outline)
                    {
                        pixel = border;
                    }
                    else if (filled && inside)
                    {
                        pixel = shine ? border : color;
                    }
                    else if (!filled && (x == 3 || x == size - 4 || y == 3 || y == size - 4))
                    {
                        pixel = shadow;
                    }

                    pixels[(y * size) + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);

            UnityEngine.Sprite sprite = UnityEngine.Sprite.Create(
                texture,
                new UnityEngine.Rect(0f, 0f, size, size),
                new UnityEngine.Vector2(0.5f, 0.5f),
                size);
            sprite.name = texture.name;
            itemGeneratedSprites[key] = sprite;
            return sprite;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public settings.item.EquipmentBase GetEquipmentBase()
        {
            return this.equipmentBase;
        }

        public settings.item.MagicScriptBase GetMagicScriptBase()
        {
            return this.magicScriptBase;
        }

        public settings.item.SimpleItemBase GetSimpleItemBase()
        {
            return this.simpleItemBase;
        }

        public List<settings.skill.SkillSettingData.KMagicAttrib> GetBasicAttribs()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.GetBasicAttrib();
            }

            if (this.simpleItemBase != null)
            {
                return this.simpleItemBase.GetBasicAttrib();
            }

            return null;
        }

        public List<settings.skill.SkillSettingData.KMagicAttrib> GetRequiredAttribs()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.GetRequiredAttrib();
            }

            return null;
        }

        public bool HaveMagicAttrib()
        {
            if(this.magicAttrib != null
                && this.magicAttrib.Count > 0)
            {
                return true;
            }

            return false;
        }

        public List<settings.skill.SkillSettingData.KMagicAttrib> GetMagicAttribs()
        {
            return this.magicAttrib;
        }

        public int GetGenre()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.genre;
            }
            else if (this.simpleItemBase != null)
            {
                return this.simpleItemBase.genre;
            }
            else if (this.magicScriptBase != null)
            {
                return this.magicScriptBase.genre;
            }

            return -1;
        }

        public int GetDetail()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.detail;
            }
            else if (this.simpleItemBase != null)
            {
                return this.simpleItemBase.detail;
            }
            else if (this.magicScriptBase != null)
            {
                return this.magicScriptBase.detail;
            }

            return -1;
        }

        public int GetParticular()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.particular;
            }
            else if (this.simpleItemBase != null)
            {
                return this.simpleItemBase.particular;
            }
            else if (this.magicScriptBase != null)
            {
                return this.magicScriptBase.particular;
            }

            return -1;
        }

        public int GetLevel()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.level;
            }

            if (this.simpleItemBase != null)
            {
                return this.level > 0 ? this.level : this.simpleItemBase.level;
            }

            return this.level;
        }

        public int GetSeries()
        {
            return this.series;
        }

        public string GetGDPLS()
        {
            return string.Empty + this.GetGenre() + ", " + this.GetDetail() + ", " + this.GetParticular() + ", " + this.GetLevel() + ", " + this.GetSeries();
        }

        public string GetName()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.name;
            }

            if (this.simpleItemBase != null)
            {
                return this.simpleItemBase.name;
            }

            if (this.magicScriptBase != null)
            {
                return this.magicScriptBase.name;
            }

            return null;
        }

        public int GetPrice()
        {
            if (this.equipmentBase != null)
            {
                return this.equipmentBase.price;
            }

            if (this.simpleItemBase != null)
            {
                return this.simpleItemBase.price > 0 ? this.simpleItemBase.price : this.simpleItemBase.priceXu;
            }

            if (this.magicScriptBase != null)
            {
                return this.magicScriptBase.price;
            }

            return 0;
        }

        public string GetBasicAttribDesc()
        {
            if (this.basicAttribDesc != null)
            {
                return this.basicAttribDesc;
            }

            this.basicAttribDesc = string.Empty;
            List<settings.skill.SkillSettingData.KMagicAttrib> basicList = this.GetBasicAttribs();

            if (basicList == null)
            {
                return this.basicAttribDesc;
            }

            foreach (skill.SkillSettingData.KMagicAttrib magicEntry in basicList)
            {
                if (magicEntry.nAttribType <= 0)
                {
                    continue;
                }

                if (magicEntry.nValue[0] <= 0
                    && magicEntry.nValue[1] <= 0)
                {
                    continue;
                }

                if (this.basicAttribDesc.Length > 0)
                {
                    this.basicAttribDesc += "\n";
                }

                this.basicAttribDesc += settings.MagicDesc.Get(magicEntry);
            }

            return this.basicAttribDesc;
        }

        public List<string> GetMagicAttribDesc()
        {
            if (this.magicAttribDesc != null)
            {
                return this.magicAttribDesc;
            }

            this.magicAttribDesc = new List<string>();

            if(this.magicAttrib == null)
            {
                return this.magicAttribDesc;
            }

            foreach (skill.SkillSettingData.KMagicAttrib magicEntry in this.magicAttrib)
            {
                this.magicAttribDesc.Add(settings.MagicDesc.Get(magicEntry));
            }

            return this.magicAttribDesc;
        }

        public bool IsEquipment()
        {
            if (this.equipmentBase == null)
            {
                return false;
            }

            return (int)item.Defination.Genre.item_equip == this.equipmentBase.genre;
        }

        public uint GetDatabaseId()
        {
            return this.databaseId;
        }

        public string GetIntro()
        {
            if (this.equipmentBase != null)
            {
                return Getter.GetRichText(this.equipmentBase.intro);
            }

            if (this.simpleItemBase != null)
            {
                return Getter.GetRichText(this.simpleItemBase.intro ?? string.Empty);
            }

            if (this.magicScriptBase != null)
            {
                return Getter.GetRichText(this.magicScriptBase.intro);
            }

            return null;
        }

        public settings.item.Defination.Type GetItemType()
        {
            return this.type;
        }

        public Dictionary<int, string> GetSetItemList()
        {
            if (this.type != Defination.Type.goldEquip)
            {
                return null;
            }

            if (this.setItemList != null)
            {
                return this.setItemList;
            }

            return this.setItemList = item.Getters.GetGoldItemSet(((item.GoldEquipBase)this.equipmentBase).idSet);
        }

        public int GetStackCurrently() => this.stack;
        public int GetStackMaximun()
        {
            if(this.magicScriptBase == null)
            {
                return this.simpleItemBase != null ? this.simpleItemBase.stack : 0;
            }

            return this.magicScriptBase.stackValue;
        }

        public long GetTimeUse() => this.timeUse;

        public void SetTimeUse(long timeUse) => this.timeUse = timeUse;

        public bool IsShowValue()
        {
            return this.simpleItemBase != null && this.simpleItemBase.isShowValue != 0;
        }

        public string GetTimeExpire()
        {
            return System.DateTimeOffset.FromUnixTimeSeconds(this.timeUse).ToString("HH:mm dd-MM-yyyy");
        }
    }
}
