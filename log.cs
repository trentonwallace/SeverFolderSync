using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WebFolderSync
{
    class log
    {
        private static string LOG_FILE = Application.StartupPath + "\\log.txt";

        public static void Insert(string logText)
        {   
            //if file is not open /read only
            if (!isFileOpenOrReadOnly(LOG_FILE))
            {
                StreamWriter sw;

                //doesn't exist
                if (!File.Exists(LOG_FILE))
                {
                    //create new one
                    sw = new StreamWriter(LOG_FILE);
                }
                else
                {
                    sw = File.AppendText(LOG_FILE);
                }

                // Write to the file:
                sw.WriteLine(DateTime.Now + " - "+ logText);
                //sw.WriteLine();

                // Close the stream:
                sw.Close();
            }
            else
            {
                LogError.Log("The File : " + LOG_FILE + "." + Environment.NewLine + "Is currently open or ReadOnly" + Environment.NewLine + "Nothing will be logged to the log File");
            }
        }

        private static bool isFileOpenOrReadOnly(string file)
        {
            try
            {
                //first make sure it's not a read only file
                if ((File.GetAttributes(file) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
                {
                    //first we open the file with a FileStream
                    using (FileStream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
                    {
                        try
                        {
                            stream.ReadByte();
                            return false;
                        }
                        catch (IOException)
                        {
                            return true;
                        }
                        finally
                        {
                            stream.Close();
                            stream.Dispose();
                        }
                    }
                }
                else
                    return true;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
