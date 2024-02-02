using System.Reflection;

namespace XWear.IO.XResource.Component
{
    /// <summary>
    /// コンポーネントのコピーをリフレクションで実行する基底クラス
    /// 実際にXResourceを構築する処理は安全性のため継承先で実行する必要がある
    /// 多用に注意
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ReflectionCollectComponentPlugin<T> : DefaultCollectComponentPluginBase<T>
        where T : UnityEngine.Component
    {
        protected override void CopyComponent(UnityEngine.Transform attachTarget, T sourceComponent)
        {
            var attached = attachTarget.gameObject.AddComponent<T>();
            var type = sourceComponent.GetType();

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                field.SetValue(attached, field.GetValue(sourceComponent));
            }

            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    property.SetValue(attached, property.GetValue(sourceComponent, null), null);
                }
            }
        }
    }
}
