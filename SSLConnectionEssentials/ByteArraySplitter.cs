using System;

namespace SSLConnectionEssentials
{
    public static class ByteArraySplitter
    {
        public static byte[][] Split(byte[] arrayToBeSplit, int fixedSize)
        {
            //calculate the length of array
            int arraySize = arrayToBeSplit.Length;


            // get the number of packets by dividing the length with the fixed size of packets
            int chunkCount = arraySize / fixedSize;

            int lastPackageSize = arrayToBeSplit.Length - fixedSize * chunkCount;


            //allocate byte[][]
            if (arrayToBeSplit.Length % fixedSize == 0)
            {
                byte[][] splitArray = new byte[chunkCount][];
                for (int y = 0; y < chunkCount; y++)
                {
                    splitArray[y] = new byte[fixedSize];
                    for (int x = 0; x < fixedSize && y * chunkCount + x < arraySize; x++)
                    {
                        splitArray[y][x] = arrayToBeSplit[y * fixedSize + x]; //self explanatory
                    }
                }

                return splitArray;
            }
            else
            {
                byte[][] splitArray = new byte[chunkCount + 1][];
                for (int y = 0; y < chunkCount + 1; y++)
                {
                    if (y == chunkCount)
                    {
                        splitArray[y] = new byte[lastPackageSize];
                        for (int x = 0; x < lastPackageSize && y * (chunkCount + 1) + x < arraySize; x++)
                        {
                            splitArray[y][x] = arrayToBeSplit[y * fixedSize + x]; //self explanatory
                        }
                    }
                    else
                    {
                        splitArray[y] = new byte[fixedSize];
                        //the second check makes sure the index is not out of bounds
                        for (int x = 0; x < fixedSize && y * (chunkCount + 1) + x < arraySize; x++)
                        {
                            splitArray[y][x] = arrayToBeSplit[y * fixedSize + x]; //self explanatory
                        }
                    }
                }

                return splitArray;
            }
        }
    }
}