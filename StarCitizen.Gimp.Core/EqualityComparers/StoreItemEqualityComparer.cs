using System.Collections.Generic;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class StoreItemEqualityComparer : IEqualityComparer<StoreItem>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(StoreItem x, StoreItem y)
        {
            return x.Sku == y.Sku;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(StoreItem obj)
        {
            return obj.Sku.GetHashCode();
        }
    }
}
