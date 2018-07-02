using System;

namespace ObjectSerializer
{
    public class ObjectSerializer
    {
        /// <summary>
        /// Function to get object from byte array
        /// </summary>
        /// <param name="_ByteArray">byte array to get object</param>
        /// <returns>object</returns>
        public static object ByteArrayToObject(byte[] _ByteArray)
        {
            try
            {
                // convert byte array to memory stream
                using (System.IO.MemoryStream _MemoryStream = new System.IO.MemoryStream(_ByteArray))
                {
                    // create new BinaryFormatter
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _BinaryFormatter
                                = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    // set memory stream position to starting point
                    _MemoryStream.Position = 0;

                    // Deserializes a stream into an object graph and return as a object.
                    return _BinaryFormatter.Deserialize(_MemoryStream);
                }
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

            // Error occured, return null
            return null;
        }

        /// <summary>
        /// Function to get byte array from a object
        /// </summary>
        /// <param name="_Object">object to get byte array</param>
        /// <returns>Byte Array</returns>
        public static byte[] ObjectToByteArray(object _Object)
        {
            try
            {
                // create new memory stream
                using (System.IO.MemoryStream _MemoryStream = new System.IO.MemoryStream())
                {
                    // create new BinaryFormatter
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _BinaryFormatter
                                = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    // Serializes an object, or graph of connected objects, to the given stream.
                    _BinaryFormatter.Serialize(_MemoryStream, _Object);

                    // convert stream to byte array and return
                    return _MemoryStream.ToArray();
                }
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

            // Error occured, return null
            return null;
        }
    }
}