using System;

namespace XWear.XWearPackage.Editor.Util
{
    [Serializable]
    public abstract class EditorContent
    {
        public XWearPackageEditorWindow root;

        protected EditorContent(XWearPackageEditorWindow root)
        {
            this.root = root;
        }

        public virtual void OnFocus() { }

        public virtual void OnHierarchyChange() { }

        public virtual void OnSelect() { }

        public virtual void OnUndoRedo() { }

        public virtual void OnForceUpdate() { }

        public abstract void DrawGui();
    }
}
