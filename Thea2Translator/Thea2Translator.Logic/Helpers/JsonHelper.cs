using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Thea2Translator.Logic
{
    public class JsonHelper
    {
        public static string ToJson(object obj)
        {
            var json = new JavaScriptSerializer().Serialize(obj);
            json = json.Replace("{", "{\r\n");
            json = json.Replace("}", "}\r\n");
            json = json.Replace("[", "[\r\n");
            json = json.Replace("]", "]\r\n");
            json = json.Replace(",", ",\r\n");
            return json;
        }
    }
}
