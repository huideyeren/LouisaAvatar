using System.Collections.Generic;
using System.Linq;

namespace XWear.IO.XResource.Util
{
    public static class DictionaryUtil
    {
        public static Dictionary<T1, T> FlipKvp<T, T1>(this Dictionary<T, T1> dict)
        {
            return dict.ToDictionary(x => x.Value, x => x.Key);
        }
    }
}
