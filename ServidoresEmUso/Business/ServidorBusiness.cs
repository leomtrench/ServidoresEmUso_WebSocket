using ServidoresEmUso.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidoresEmUso.Business
{
    public class ServidorBusiness
    {
        private string _filePath = "C:\\Sample.json";

        #region Singleton
        private static ServidorBusiness _business;
        public static ServidorBusiness Instance
        {
            get
            {
                if (_business == null)
                    _business = new ServidorBusiness();
                return _business;
            }
        }
        #endregion

        public string JsonServidores
        {
            get
            {
                if (Servidores == null)
                    ReadFile();
                return System.Text.Json.JsonSerializer.Serialize(Servidores);
            }
        }
        public List<Models.Servidor> Servidores;

        public object LockFile = new object();
        public string ReadFile()
        {
            StringBuilder contentFile = new StringBuilder();
            String line;
            try
            {
                lock (LockFile)
                {
                    //Pass the file path and file name to the StreamReader constructor
                    using (StreamReader sr = new StreamReader(_filePath))
                    {
                        //Read the first line of text
                        contentFile.Append(sr.ReadLine());
                        //Continue to read until you reach end of file
                        while ((line = sr.ReadLine()) != null)
                        {
                            //Read the next line
                            contentFile.Append(line);
                        }
                        //close the file
                        sr.Close();
                    }
                    Servidores = System.Text.Json.JsonSerializer.Deserialize<List<Models.Servidor>>(contentFile.ToString());
                }
                return contentFile.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            return "";
        }

        public string WriteFile(string value)
        {
            try
            {
                lock (LockFile)
                {
                    //Pass the filepath and filename to the StreamWriter Constructor
                    using (StreamWriter sw = new StreamWriter(_filePath))
                    {
                        //Write a line of text
                        sw.WriteLine(value);
                        //Close the file
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            return "";
        }

        public void ChangeStatusServidor(string hash)
        {
            if (!string.IsNullOrEmpty(hash))
            {
                Servidor servidor = Servidores.Where(x => x.Hash == hash).FirstOrDefault();
                if (servidor != null)
                {
                    servidor.Conectado = !servidor.Conectado;
                    WriteFile(System.Text.Json.JsonSerializer.Serialize(Servidores));
                    Servidores = null;
                }
            }
        }
    }
}
