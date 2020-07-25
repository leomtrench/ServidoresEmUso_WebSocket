using System;
using System.Security.Cryptography;
using System.Text;

namespace ServidoresEmUso.Models
{
    public class Servidor
    {
        public string Nome { get; set; }
        public string _hash;
        public string Hash
        {
            get
            {
                if (_hash != null)
                {
                    return _hash;
                }
                else
                {
                    string rawData = Guid.NewGuid() + Nome + Conectado;
                    // Create a SHA256   
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        // ComputeHash - returns byte array  
                        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                        // Convert byte array to a string   
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            builder.Append(bytes[i].ToString("x2"));
                        }
                        _hash = builder.ToString();
                        return _hash;
                    }
                }
            }
        }
        public bool Conectado { get; set; }
    }
}
