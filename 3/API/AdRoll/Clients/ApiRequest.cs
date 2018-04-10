using System;
using System.Linq;
using System.Text;
using RestSharp;

namespace AdRoll.Clients
{
    public class ApiRequest
    {
        /// <summary>
        /// Add a parameter with the name and value of each public property of the instance.
        /// </summary>
        public virtual void AddParametersTo(RestRequest restRequest)
        {
            var propertiesWithValue = from property in GetType().GetProperties()
                                      let value = property.GetValue(this)
                                      where value != null
                                      select Tuple.Create(property.Name, value.ToString());

            foreach (var tuple in propertiesWithValue)
                restRequest.AddParameter(tuple.Item1, tuple.Item2);
        }

        public virtual string ParametersAsString(bool includeQuestionMark = false)
        {
            var propertiesWithValue = from property in GetType().GetProperties()
                                      let value = property.GetValue(this)
                                      where value != null
                                      select Tuple.Create(property.Name, value.ToString());
            bool first = true;
            var parms = new StringBuilder(includeQuestionMark ? "?" : String.Empty);
            foreach (var tuple in propertiesWithValue)
            {
                if (first)
                    first = false;
                else
                    parms.Append("&");
                parms.Append(tuple.Item1 + "=" + tuple.Item2);
            }
            return parms.ToString();
        }
    }
}
