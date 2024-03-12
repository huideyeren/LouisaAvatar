namespace XWear.IO.Avatar.HumanoidTransfer
{
    public interface IHumanoidReferenceTransfer
    {
        int Order { get; }
        bool Check(UnityEngine.Component component);

        void Transfer(
            UnityEngine.Component component,
            HumanoidMap baseModelHumanoidMap,
            HumanoidMap dressPartSourceHumanoidMap
        );
    }
}
