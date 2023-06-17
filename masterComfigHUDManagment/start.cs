using System.ComponentModel;
using System.Diagnostics;

namespace masterComfigHUDManagement
{
    internal class start
    {

        private static void Main()
        {
            Console.Title = "HUDManager";
#if AUTOUPDATE
            Program instance = new Program();
            string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            bool fileExists = File.Exists(documentsFolderPath + "\\HUDManagment\\config.config");

            if (fileExists)
            {
                string[] filePaths = File.ReadAllLines(documentsFolderPath + "\\HUDManagment\\config.config");

                instance.userTf2FolderPath = filePaths[0];
                instance.dbFilePath = filePaths[1];
                instance.tf2args = filePaths[2];

            }
            try
            {

                instance.Update();
                Process.Start(instance.userTf2FolderPath+"\\hl2.exe", "-steam -game tf "+instance.tf2args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
#elif NORMAL
            Program instance = new Program();
            instance.NormalStart();
#endif
        }
    }
}
