using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Frogy.Methods
{
    /// <summary>
    /// 数据相关Helper类
    /// </summary>
    class MyDataHelper
    {
        /// <summary>
        /// 将Json反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="Json">Json</param>
        /// <returns>对象</returns>
        public static T CoverJsonToObject<T>(string Json)
        {
            return JsonConvert.DeserializeObject<T>(Json);
        }


        public static T Deserialize<T>(byte[] datas)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(datas, 0, datas.Length);
            T obj = (T)bf.Deserialize(stream);
            stream.Dispose();
            return obj;
        }

        /// <summary>
        /// 将对象序列化为Json
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="Object">对象</param>
        /// <returns>Json</returns>
        public static string CoverObjectToJson<T>(T Object)
        {
            return JsonConvert.SerializeObject(Object, Formatting.Indented);
        }

        public static byte[] Serialize<T>(T obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            bf.Serialize(stream, obj);
            byte[] datas = stream.ToArray();
            stream.Dispose();
            return datas;
        }

        /// <summary>
        /// 将内容写入文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="Content">文件内容</param>
        public static void WriteFile(string Path,string Content)
        {
            FileStream fileStream = new FileStream(Path, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            
            streamWriter.Write(Content);
            streamWriter.Flush();

            streamWriter.Close();
            fileStream.Close();
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <returns>文件内容</returns>
        public static string ReadFile(string Path)
        {
            StreamReader streamReader = new StreamReader(Path, Encoding.Default);
            string line, Content = "";
            while ((line = streamReader.ReadLine()) != null)
            {
                Content += (line.ToString() + Environment.NewLine);
            }
            streamReader.Close();
            return Content;
        }
    }
}
