using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.IO;

namespace DALTools
{
    class XMLTools
    {
        static string dir = @"xml\";
        static XMLTools()
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveListToXMLSerializer<T>(List<T> list,string filePath)
        {
            try
            {
                FileStream fStream = new FileStream(dir + filePath, FileMode.Create);
                XmlSerializer x = new XmlSerializer(list.GetType());
                x.Serialize(fStream, list);
                fStream.Close();
              
            }
            catch (Exception x)
            {
                string t = x.Message;
                throw new Exception();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IEnumerable<T> LoadListFromXMLSerializer<T>(string filePath)
        {
            try
            {
                //bool isFileOpenAlready = IsFileLocked(new FileInfo(dir + filePath));
                //if(isFileOpenAlready)
                //{
                    
                //}    
               
                if (File.Exists(dir + filePath))
                {

                    List<T> list;
                    XmlSerializer x = new XmlSerializer(typeof(List<T>));
                    FileStream file = new FileStream(dir + filePath, FileMode.Open);
                    list = (List<T>)x.Deserialize(file);
                    file.Close();
                    return list;
                }
                else
                    return new List<T>();
            }
            catch (Exception x)
            {
                string t = x.Message;
                throw new Exception();
            }
        }









       static bool IsFileLocked(FileInfo file)//method is used for debugging only...
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }


        //end of class
    }
}




