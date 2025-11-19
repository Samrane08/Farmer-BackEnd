using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Helper
{
    public class QueryStringEncryptDecrypt
    {
        [Serializable]
        public class QryString
        {
            public String QryStr { get; set; }
            public String QryStrValue { get; set; }

            public QryString(String QryStrng, String QryStrVal)
            {
                this.QryStr = QryStrng;
                this.QryStrValue = QryStrVal;
            }
        }

        [Serializable]
        public class QryStringsCollection : CollectionBase, IEnumerable, IEnumerator
        {
            private int index = -1;

            public QryStringsCollection()
            {
                this.index = -1;
            }

            public QryStringsCollection(string str)
            {
                String[] arLst = str.Split('&');
                QryString qryStr;
                String[] arLstPair;
                foreach (String str1 in arLst)
                {
                    arLstPair = str1.Split('=');
                    qryStr = new QryString(arLstPair[0], arLstPair[1]);
                    this.Add(qryStr);
                }
            }

            public QryStringsCollection(string str, string separator)
            {
                String[] arLst = str.Split(new string[] { separator }, StringSplitOptions.None);
                QryString qryStr;
                String[] arLstPair;
                foreach (String str1 in arLst)
                {
                    arLstPair = str1.Split('=');
                    qryStr = new QryString(arLstPair[0], arLstPair[1]);
                    this.Add(qryStr);
                }
            }

            public void Add(QryString qs)
            {
                this.List.Add(qs);
            }

            public void Remove(QryString qs)
            {
                this.List.Remove(qs);
            }

            public QryString this[int index]
            {
                get { return (QryString)this.List[index]; }
                set { this.List[index] = value; }
            }



            public String this[String str]
            {
                get
                {
                    Int32 iCount = 0;
                    this.Reset();
                    foreach (QryString lstItem in this.List)
                    {
                        if (lstItem.QryStr == str)
                        {
                            return ((QryString)this.List[iCount]).QryStrValue;
                        }
                        iCount++;
                    }
                    return String.Empty; //((QryString)this.List[iCount]).QryStrValue;
                }

            }

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }

            #endregion

            #region IEnumerator Members

            public Object Current
            {
                get
                {
                    return this.List[index];
                }
            }

            public bool MoveNext()
            {
                this.index++;
                return (this.index < this.List.Count);
            }

            public void Reset()
            {
                this.index = -1;
            }

            #endregion
        }

        public static String EncryptQueryString(String Data)
        {
            string strKey = "@pn@rev@m@h@0nl!ne@30442";
            string striv = "REV@54@2";
            byte[] key = Encoding.ASCII.GetBytes(strKey);
            byte[] iv = Encoding.ASCII.GetBytes(striv);
            byte[] data = Encoding.ASCII.GetBytes(Data);
            byte[] enc = new byte[0];
            TripleDES tdes = TripleDES.Create();
            tdes.IV = iv;
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            ICryptoTransform ict = tdes.CreateEncryptor();
            enc = ict.TransformFinalBlock(data, 0, data.Length);
            return ByteArrayToString(enc);
        }

        public static QryStringsCollection DecryptQueryString(String Data)
        {

            string strKey = "@pn@rev@m@h@0nl!ne@30442";
            string striv = "REV@54@2";
            byte[] key = Encoding.ASCII.GetBytes(strKey);
            byte[] iv = Encoding.ASCII.GetBytes(striv);
            byte[] data = StringToByteArray(Data);
            byte[] enc = new byte[0];
            TripleDES tdes = TripleDES.Create();
            tdes.IV = iv;
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            ICryptoTransform ict = tdes.CreateDecryptor();
            enc = ict.TransformFinalBlock(data, 0, data.Length);
            QryStringsCollection qsCollection = new QryStringsCollection(Encoding.ASCII.GetString(enc).TrimEnd('\0'));

            return qsCollection;
        }

        static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        private static string EncryptionKey = "!#853g`de";
        private static byte[] key = System.Text.Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 8));
        private static byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        public static String EncryptQueryString(String Data, int Flag)
        {
            string strKey = "@pn@rev@m@h@0nl!ne@30442";
            string striv = "REV@54@2";
            byte[] key = Encoding.ASCII.GetBytes(strKey);
            byte[] iv = Encoding.ASCII.GetBytes(striv);
            byte[] data = Encoding.ASCII.GetBytes(Data);
            byte[] enc = new byte[0];
            TripleDES tdes = TripleDES.Create();
            tdes.IV = iv;
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            ICryptoTransform ict = tdes.CreateEncryptor();
            enc = ict.TransformFinalBlock(data, 0, data.Length);
            return ByteArrayToString(enc);
        }

        public static QryStringsCollection DecryptQueryString(String Data, string Flag)
        {
            string strKey = "@pn@rev@m@h@0nl!ne@30442";
            string striv = "REV@54@2";
            byte[] key = Encoding.ASCII.GetBytes(strKey);
            byte[] iv = Encoding.ASCII.GetBytes(striv);
            byte[] data = StringToByteArray(Data);
            byte[] enc = new byte[0];
            TripleDES tdes = TripleDES.Create();
            tdes.IV = iv;
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            ICryptoTransform ict = tdes.CreateDecryptor();
            enc = ict.TransformFinalBlock(data, 0, data.Length);
            QryStringsCollection qsCollection = new QryStringsCollection(Encoding.ASCII.GetString(enc).TrimEnd('\0'), Flag);

            return qsCollection;
        }

        public static QryStringsCollection DecryptQueryString(String Data, int Flag)
        {
            string strKey = "@pn@rev@m@h@0nl!ne@30442";
            string striv = "REV@54@2";
            byte[] key = Encoding.ASCII.GetBytes(strKey);
            byte[] iv = Encoding.ASCII.GetBytes(striv);
            byte[] data = StringToByteArray(Data);
            byte[] enc = new byte[0];
            TripleDES tdes = TripleDES.Create();
            tdes.IV = iv;
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            ICryptoTransform ict = tdes.CreateDecryptor();
            enc = ict.TransformFinalBlock(data, 0, data.Length);
            QryStringsCollection qsCollection = new QryStringsCollection(Encoding.ASCII.GetString(enc).TrimEnd('\0'));

            return qsCollection;
        }


        #region Aadhar Encrypt Decrypt

        public static String EncryptAadhaar(String Data)
        {
            string strKey = "@AA@dha@m@r@0nl!ne@30442";
            string striv = "rAa@dh@1";

            byte[] key = Encoding.ASCII.GetBytes(strKey);
            byte[] iv = Encoding.ASCII.GetBytes(striv);
            byte[] data = Encoding.ASCII.GetBytes(Data);
            byte[] enc = new byte[0];
            TripleDES tdes = TripleDES.Create();
            tdes.IV = iv;
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            ICryptoTransform ict = tdes.CreateEncryptor();
            enc = ict.TransformFinalBlock(data, 0, data.Length);
            return ByteArrayToString(enc);
        }

        public static QryStringsCollection DecryptAadhaar(String Data)
        {
            string strKey = "@AA@dha@m@r@0nl!ne@30442";
            string striv = "rAa@dh@1";
            byte[] key = Encoding.ASCII.GetBytes(strKey);
            byte[] iv = Encoding.ASCII.GetBytes(striv);
            byte[] data = StringToByteArray(Data);
            byte[] enc = new byte[0];
            TripleDES tdes = TripleDES.Create();
            tdes.IV = iv;
            tdes.Key = key;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            ICryptoTransform ict = tdes.CreateDecryptor();
            enc = ict.TransformFinalBlock(data, 0, data.Length);
            QryStringsCollection qsCollection = new QryStringsCollection(Encoding.ASCII.GetString(enc).TrimEnd('\0'));

            return qsCollection;
        }

        #endregion
    }
}
