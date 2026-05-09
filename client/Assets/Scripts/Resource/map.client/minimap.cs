
using System.Collections.Generic;

namespace game.resource.map
{
    public class MiniMap
    {
        public static UnityEngine.Sprite CreateDotColor(UnityEngine.Color32 color32)
        {
            UnityEngine.Texture2D texture = new UnityEngine.Texture2D(1, 1);

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, color32);
                }
            }

            texture.Apply();

            return UnityEngine.Sprite.Create(
                texture,
                new UnityEngine.Rect(0, 0, texture.width, texture.height),
                new UnityEngine.Vector2(0.5f, 0.5f)
            );
        }

        private game.scene.world.userInterface.MiniMap userInterface;

        public readonly UnityEngine.GameObject go;
        public readonly UnityEngine.RectTransform compRect;
        public readonly UnityEngine.UI.Image compImage;

        private readonly UnityEngine.Sprite redDot;
        private readonly UnityEngine.Sprite purpleDot;

        // special.npc => red.dot
        private readonly Dictionary<settings.npcres.Controller, UnityEngine.GameObject> specialNpcPoints;
        private readonly Dictionary<settings.npcres.Controller, UnityEngine.GameObject> normalNpcPoints;

        public float xRatio;
        public float yRatio;

        public MiniMap()
        {
            this.go = new UnityEngine.GameObject(typeof(game.resource.map.MiniMap).FullName);
            this.compRect = this.go.AddComponent<UnityEngine.RectTransform>();
            this.compImage = this.go.AddComponent<UnityEngine.UI.Image>();

            this.redDot = MiniMap.CreateDotColor(new UnityEngine.Color32(255, 0, 0, 255));
            this.purpleDot = MiniMap.CreateDotColor(new UnityEngine.Color32(128, 0, 128, 255));

            this.specialNpcPoints = new Dictionary<settings.npcres.Controller, UnityEngine.GameObject>();
            this.normalNpcPoints = new Dictionary<settings.npcres.Controller, UnityEngine.GameObject>();
        }

        public void SetUI(game.scene.world.userInterface.MiniMap userInterface) => this.userInterface = userInterface;

        public void Clear()
        {
            this.compImage.sprite = null;

            foreach (KeyValuePair<settings.npcres.Controller, UnityEngine.GameObject> entry in this.specialNpcPoints)
            {
                UnityEngine.GameObject.Destroy(entry.Value);
            }

            foreach (KeyValuePair<settings.npcres.Controller, UnityEngine.GameObject> entry in this.normalNpcPoints)
            {
                UnityEngine.GameObject.Destroy(entry.Value);
            }

            this.specialNpcPoints.Clear();
            this.normalNpcPoints.Clear();
        }

        public void Load(settings.MapList.MapInfo mapInfo)
        {
            UnityEngine.Sprite sprite = Game.Resource(mapInfo.filePath.miniMapImage).Get<UnityEngine.Sprite>();

            if (sprite == null)
            {
                this.compRect.sizeDelta = UnityEngine.Vector2.zero;
                this.compImage.sprite = null;
                this.xRatio = 0;
                this.yRatio = 0;
                UnityEngine.Debug.LogWarning("game.resource.map.MiniMap missing image: " + mapInfo.filePath.miniMapImage);
                return;
            }

            this.compRect.sizeDelta = new UnityEngine.Vector2(sprite.texture.width, sprite.texture.height);
            this.compImage.sprite = sprite;

            this.xRatio = -((mapInfo.worFile.rect.left * 512) / 16f) - (this.compRect.sizeDelta.x / 2f);
            this.yRatio = ((mapInfo.worFile.rect.top * 512) / 16f) + (this.compRect.sizeDelta.y / 2f);
        }

        public void Reset(settings.MapList.MapInfo mapInfo)
        {
            this.Clear();
            this.Load(mapInfo);

            if (this.userInterface != null)
            {
                this.userInterface.SetMapName(mapInfo.name);
            }
        }

        public void AddObject(settings.npcres.Controller npcController)
        {
            UnityEngine.Sprite dotSprite;
            Dictionary<settings.npcres.Controller, UnityEngine.GameObject> storage;

            if (npcController.IsSpecialNpc())
            {
                dotSprite = this.redDot;
                storage = this.specialNpcPoints;
            }
            else
            {
                dotSprite = this.purpleDot;
                storage = this.normalNpcPoints;
            }

            if (storage.ContainsKey(npcController))
            {
                return;
            }

            UnityEngine.GameObject goDot = new UnityEngine.GameObject(npcController.GetIdentify().GetName());
            goDot.AddComponent<UnityEngine.RectTransform>().sizeDelta = new UnityEngine.Vector2(10, 10);
            goDot.AddComponent<UnityEngine.UI.Image>().sprite = dotSprite;
            goDot.transform.SetParent(this.go.transform, false);

            storage[npcController] = goDot;
        }

        public void SetMapPosition(settings.npcres.Controller npcController, resource.map.Position position)
        {
            UnityEngine.RectTransform obRect;

            if (npcController.IsSpecialNpc())
            {
                if (specialNpcPoints.Count == 0)
                {
                    return;
                }
                obRect = this.specialNpcPoints[npcController].GetComponent<UnityEngine.RectTransform>();
            }
            else
            {
                if (normalNpcPoints.Count == 0)
                {
                    return;
                }
                obRect = this.normalNpcPoints[npcController].GetComponent<UnityEngine.RectTransform>();
            }

            obRect.anchoredPosition = new UnityEngine.Vector2(
                (position.left / 16f) + this.xRatio,
                this.yRatio - (position.top / 16f)
            );
        }

    }
}
