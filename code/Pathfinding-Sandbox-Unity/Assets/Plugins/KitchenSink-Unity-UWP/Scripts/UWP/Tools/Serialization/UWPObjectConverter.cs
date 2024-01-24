using dk.Tools.Debug;
using dk.UWP.Interfaces;

using System;

#if NETFX_CORE

using System.IO;
using System.Runtime.Serialization;
using System.Xml;

#endif

namespace dk.UWP.Tools.Serialization {
    /// <summary>
    /// UWP implementation of ObjectConverter.
    /// </summary>
    public class UWPObjectConverter : IUWPObjectConverterWrapper {
        /// <summary>
        /// Converts the object to byte array.
        /// </summary>
        /// <returns>The object to byte array.</returns>
        /// <param name="obj">Object.</param>
        public override byte[] ConvertObjectToByteArray(System.Object obj) {
            #if NETFX_CORE

            if (obj == null) return default(byte[]);

            using (MemoryStream ms = new MemoryStream()) {
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(ms)) {
                    DataContractSerializer dcs = new DataContractSerializer(typeof(object));
                    dcs.WriteObject(writer, obj);
                    writer.Flush();
                }

                return ms.ToArray();
            }

            #else

            throw new NotImplementedException("This feature is not supported in non-UWP applications.");

            #endif
        }

        /// <summary>
        /// Converts the byte array to object.
        /// </summary>
        /// <returns>The byte array to object.</returns>
        /// <param name="bytes">Bytes.</param>
        public override T ConvertByteArrayToObject<T>(byte[] bytes) {
            #if NETFX_CORE

            if (bytes == null || bytes.Length == 0) return default(T);

            using (MemoryStream ms = new MemoryStream(bytes)) {
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(ms, XmlDictionaryReaderQuotas.Max)) {
                    DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                    return (T)dcs.ReadObject(reader);
                }
            }

            return default(T);

            #else

            throw new NotImplementedException("This feature is not supported in non-UWP applications.");

            #endif
        }
    }
}
