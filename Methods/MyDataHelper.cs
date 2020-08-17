using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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

        /// <summary>
        /// 将内容写入文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="Content">文件内容</param>
        public static void WriteFile(string Path,string Content)
        {
            try
            {
                FileStream fileStream = new FileStream(Path, FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fileStream);

                streamWriter.Write(Content);
                streamWriter.Flush();

                streamWriter.Close();
                fileStream.Close();
            }
            catch(Exception e) { throw e; }
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <returns>文件内容</returns>
        public static string ReadFile(string Path)
        {
            FileStream fileStream = new FileStream(Path, FileMode.Open, FileAccess.Read);
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            char[] content = Encoding.UTF8.GetChars(bytes);
            fileStream.Close();

            return new string(content);
        }

        /// <summary>
        /// 将图片转码位Base64
        /// </summary>
        /// <param name="bmp">Bitmap</param>
        /// <returns>Base64</returns>
        public static string ImgToBase64String(Bitmap bmp)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将Base64转码为图片
        /// </summary>
        /// <param name="strbase64">Base64</param>
        /// <returns>Bitmap</returns>
        public static Bitmap Base64StringToImage(string strbase64)
        {
            try
            {
                byte[] arr = Convert.FromBase64String(strbase64);
                MemoryStream ms = new MemoryStream(arr);
                Bitmap bmp = new Bitmap(ms);
                ms.Close();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将Bitmap转换为BitmapImage
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <returns>BitmapImage 若bitmap为空或无效，则返回null</returns>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }
    }
}
