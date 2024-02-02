namespace XWear.IO.Avatar.HumanoidTransfer
{
    public abstract class DefaultHumanoidTransferBase<T> : IHumanoidReferenceTransfer
        where T : UnityEngine.Component
    {
        public abstract int Order { get; }

        public bool Check(UnityEngine.Component component)
        {
            return component is T;
        }

        public virtual void Transfer(
            UnityEngine.Component component,
            HumanoidMap baseModelHumanoidMap,
            HumanoidMap dressPartSourceHumanoidMap
        )
        {
            if (component is T validComponent)
            {
                Transfer(
                    validComponent,
                    baseModelHumanoidMap: baseModelHumanoidMap,
                    xResourceHumanoidMap: dressPartSourceHumanoidMap
                );
            }
        }

        protected abstract void Transfer(
            T component,
            HumanoidMap baseModelHumanoidMap,
            HumanoidMap xResourceHumanoidMap
        );
    }
}
