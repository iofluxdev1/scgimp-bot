using System.Collections.Generic;

namespace StarCitizen.Gimp.Core
{
    public class ScGimpNotificationEqualityComparer : IEqualityComparer<ScGimpNotification>
    {
        public bool Equals(ScGimpNotification x, ScGimpNotification y)
        {
            bool isEqual = string.Compare(x.Medium, y.Medium, true) == 0 &&
                string.Compare(x.NotificationType, y.NotificationType, true) == 0 &&
                string.Compare(x.Recipients, y.Recipients, true) == 0 &&
                string.Compare(x.Items, y.Items, true) == 0 &&
                string.Compare(x.Body, y.Body, true) == 0;

            return isEqual;
        }

        public int GetHashCode(ScGimpNotification obj)
        {
            int hashCode = obj.GetHashCode();

            return hashCode;
        }
    }
}
