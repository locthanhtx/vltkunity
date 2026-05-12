
using System.Collections.Generic;

namespace game.resource.settings.npcres
{
    public class Shape
    {
        public class Appearance
        {
            public UnityEngine.GameObject parent;
            public readonly UnityEngine.Transform transform;
            public readonly UnityEngine.Rendering.SortingGroup sortingGroup;

            public Appearance(string _objectName)
            {
                this.parent = new UnityEngine.GameObject(_objectName);
                this.transform = parent.transform;
                this.sortingGroup = this.parent.AddComponent<UnityEngine.Rendering.SortingGroup>();
            }

            public static implicit operator UnityEngine.GameObject(Shape.Appearance _appearance)
            {
                return _appearance.parent;
            }
        }

        public class PartFields
        {
            public UnityEngine.GameObject gameObject;
            public UnityEngine.RectTransform rectTransform;
            public UnityEngine.SpriteRenderer spriteRenderer;
            public bool isValid;
            //public game.resource.SPR.Info sprInfo;
        }

        public class PartFrame
        {
            public bool isValid;
            //public game.resource.SPR.FrameInfo frameInfo;
            public UnityEngine.Sprite sprite;
            public UnityEngine.Vector2 sizeDelta;
            public UnityEngine.Vector2 anchoredPosition;
        }

        private readonly Shape.Appearance appearance;
        private readonly Dictionary<string, npcres.Shape.PartFields> partFields;
        private readonly Dictionary<string, Dictionary<int, npcres.Shape.PartFrame>> partFrame;

        public Shape(string _objectName)
        {
            this.appearance = new Shape.Appearance(_objectName);
            this.partFields = new Dictionary<string, PartFields>();
            this.partFrame = new Dictionary<string, Dictionary<int, PartFrame>>();
        }

        public void Destroy()
        {
            UnityEngine.GameObject.Destroy(this.appearance.parent);
            this.appearance.parent = null;
        }

        public Shape.Appearance GetAppearance() => this.appearance;

        public npcres.Shape.PartFields GetPartField(string _partName)
        {
            if (this.partFields.ContainsKey(_partName) == false)
            {
                return null;
            }

            return this.partFields[_partName];
        }

        public Dictionary<string, npcres.Shape.PartFields> GetPartList()
        {
            return this.partFields;
        }

        //public npcres.Shape.PartFields GetPartFields(string _partName, game.resource.settings.npcres.Structures.PartAnimation _partAnimation)
        public npcres.Shape.PartFields GetPartFields(string _partName)
        {
            if (this.partFields.ContainsKey(_partName) == false)
            {
                npcres.Shape.PartFields newPartField = new PartFields();
                newPartField.gameObject = new UnityEngine.GameObject(_partName);
                newPartField.rectTransform = newPartField.gameObject.AddComponent<UnityEngine.RectTransform>();
                newPartField.spriteRenderer = newPartField.gameObject.AddComponent<UnityEngine.SpriteRenderer>();
                newPartField.isValid = false;

                newPartField.gameObject.transform.SetParent(this.appearance.transform, false);
                newPartField.rectTransform.sizeDelta = new UnityEngine.Vector2(0, 0);

                this.partFields[_partName] = newPartField;
            }

            npcres.Shape.PartFields updatePartData = this.partFields[_partName];

            if (updatePartData.isValid)
            {
                return updatePartData;
            }

            updatePartData.gameObject.SetActive(true);
            updatePartData.isValid = true;

            return updatePartData;
        }

        public npcres.Shape.PartFrame GetPartFrame(string _partName, ushort _frameIndex, game.resource.settings.npcres.Structures.PartAnimation _partAnimation)
        {
            if (this.partFrame.TryGetValue(_partName, out Dictionary<int, PartFrame> frameByIndex) == false)
            {
                frameByIndex = new Dictionary<int, PartFrame>();
                this.partFrame[_partName] = frameByIndex;
            }

            if (frameByIndex.TryGetValue(_frameIndex, out npcres.Shape.PartFrame cachedPartFrame)
                && cachedPartFrame.isValid)
            {
                return cachedPartFrame;
            }

            settings.skill.texture.SprCache.Data.SprFrame sprFrame = npcres.Cache.GetSprFrame(_partAnimation.sprPath, _frameIndex);
            if (sprFrame == null || sprFrame.sprite == null)
            {
                return null;
            }

            npcres.Shape.PartFrame updatePartFrame = cachedPartFrame ?? new PartFrame();
            updatePartFrame.sprite = sprFrame.sprite;
            updatePartFrame.sizeDelta = sprFrame.sizeDelta;
            updatePartFrame.anchoredPosition = sprFrame.anchoredPosition;
            updatePartFrame.isValid = true;

            if (cachedPartFrame == null)
            {
                frameByIndex[_frameIndex] = updatePartFrame;
            }

            return updatePartFrame;
        }

        public void InValidPart(string _partName, bool deactivate = true)
        {
            if (this.partFields.ContainsKey(_partName)
                && this.partFields[_partName].isValid == true)
            {
                npcres.Shape.PartFields updatePartFields = this.partFields[_partName];
                if (deactivate)
                {
                    updatePartFields.gameObject.SetActive(false);
                    updatePartFields.isValid = false;
                }
            }

            this.partFrame.Remove(_partName);
        }

        public void InValidPartList(
            Dictionary<string, game.resource.settings.npcres.Structures.PartAnimation> _partList,
            bool deactivate = false)
        {
            foreach (KeyValuePair<string, game.resource.settings.npcres.Structures.PartAnimation> partPair in _partList)
            {
                this.InValidPart(partPair.Key, deactivate);
            }
        }
    }
}
