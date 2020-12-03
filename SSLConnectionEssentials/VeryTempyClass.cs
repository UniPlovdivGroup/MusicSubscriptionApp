namespace SSLConnectionEssentials
{
    public class AVeryTempClass
    {
        public int NumberSixtyNine { get; set; }
        public string SixtyNineInString { get; set; }

        public long JustForTestButStilSixtyNine { get; set; }

        public AVeryTempClass()
        {

        }
        public AVeryTempClass(int test1, string test2, long test3)
        {
            NumberSixtyNine = test1;
            SixtyNineInString = test2;
            JustForTestButStilSixtyNine = test3;
        }

        public void Dispose()
        {
            SixtyNineInString = null;
        }
    }
}