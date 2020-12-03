using Compression;
using System.Text.Json;


namespace Serialization
{
    public static class JsonSerialization
    {
        public static T JsonObjectDeserialize<T>(byte[] serializedInBytes)
        {
            byte[] decompressedByteArray = GZipCompression.GZipDecompress(serializedInBytes);
            var objectReconstructor = new Utf8JsonReader(decompressedByteArray);
            T reconstructedObject = JsonSerializer.Deserialize<T>(ref objectReconstructor);
            return reconstructedObject;
        }

        public static byte[] JsonObjectSerialize<T>(T data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            byte[] fullByteArray = JsonSerializer.SerializeToUtf8Bytes(data, options);
            return GZipCompression.GZipCompress(fullByteArray);
        }
    }
}