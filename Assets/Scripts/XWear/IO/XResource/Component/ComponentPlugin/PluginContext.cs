namespace XWear.IO.XResource.Component.ComponentPlugin
{
    public class PluginContext<T>
    {
        private T _entity;

        public void Set(T entity)
        {
            _entity = entity;
        }

        public T Get()
        {
            return _entity;
        }
    }
}
