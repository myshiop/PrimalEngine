using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;
using System.Windows.Shapes;

namespace PrimalEditor.Utilities
{
    public static class Serializer
    {
        public static void ToFile<T>(T instance, string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Create);
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    var settings = new XmlWriterSettings { Indent = true }; // 添加这行代码以设置缩进
                    using var writer = XmlWriter.Create(fs, settings); // 使用带有设置的XmlWriter
                    serializer.WriteObject(writer, instance);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                
            }
        }

        public static T FromFile<T>(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open);
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    T instance = (T)serializer.ReadObject(fs);
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return default(T);
            }
        }
    }
}
